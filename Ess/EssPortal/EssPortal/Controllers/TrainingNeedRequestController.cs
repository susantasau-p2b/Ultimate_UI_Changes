///
/// Created by Kapil
///
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
using Training;
using Leave;
using System.Diagnostics;
using System.Web.Script.Serialization;
namespace EssPortal.Controllers.Training.MainController
{
    public class TrainingNeedRequestController : Controller
    {
        public ActionResult Partial()
        {
            return View("~/Views/TrainingNeed/Index.cshtml");
        }
        public ActionResult Partial_View()
        {
            return View("~/Views/Shared/_TrainingNeedReqPartial.cshtml");
        }
        public ActionResult Partial_View2()
        {
            return View("~/Views/Shared/_TrainingNeedEmpReqPartial.cshtml");
        }
        public class returnDataClass
        {
            public Int32 EmpId { get; set; }
            public String val { get; set; }
            public Int32 Id2 { get; set; }
            public bool Id3 { get; set; }
            public Int32 LvHead_Id { get; set; }
        }
        [HttpPost]
        public ActionResult GetRecruitExpensesLKDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<TrainingProgramCalendar> all = new List<TrainingProgramCalendar>();
                List<TrainingProgramCalendar> fall = new List<TrainingProgramCalendar>();
                Calendar calendarid = db.Calendar.Where(e => e.Name.LookupVal.ToUpper() == "TRAININGCALENDAR" && e.Default == true).SingleOrDefault();
                if (calendarid != null)
                {
                    var fall1 = db.YearlyTrainingCalendar
                        .Include(e => e.TrainigProgramCalendar)
                        .Include(e => e.TrainingCalendar)
                        .Where(e => e.TrainingCalendar.Id == calendarid.Id)
                        .ToList();

                    foreach (var item in fall1)
                    {

                        if (SkipIds != null)
                        {
                            foreach (var a in SkipIds)
                            {
                                if (item.TrainigProgramCalendar == null)
                                    all = db.TrainingProgramCalendar.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                                else
                                    all = item.TrainigProgramCalendar.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                            }
                        }
                        else
                        {
                            all = item.TrainigProgramCalendar.ToList();
                        }
                        fall.AddRange(all);
                    }

                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = "Start Date:" + ca.StartDate.Value.ToShortDateString() + "," + "End Date:" + ca.EndDate.Value.ToShortDateString() }).Distinct();

                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                return null;


            }
        }

        public ActionResult GetEmpTrainingNeedData(string data)
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
                // var LvHeadId = ids.Count > 0 ? ids[3] : null;

                // var lvheadidint = Convert.ToInt32(LvHeadId);
                var EmpLvIdint = Convert.ToInt32(emplvId);

                var W = db.EmployeeTraining
                    .Include(e => e.Employee.EmpName)
                    .Include(e => e.Employee.GeoStruct)
                    .Include(e => e.Employee.FuncStruct)
                    .Include(e => e.Employee.FuncStruct.Job)
                     .Include(e => e.Employee.GeoStruct.Location)
                     .Include(e => e.Employee.GeoStruct.Location.LocationObj)
                    .Include(e => e.Employee.GeoStruct.Department.DepartmentObj)
                    .Include(e => e.EmpTrainingNeed.Select(t => t.ProgramList))
                    .Include(e => e.EmpTrainingNeed.Select(t => t.ProgramList))
                    .Include(e => e.EmpTrainingNeed.Select(t => t.LvWFDetails))
                    .Where(e => e.Employee.Id == EmpLvIdint && e.EmpTrainingNeed.Any(w => w.Id == id)).SingleOrDefault();

                var v = W.EmpTrainingNeed.Where(e => e.Id == id).Select(s => new EmpmLVdata
                {
                    EmployeeId = W.Employee.Id,
                    EmployeeName = W.Employee.EmpCode + " " + W.Employee.EmpName.FullNameFML,
                    Branch = W.Employee.GeoStruct != null && W.Employee.GeoStruct.Location != null && W.Employee.GeoStruct.Location.LocationObj != null ? W.Employee.GeoStruct.Location.LocationObj.LocDesc.ToString() : null,
                    Department = W.Employee.GeoStruct != null && W.Employee.GeoStruct.Department != null && W.Employee.GeoStruct.Department.DepartmentObj != null ? W.Employee.GeoStruct.Department.DepartmentObj.DeptDesc.ToString() : null,
                    Designation = W.Employee.FuncStruct != null && W.Employee.FuncStruct.Job != null ? W.Employee.FuncStruct.Job.Name : null,
                    Status = status,
                    Id = s.Id,
                    TrClosed = s.TrClosed,
                    ProgrameName = s.ProgramList != null ? s.ProgramList.Subject : null,
                    Req_Date = s.RequisitionDate.Value.ToShortDateString(),
                    EmployeeComment = s.LvWFDetails != null && s.LvWFDetails.Count > 0 ? s.LvWFDetails.Where(z => z.WFStatus == 0).OrderByDescending(e => e.Id).Select(e => e.Comments).FirstOrDefault() : null,
                    SanctionCode = s.LvWFDetails != null && s.LvWFDetails.Count > 0 ? s.LvWFDetails.Where(z => WfStatus.Contains(z.WFStatus)).OrderByDescending(e => e.Id).Select(e => e.DBTrack.CreatedBy).FirstOrDefault() : null,
                    SanctionComment = s.LvWFDetails != null && s.LvWFDetails.Count > 0 ? s.LvWFDetails.Where(z => SanctionStatus.Contains(z.WFStatus)).OrderByDescending(e => e.Id).Select(e => e.Comments).FirstOrDefault() : null,
                    ApporavalComment = s.LvWFDetails != null && s.LvWFDetails.Count > 0 ? s.LvWFDetails.Where(z => ApporvalStatus.Contains(z.WFStatus)).OrderByDescending(e => e.Id).Select(e => e.Comments).FirstOrDefault() : null,
                    Wf = s.LvWFDetails != null && s.LvWFDetails.Count > 0 ? s.LvWFDetails.Where(z => WfStatus.Contains(z.WFStatus)).OrderByDescending(e => e.Id).Select(e => e.Comments).LastOrDefault() : null
                }).SingleOrDefault();


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
                listOfObject.Add(v);
                return Json(listOfObject, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult GetProgramListDetails(List<int> SkipIds, string TRProgid)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<int> forwardataids = null;
                List<TrainingProgramCalendar> fall = new List<TrainingProgramCalendar>();
                if (TRProgid != null)
                {
                    forwardataids = Utility.StringIdsToListIds(TRProgid);
                }
                foreach (var item in forwardataids)
                {

                    fall = db.TrainingProgramCalendar.Include(e => e.ProgramList).Where(e => e.Id == item).ToList();

                    IEnumerable<TrainingProgramCalendar> all;
                    if (SkipIds != null)
                    {
                        foreach (var a in SkipIds)
                        {
                            if (fall == null)
                                fall = db.TrainingProgramCalendar.Include(e => e.ProgramList).Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                        }
                    }
                }

                var r = (from ca in fall select new { srno = ca.ProgramList.Id, lookupvalue = ca.ProgramList.FullDetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);


            }
        }
        [HttpPost]
        public Object Create(Skill lkval, FormCollection form) //Create submit
        {
            lkval.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.EmpId, IsModified = false };

            Skill LookupValue = new Skill
            {
                Id = lkval.Id,
                Name = lkval.Name,
                SkillType = lkval.SkillType,
                FullDetails = lkval.FullDetails,
                DBTrack = lkval.DBTrack
            };
            try
            {
                if (ModelState.IsValid)
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        using (DataBaseContext db = new DataBaseContext())
                        {
                            db.Skill.Add(LookupValue);
                            var a = DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, lkval.DBTrack);
                            db.SaveChanges();
                            DT_Skill DT_LKVal = (DT_Skill)a;
                            DT_LKVal.Orig_Id = LookupValue.Id;
                            db.Create(DT_LKVal);
                            db.SaveChanges();
                            var empid = Convert.ToInt32(SessionManager.EmpId);
                            var EmpAcedemicDataChk = db.Employee.Include(e => e.EmpAcademicInfo)
                                .Include(e => e.EmpAcademicInfo.Skill)
                                .Where(e => e.Id == empid).SingleOrDefault();
                            if (EmpAcedemicDataChk != null && EmpAcedemicDataChk.EmpAcademicInfo != null)
                            {
                                if (EmpAcedemicDataChk.EmpAcademicInfo.Skill != null)
                                {
                                    EmpAcedemicDataChk.EmpAcademicInfo.Skill.Add(LookupValue);
                                }
                                else
                                {
                                    EmpAcedemicDataChk.EmpAcademicInfo.Skill = new List<Skill> { LookupValue };
                                }
                            }
                            else
                            {
                                var oEmpAcademicInfo = new EmpAcademicInfo();
                                oEmpAcademicInfo.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.EmpId, IsModified = false };
                                oEmpAcademicInfo.Skill = new List<Skill> { LookupValue };
                                EmpAcedemicDataChk.EmpAcademicInfo = oEmpAcademicInfo;
                            }
                            db.Entry(EmpAcedemicDataChk).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            ts.Complete();
                            return Json(new { status = true, responseText = "Data Created Successfully." });
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
                    return Json(new { status = false, responseText = errorMsg });
                }

            }
            catch (DataException e) { throw e; }
            catch (DBConcurrencyException e) { throw e; }
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
            public string Comment { get; set; }
            public string Status { get; set; }
        }

        [HttpPost]
        public ActionResult CreateTrainingNeed(EmpTrainingNeed LvReq, FormCollection form, String forwarddata, string TrainingData)
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
            using (DataBaseContext db = new DataBaseContext())
            {
                int EmpId = 0;
                if (Emp != null && Emp != "0" && Emp != "false")
                {
                    EmpId = int.Parse(Emp);
                }
                else
                {
                    return Json(new Object[] { "", "", "Please Select Employee" }, JsonRequestBehavior.AllowGet);
                }
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
                LvReq.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                LvReq.TrainingCalendar = db.Calendar.Where(e => e.Name.LookupVal.ToUpper() == "TRAININGCALENDAR" && e.Default == true).SingleOrDefault();

                var val1 = db.LookupValue.Where(e => e.LookupVal.ToString().ToUpper() == "TRAININGNEED").SingleOrDefault();
                LvReq.EmployeeTrainingSource = val1;
                Employee OEmployee = null;
                EmployeeTraining Oemployeetraining = null;
                Oemployeetraining = db.EmployeeTraining.Include(e => e.EmpTrainingNeed).Include(e => e.Employee)
                .Where(e => e.Employee.Id == EmpId).SingleOrDefault();
                List<EmpTrainingNeed> OFAT = new List<EmpTrainingNeed>();
                LvReq.RequisitionDate = DateTime.Now.Date;
                if (ModelState.IsValid)
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                               new System.TimeSpan(0, 30, 0)))
                    {
                        try
                        {
                            foreach (var item in obj)
                            {
                                if (item.Comment != "")
                                {
                                    LvWFDetails oLvWFDetails = new LvWFDetails
                                    {
                                        WFStatus = 0,
                                        Comments = item.Comment,
                                        DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                                    };
                                    List<LvWFDetails> oLvWFDetails_List = new List<LvWFDetails>();
                                    oLvWFDetails_List.Add(oLvWFDetails);
                                    LvReq.ProgramList = db.ProgramList.Where(e => e.Id == item.Id).SingleOrDefault();
                                    EmpTrainingNeed TrainingNeed = new EmpTrainingNeed()
                                    {
                                        RequisitionDate = LvReq.RequisitionDate,
                                        LvWFDetails = oLvWFDetails_List,
                                        TrClosed = false,
                                        DBTrack = LvReq.DBTrack,
                                        EmployeeTrainingSource = LvReq.EmployeeTrainingSource,
                                        ProgramList = LvReq.ProgramList,
                                        TrainingCalendar = LvReq.TrainingCalendar,
                                        //WFStatus = db.LookupValue.Where(e => e.LookupVal == "1").SingleOrDefault(),
                                        WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "1").SingleOrDefault(),
                                    };

                                    db.EmpTrainingNeed.Add(TrainingNeed);
                                    db.SaveChanges();

                                    OFAT.Add(db.EmpTrainingNeed.Find(TrainingNeed.Id));
                                }
                            }


                            if (Oemployeetraining == null)
                            {
                                EmployeeTraining OTEP = new EmployeeTraining()
                                {
                                    Employee = db.Employee.Find(OEmployee.Id),
                                    EmpTrainingNeed = OFAT,
                                    DBTrack = LvReq.DBTrack
                                };
                                db.EmployeeTraining.Add(OTEP);
                                db.SaveChanges();
                            }
                            else
                            {
                                var aa = db.EmployeeTraining.Find(Oemployeetraining.Id);
                                OFAT.AddRange(aa.EmpTrainingNeed);
                                aa.EmpTrainingNeed = OFAT;
                                db.EmployeeTraining.Attach(aa);
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

                    string LvCancellist = form["IsCancel"] == "0" ? "" : form["IsCancel"];
                    string LvId = form["LvId"] == "0" ? "" : form["LvId"];
                    string Reason = form["ReasonIsCancel"] == "0" ? "" : form["ReasonIsCancel"];
                    var LvCancelchk = false;
                    string Reasoniscancel = "";
                    var LeaveId = 0;
                    if (LvId != null)
                    {
                        LeaveId = Convert.ToInt32(LvId);
                    }
                    else
                    {
                        return Json(new { status = true, responseText = "Try Again" }, JsonRequestBehavior.AllowGet);
                    }
                    if (Reason != "")
                    {
                        Reasoniscancel = Reason;
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

                    int lvnewreqid = Convert.ToInt32(data);

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            try
                            {
                                var qurey = db.EmpTrainingNeed
                                              .Include(e => e.WFStatus)
                                              .Include(e => e.LvWFDetails)
                                             .Where(e => e.Id == lvnewreqid).SingleOrDefault();
                                var lvwfdetails = qurey.LvWFDetails.ToList();

                                foreach (var item in lvwfdetails)
                                {

                                    var LvEP = db.LvWFDetails.Find(item.Id);
                                    db.LvWFDetails.Remove(LvEP);
                                    db.SaveChanges();
                                }

                                var emptrainingneed = db.EmpTrainingNeed.Find(lvnewreqid);
                                db.EmpTrainingNeed.Remove(emptrainingneed);
                                db.SaveChanges();
                                ts.Complete();
                                return Json(new { status = true, responseText = "Data Removed Successfully." }, JsonRequestBehavior.AllowGet);
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
            public string Emp { get; set; }
            public string ReqDate { get; set; }
            public string ProgramName { get; set; }
            public string Status { get; set; }

            public ChildGetLvNewReqClass RowData { get; set; }
        }
        public class GetEmptrainingNeeddata
        {
            public int Id { get; set; }
            public string StartDate { get; set; }
            public string Reqdate { get; set; }
            public string EndDate { get; set; }
            public string ProgramName { get; set; }
            public string Status { get; set; }
            public string Comment { get; set; }
            public string isClose { get; set; }
            public string Employee { get; set; }

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
                                        .Include(e => e.EmpTrainingNeed)
                                        .Include(e => e.EmpTrainingNeed.Select(t => t.ProgramList))
                                        .Include(e => e.EmpTrainingNeed.Select(t => t.TrainingCalendar))
                                        .Where(e => e.Employee.Id == Id).SingleOrDefault();
                Calendar DefaultCalendaryearid = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "TRAININGCALENDAR" && e.Default == true).SingleOrDefault();
                var Appliedprogramlistid = Empprogramlistids.EmpTrainingNeed.Where(e => e.IsCancel != true && e.TrReject != true && e.TrainingCalendar.Id == DefaultCalendaryearid.Id).Select(e => e.ProgramList.Id).ToList();
                var db_data = db.YearlyTrainingCalendar
                      .Include(e => e.TrainingCalendar)
                      .Include(e => e.TrainigProgramCalendar)
                      .Include(e => e.TrainigProgramCalendar.Select(t => t.ProgramList))
                      .Where(e => e.TrainingCalendar.Id == DefaultCalendaryearid.Id)
                     .ToList();

                var filtercurrentdateids = db.TrainingProgramCalendar.Where(e => e.EndDate <= date).Select(e => e.Id).ToList();
                List<GetEmptrainingNeeddata> returndata = new List<GetEmptrainingNeeddata>();

                foreach (var item in db_data)
                {
                    var appliedprogrammeids = item.TrainigProgramCalendar.Where(e => !Appliedprogramlistid.Contains(e.ProgramList.Id) && !filtercurrentdateids.Contains(e.Id)).ToList();
                    foreach (var item1 in appliedprogrammeids)
                    {
                        string programFulldetails = "Subject:" + item1.ProgramList.Subject + ",SubjectDetails:" + item1.ProgramList.SubjectDetails;
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
                            Id = item1.ProgramList == null ? 0 : item1.ProgramList.Id,
                            StartDate = item1.StartDate.Value.ToShortDateString(),
                            EndDate = item1.EndDate.Value.ToShortDateString(),
                            ProgramName = programFulldetails,
                            Comment = ""
                        });
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
                    FuncModule = db.LookupValue.Where(e => e.LookupVal.ToUpper() == id.ToUpper()).FirstOrDefault();
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


                                if (item1.SubModuleName == null || item1.SubModuleName == "")
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
        public ActionResult UpdateStatus(LvNewReq LvReq, FormCollection form, String data, string TrainingData)
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
            string authority = form["authority"] == null ? null : form["authority"];
            var isClose = form["isClose"] == null ? null : form["isClose"];
            if (authority == null && isClose == null)
            {
                return Json(new { status = false }, JsonRequestBehavior.AllowGet);
            }
            //var ids = Utility.StringIdsToListString(data);
            //var lvnewreqid = Convert.ToInt32(ids[0]);
            //var EmpLvId = Convert.ToInt32(ids[1]);
            //string Sanction = form["Sanction"];
            //string ReasonSanction = form["ReasonSanction"];
            //string HR = form["HR"] == null ? null : form["HR"];
            //string MySelf = form["MySelf"] == null ? "false" : form["MySelf"];
            //string ReasonMySelf = form["ReasonMySelf"] == null ? null : form["ReasonMySelf"];
            //string Approval = form["Approval"];
            //string ReasonApproval = form["ReasonApproval"];
            //string ReasonHr = form["ReasonHr"] == null ? null : form["ReasonHr"];
            bool SanctionRejected = false;
            bool HrRejected = false;
            List<LvWFDetails> oLvWFDetails_List = new List<LvWFDetails>();

            //bool self = false;
            using (DataBaseContext db = new DataBaseContext())
            {
                foreach (var item in obj)
                {
                    if (item.Status != "" && item.Comment != "")
                    {

                        var qurey = db.EmpTrainingNeed
                            .Include(e => e.WFStatus)
                            .Include(e => e.LvWFDetails)
                            .Where(e => e.Id == item.Id).SingleOrDefault();

                        // var LvCalendar = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "LEAVECALENDAR" && e.Default == true).SingleOrDefault();
                        if (authority.ToUpper() == "MYSELF")
                        {
                            qurey.IsCancel = true;
                            qurey.TrClosed = true;
                            //qurey.WFStatus = db.LookupValue.Where(e => e.LookupVal == "6").SingleOrDefault();
                            qurey.WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "6").FirstOrDefault();
                        }
                        if (authority.ToUpper() == "SANCTION")
                        {

                            //if (item.Status == "")
                            //{
                            //    return Json(new Utility.JsonClass { status = false, responseText = "Please Select The Sanction Status" }, JsonRequestBehavior.AllowGet);
                            //}
                            //if (item.Comment == "")
                            //{
                            //    return Json(new Utility.JsonClass { status = false, responseText = "Please Enter The Reason" }, JsonRequestBehavior.AllowGet);
                            //}
                            if (item.Status.ToUpper() == "YES")
                            {
                                //sanction yes -1
                                var LvWFDetails = new LvWFDetails
                                {
                                    WFStatus = 1,
                                    Comments = item.Comment,
                                    DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName }
                                };
                                qurey.LvWFDetails.Add(LvWFDetails);
                               // qurey.WFStatus = db.LookupValue.Where(e => e.LookupVal == "1").SingleOrDefault();
                                qurey.WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "1").FirstOrDefault();
                                qurey.TrClosed = Convert.ToBoolean(isClose) == true ? true : false;
                            }
                            else if (item.Status.ToUpper() == "NO")
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
                                    Comments = item.Comment,
                                    DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                                };
                                oLvWFDetails_List.Add(oLvWFDetails);
                                //qurey.WFStatus = db.LookupValue.Where(e => e.LookupVal == "2").SingleOrDefault();
                                qurey.WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "2").FirstOrDefault();
                                qurey.TrClosed = true;
                                SanctionRejected = true;
                            }
                        }
                        else if (authority.ToUpper() == "APPROVAL")//Hr
                        {
                            //if (item.Status == "")
                            //{
                            //    return Json(new Utility.JsonClass { status = false, responseText = "Please Select The Approval Status" }, JsonRequestBehavior.AllowGet);
                            //}
                            //if (item.Comment == "")
                            //{
                            //    return Json(new Utility.JsonClass { status = false, responseText = "Please Enter The Reason" }, JsonRequestBehavior.AllowGet);
                            //}
                            if (item.Status.ToUpper() == "YES")
                            {
                                //approval yes-3
                                var LvWFDetails = new LvWFDetails
                                {
                                    WFStatus = 3,
                                    Comments = item.Comment,
                                    DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName }
                                };
                                qurey.TrClosed = Convert.ToBoolean(isClose) == true ? true : false;
                                qurey.LvWFDetails.Add(LvWFDetails);
                                //qurey.WFStatus = db.LookupValue.Where(e => e.LookupVal == "3").SingleOrDefault();
                                qurey.WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "3").FirstOrDefault();
                            }
                            else if (item.Status.ToUpper() == "NOs")
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
                                    Comments = item.Comment,
                                    DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                                };
                                oLvWFDetails_List.Add(oLvWFDetails);
                                //qurey.WFStatus = db.LookupValue.Where(e => e.LookupVal == "4").SingleOrDefault();
                                qurey.WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "4").FirstOrDefault();
                                qurey.TrClosed = true;
                                HrRejected = true;
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
                                    qurey.TrReject = true;
                                }
                                oLvWFDetails_List.AddRange(qurey.LvWFDetails);
                                qurey.LvWFDetails = oLvWFDetails_List;
                                db.EmpTrainingNeed.Attach(qurey);
                                db.Entry(qurey).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(qurey).State = System.Data.Entity.EntityState.Detached;
                                ts.Complete();
                            }
                        }
                        catch (Exception e)
                        {
                            return Json(new Utility.JsonClass { status = false, responseText = e.InnerException.Message.ToString() }, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
                return Json(new Utility.JsonClass { status = true, responseText = "Record Updated..!" }, JsonRequestBehavior.AllowGet);

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
        public ActionResult GetEmpInvestmentHistory()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                LookupValue FuncModule = new LookupValue();
                string AccessRight = "";
                if (!string.IsNullOrEmpty(Convert.ToString(Session["user-module"])))
                {
                    var id = Convert.ToString(Session["user-module"]);
                    FuncModule = db.LookupValue.Where(e => e.LookupVal.ToUpper() == id.ToUpper()).FirstOrDefault();
                }
                if (!string.IsNullOrEmpty(Convert.ToString(Session["user-module"])))
                {
                    AccessRight = Convert.ToString(Session["auho"]);
                }
                if (FuncModule == null)
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                //var EmpIds = UserManager.GeEmployeeList(FuncModule.Id, 0);
                //if (EmpIds == null && EmpIds.Count == 0)
                //{
                //    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                //}
                List<EmployeeTraining> Emps = new List<EmployeeTraining>();
                List<EssPortal.Process.PortalRights.AccessRightsData> EmpidsWithfunsub = UserManager.GeEmployeeListWithfunsub(FuncModule.Id, 0, AccessRight);
                foreach (var item in EmpidsWithfunsub)
                {


                    var Empid = db.EmployeeTraining
                        .Include(e => e.Employee)
                         .Include(e => e.Employee.EmpName)
                        .Include(e => e.EmpTrainingNeed.Select(a => a.ProgramList))
                        .Include(e => e.EmpTrainingNeed.Select(a => a.WFStatus))
                        .Include(e => e.EmpTrainingNeed.Select(a => a.LvWFDetails));
                    //.Where(e => EmpIds.Contains(e.Employee.Id)&& e.ITInvestmentPayment !=null).ToList();

                    Emps = Empid.Where(e => item.ReportingEmployee.Contains(e.Employee.Id)).AsNoTracking().ToList();
                }

                var allLvHead = db.ProgramList.ToList();
                List<EmpLvClass> ListEmpLvClass = new List<EmpLvClass>();

                foreach (var ca in Emps)
                {
                    var item1 = ca.EmpTrainingNeed.Where(e => e.ProgramList != null).ToList();
                    var oEmpLvClass = new EmpLvClass();
                    //foreach (var item2 in item1)
                    //{
                    //if (item2.ITInvestment != null)
                    //{
                    foreach (var lvhead in allLvHead)
                    {
                        var temp = new List<tempClass>();
                        var LvData = item1.Where(e => e.ProgramList.Id == lvhead.Id).OrderByDescending(e => e.RequisitionDate).ToList();

                        foreach (var item in LvData)
                        {
                            var Status = "--";
                            if (item.LvWFDetails.Count() > 0)
                            {
                                Status = Utility.GetStatusName().Where(e => e.Key == item.LvWFDetails.OrderBy(t => t.Id).LastOrDefault().WFStatus.ToString())
                                .Select(e => e.Value).SingleOrDefault();
                            }

                            //if (item.InputMethod == 0)
                            //{
                            //    Status = "Approved By HRM (M)";
                            //}
                            if (item.ProgramList != null)
                            {
                                temp.Add(new tempClass
                                {
                                    LvName = item.ProgramList.Subject,
                                    LvCode = item.Id.ToString(),
                                    LvBal = "Requisition Date:" + item.RequisitionDate.Value.ToShortDateString() + "Programe Details :" + item.ProgramList.FullDetails + " Status :" + Status,
                                });
                            }

                            if (LvData != null && LvData.Count > 0)
                            {
                                oEmpLvClass.EmpName = ca.Employee.EmpCode + " " + ca.Employee.EmpName.FullNameFML;
                                if (oEmpLvClass.LvHeadName == null)
                                {
                                    oEmpLvClass.LvHeadName = new List<ReqLvHeadWise>{new ReqLvHeadWise {
                                            LvHeadName=temp.Select(e=>e.LvName).FirstOrDefault().ToString(),
                                            LvHeadCode=temp.Select(e=>e.LvCode).FirstOrDefault().ToString(),
                                            LvHeadBal=temp.Select(e=>e.LvBal).FirstOrDefault().ToString()
                                        }};
                                }
                                else
                                {
                                    foreach (var ttt in temp)
                                    {
                                        oEmpLvClass.LvHeadName.Add(new ReqLvHeadWise
                                        {
                                            LvHeadName = ttt.LvName,
                                            LvHeadCode = ttt.LvCode,
                                            LvHeadBal = ttt.LvBal
                                        });
                                    }
                                }

                            }
                        }
                    }
                    if (oEmpLvClass.EmpName != null)
                    {
                        ListEmpLvClass.Add(oEmpLvClass);
                    }
                    //  }
                    // }
                }
                return Json(new Utility.JsonClass { status = true, Data = ListEmpLvClass }, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult GetMyTrainingNeed()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (string.IsNullOrEmpty(Convert.ToString(SessionManager.EmpLvId)))
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                var Id = Convert.ToInt32(SessionManager.EmpLvId);
                var db_data = db.EmployeeTraining
                      .Where(e => e.Id == Id)
                      .Include(e => e.EmpTrainingNeed)
                      .Include(e => e.EmpTrainingNeed.Select(a => a.ProgramList))
                      .Include(e => e.EmpTrainingNeed.Select(a => a.WFStatus))
                      .Include(e => e.EmpTrainingNeed.Select(a => a.LvWFDetails))
                     .SingleOrDefault();

                // var LvCalendar = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "LEAVECALENDAR" && e.Default == true).SingleOrDefault();


                //if (listLvs != null && listLvs.Count() > 0)
                //{
                List<GetLvNewReqClass> returndata = new List<GetLvNewReqClass>();
                returndata.Add(new GetLvNewReqClass
                {
                    ReqDate = "Requisition Date",
                    ProgramName = "Program Name",
                    Status = "Status"
                });
                //List<int> lvcode = listLvs.Where(e => e.LvCreditDate != null).Select(e => e.LeaveHead.Id).Distinct().ToList();
                //foreach (var item1 in lvcode)
                //{
                // DateTime? lvcrdate = listLvs.Where(a => a.LeaveHead.Id == item1 && a.LvCreditDate != null).Max(a => a.LvCreditDate.Value);
                //if (lvcrdate != null)
                //{

                foreach (var item in db_data.EmpTrainingNeed)
                {

                    var Status = "--";
                    if (item.LvWFDetails.Count > 0)
                    {
                        Status = Utility.GetStatusName().Where(e => e.Key == item.LvWFDetails.LastOrDefault().WFStatus.ToString())
                        .Select(e => e.Value).SingleOrDefault();
                    }
                    else
                    {
                        Status = "Approved By HRM (M)";
                    }


                    returndata.Add(new GetLvNewReqClass
                    {
                        RowData = new ChildGetLvNewReqClass
                        {
                            LvNewReq = item.Id.ToString(),
                            EmpLVid = db_data.Id.ToString(),
                            IsClose = Status,
                            Status = Status,
                            // LvHead_Id = item.LeaveHead.Id.ToString(),
                        },
                        ReqDate = item.RequisitionDate.Value.ToShortDateString(),
                        ProgramName = item.ProgramList.Subject,
                        Status = Status
                    });
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

            public string Status { get; set; }
            public string Emplvhead { get; set; }
            public string Req_Date { get; set; }
            public string Branch { get; set; }
            public string Department { get; set; }
            public string Designation { get; set; }
            public string SanctionCode { get; set; }
            public string SanctionEmpname { get; set; }
            public string SanctionComment { get; set; }
            public string ApporavalComment { get; set; }
            public string EmployeeComment { get; set; }
            public string ProgrameName { get; set; }
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
                string AccessRight = "";
                string funmodule = "";
                if (!string.IsNullOrEmpty(Convert.ToString(Session["user-module"])))
                {
                    funmodule = Convert.ToString(Session["user-module"]);
                    //FuncModule = db.LookupValue.Where(e => e.LookupVal.ToUpper() == id.ToUpper()).SingleOrDefault();
                }
                if (!string.IsNullOrEmpty(Convert.ToString(Session["user-module"])))
                {
                    AccessRight = Convert.ToString(Session["auho"]);
                }
                var ids = Utility.StringIdsToListString(data);
                // var id = Convert.ToInt32(ids[0]);
                // var status = ids.Count > 0 ? ids[2] : null;
                var emplvId = ids.Count > 0 ? ids[1] : null;
                // var LvHeadId = ids.Count > 0 ? ids[3] : null;

                // var lvheadidint = Convert.ToInt32(LvHeadId);
                var EmpLvIdint = Convert.ToInt32(emplvId);

                var Empprogramlistids = db.EmployeeTraining
                                       .Include(e => e.Employee)
                                       .Include(e => e.Employee.EmpName)
                                       .Include(e => e.Employee.ReportingStructRights.Select(a => a.AccessRights.ActionName))
                                       .Include(e => e.Employee.ReportingStructRights.Select(a => a.FuncModules))
                                       .Include(e => e.EmpTrainingNeed)
                                       .Include(e => e.EmpTrainingNeed.Select(t => t.ProgramList))
                                       .Include(e => e.EmpTrainingNeed.Select(t => t.LvWFDetails))
                                       .Include(e => e.EmpTrainingNeed.Select(t => t.TrainingCalendar))
                                       .Where(e => e.Employee.Id == EmpLvIdint).SingleOrDefault();


                var LvIds = UserManager.FilterTrainingNeed(Empprogramlistids.EmpTrainingNeed.ToList(),
                           Convert.ToString(Session["auho"]), Convert.ToInt32(SessionManager.CompanyId));

                // Calendar DefaultCalendaryearid = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "TRAININGCALENDAR" && e.Default == true).SingleOrDefault();
                var Appliedprogramlistid = Empprogramlistids.EmpTrainingNeed.Where(e => LvIds.Contains(e.Id)).ToList();
                //var db_data = db.YearlyTrainingCalendar
                //      .Include(e => e.TrainingCalendar)
                //      .Include(e => e.TrainigProgramCalendar)
                //      .Include(e => e.TrainigProgramCalendar.Select(t => t.ProgramList))
                //      .Where(e => e.TrainingCalendar.Id == DefaultCalendaryearid.Id)
                //     .ToList();

                //var filtercurrentdateids = db.TrainingProgramCalendar.Where(e => e.EndDate <= date).Select(e => e.Id).ToList();
                List<GetEmptrainingNeeddata> returndata = new List<GetEmptrainingNeeddata>();

                foreach (var item in Appliedprogramlistid)
                {
                    // var appliedprogrammeids = item..Where(e => Appliedprogramlistid.Contains(e.ProgramList.Id)).ToList();
                    //foreach (var item1 in appliedprogrammeids)
                    //{
                    // string programFulldetails = "Subject:" + item1.ProgramList.Subject + ",SubjectDetails:" + item1.ProgramList.SubjectDetails;
                    returndata.Add(new GetEmptrainingNeeddata
                    {
                        //RowData = new ChildGetLvNewReqClass
                        //{
                        //    // LvNewReq = item.Id.ToString(),
                        //    //EmpLVid = db_data.Employee.Id.ToString(),
                        //   // IsClose = Empprogramlistids.Employee.ReportingStructRights.Where(a => a.AccessRights != null && a.AccessRights.ActionName.LookupVal.ToUpper() == AccessRight.ToUpper() && a.FuncModules.LookupVal.ToUpper() == funmodule.ToUpper())
                        //                           //.Select(a => a.AccessRights.IsClose).FirstOrDefault().ToString(),
                        //    //Status = Status,
                        //    // LvHead_Id = item.LeaveHead.Id.ToString(),
                        //},
                        Employee = Empprogramlistids.Employee.EmpCode + "," + Empprogramlistids.Employee.EmpName.FullNameFML,
                        Id = item.Id,
                        Reqdate = item.RequisitionDate.Value.ToShortDateString(),
                        ProgramName = item.ProgramList == null ? "" : item.ProgramList.FullDetails,
                        Status = "",
                        Comment = "",
                        isClose = Empprogramlistids.Employee.ReportingStructRights.Where(a => a.AccessRights != null && a.AccessRights.ActionName.LookupVal.ToUpper() == AccessRight.ToUpper() && a.FuncModules.LookupVal.ToUpper() == funmodule.ToUpper())
                                                   .Select(a => a.AccessRights.IsClose).FirstOrDefault().ToString()
                    });
                    // }
                }
                return Json(new { status = true, data = returndata }, JsonRequestBehavior.AllowGet);



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
                ////if Emp Bal updated
                //var listOfObject = new List<dynamic>();
                //listOfObject.Add(v);
                //return Json(listOfObject, JsonRequestBehavior.AllowGet);
            }
        }

        //public ActionResult Edit1(string data)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var ids = Utility.StringIdsToListString(data);
        //        var id = Convert.ToInt32(ids[0]);
        //        var Emp = Convert.ToInt32(SessionManager.EmpId);
        //        var qurey = db.Employee.Include(e => e.EmpAcademicInfo)
        //            .Include(e => e.EmpAcademicInfo.Skill)
        //            .Include(e => e.EmpAcademicInfo.Skill.Select(a => a.SkillType))
        //            .Where(e => e.Id == Emp && e.EmpAcademicInfo != null && e.EmpAcademicInfo.Skill.Any(a => a.Id == id))
        //            .AsEnumerable().Select(e => new
        //            {
        //                id = qurey.Id,
        //                FamilyDetails = e.EmpAcademicInfo.Skill.Where(a => a.Id == id).SingleOrDefault(),
        //            }).SingleOrDefault();
        //        var listofdata = qurey != null && qurey.FamilyDetails != null ? qurey.FamilyDetails : null;
        //      //  return Json(new Object[] { qurey, listofdata, JsonRequestBehavior.AllowGet });
        //        return Json(new Object[] { qurey, listofdata, "", "", JsonRequestBehavior.AllowGet });


        //    }
        //}

        public ActionResult Edit(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Emp = Convert.ToInt32(SessionManager.EmpId);
                var ids = Utility.StringIdsToListString(data);
                var id = Convert.ToInt32(ids[0]);
                var qurey = db.Employee.Include(e => e.EmpAcademicInfo)
                    .Include(e => e.EmpAcademicInfo.Skill)

                    .Where(e => e.Id == Emp && e.EmpAcademicInfo != null)
                    .AsEnumerable().Select(e => new
                    {
                        Skill = e.EmpAcademicInfo.Skill.Where(w => w.Id == id).SingleOrDefault(),
                        DBTrack = e.DBTrack
                    }).SingleOrDefault();

                var returndata = (Object)null;
                var returnCurrentData = (Object)null;
                if (qurey != null)
                {
                    if (qurey.Skill != null)
                    {
                        returndata = new
                        {
                            id = qurey.Skill.Id,
                            Name = qurey.Skill.Name,
                            Action = qurey.Skill.DBTrack.Action,
                            isauth = true,
                            Add = false
                        };

                        //curr data
                        var dt_data = db.DT_Skill.Where(e => e.Orig_Id == qurey.Skill.Id && e.DBTrack.IsAuthorized == 0).OrderByDescending(e => e.Id).FirstOrDefault();
                        if (dt_data != null)
                        {
                            returnCurrentData = new
                            {
                                Action = qurey.DBTrack.Action,
                            };
                        }
                    }
                    else
                    {
                        returndata = new
                        {
                            Add = true,
                        };
                    }

                    return Json(new Object[] { returndata, returnCurrentData, "", JsonRequestBehavior.AllowGet });
                }
                return Json(new Object[] { returndata, returnCurrentData, "", JsonRequestBehavior.AllowGet });
            }
        }


        public int EditS(int data, Skill c, DBTrack dbT)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var CurCorp = db.Skill.Find(data);
                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    c.DBTrack = dbT;
                    Skill corp = new Skill()
                    {
                        Name = c.Name,
                        Id = data,
                        DBTrack = c.DBTrack
                    };

                    db.Skill.Attach(corp);
                    db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                    return 1;
                }
                return 0;
            }
        }



        [HttpPost]
        public Object EditSave(Skill c, int data, FormCollection form) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {


                // bool Auth = form["autho_allow"] == "true" ? true : false;
                bool Auth = true;


                if (Auth == false)
                {
                    if (ModelState.IsValid)
                    {
                        try
                        {
                            var db_data = db.Skill.Include(e => e.SkillType)
                                                   .Where(e => e.Id == data).SingleOrDefault();


                            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                            {
                                db.Skill.Attach(db_data);
                                db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                TempData["RowVersion"] = db_data.RowVersion;
                                db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;

                                var Curr_Lookup = db.Skill.Find(data);
                                TempData["CurrRowVersion"] = Curr_Lookup.RowVersion;
                                db.Entry(Curr_Lookup).State = System.Data.Entity.EntityState.Detached;

                                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                {

                                    Skill blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.Skill.Include(e => e.SkillType)
                                                   .Where(e => e.Id == data).SingleOrDefault();

                                        originalBlogValues = context.Entry(blog).OriginalValues;
                                    }

                                    c.DBTrack = new DBTrack
                                    {
                                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = "0029",
                                        ModifiedOn = DateTime.Now
                                    };
                                    Skill lk = new Skill
                                    {
                                        Id = data,
                                        Name = c.Name,

                                        DBTrack = c.DBTrack,
                                        FullDetails = c.FullDetails
                                    };


                                    db.Skill.Attach(lk);
                                    db.Entry(lk).State = System.Data.Entity.EntityState.Modified;

                                    db.Entry(lk).OriginalValues["RowVersion"] = TempData["RowVersion"];


                                    using (var context = new DataBaseContext())
                                    {

                                        var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                        DT_Skill DT_Corp = (DT_Skill)obj;

                                        db.Create(DT_Corp);
                                        db.SaveChanges();
                                    }
                                    //  await db.SaveChangesAsync();
                                    ts.Complete();


                                    return new { status = true, responseText = "Record Updated" };
                                }
                            }
                        }
                        catch (DbUpdateConcurrencyException ex)
                        {
                            var entry = ex.Entries.Single();
                            var clientValues = (Skill)entry.Entity;
                            var databaseEntry = entry.GetDatabaseValues();
                            if (databaseEntry == null)
                            {
                                return new { status = true, responseText = "Unable to save changes. The record was deleted by another user." };
                            }
                            else
                            {
                                var databaseValues = (Skill)databaseEntry.ToObject();
                                c.RowVersion = databaseValues.RowVersion;

                            }
                        }

                        return new { status = true, responseText = "Record modified by another user.So refresh it and try to save again." };
                    }
                }
                else
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {

                        Skill blog = null; // to retrieve old data
                        DbPropertyValues originalBlogValues = null;
                        Skill Old_Corp = null;

                        using (var context = new DataBaseContext())
                        {
                            blog = context.Skill.Where(e => e.Id == data).SingleOrDefault();
                            originalBlogValues = context.Entry(blog).OriginalValues;
                        }
                        c.DBTrack = new DBTrack
                        {
                            CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                            CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                            Action = "M",
                            IsModified = blog.DBTrack.IsModified == true ? true : false,
                            ModifiedBy = "0029",
                            ModifiedOn = DateTime.Now
                        };
                        Skill qualificationDetails = new Skill()
                        {

                            Id = data,

                            DBTrack = c.DBTrack,
                            RowVersion = (Byte[])TempData["RowVersion"]
                        };

                        using (var context = new DataBaseContext())
                        {
                            var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, qualificationDetails, "Skill", c.DBTrack);
                            Old_Corp = context.Skill.Where(e => e.Id == data)
                              .SingleOrDefault();
                            DT_Skill DT_Corp = (DT_Skill)obj;
                            db.Create(DT_Corp);
                        }
                        blog.DBTrack = c.DBTrack;
                        db.Skill.Attach(blog);
                        db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                        // db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                        db.SaveChanges();
                        ts.Complete();
                        return new { status = true, responseText = "Record Updated" };
                    }
                }
                return new Object[] { };
            }
        }


        //[HttpPost]
        //public Object  EditSave1(Skill c, int data, FormCollection form) // Edit submit
        //{
        //    //  bool Auth = form["autho_action"] == "" ? false : true;
        //    bool Auth = form["autho_allow"] == "true" ? true : false;
        //    using (DataBaseContext db = new DataBaseContext())
        //    {

        //        if (Auth == false)
        //        {
        //            if (ModelState.IsValid)
        //            {
        //                try
        //                {

        //                    //DbContextTransaction transaction = db.Database.BeginTransaction();

        //                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                    {
        //                        Skill blog = null; // to retrieve old data
        //                        // DbPropertyValues originalBlogValues = null;

        //                        using (var context = new DataBaseContext())
        //                        {
        //                            blog = context.Skill.Where(e => e.Id == data)
        //                                                    .AsNoTracking().SingleOrDefault();
        //                            // originalBlogValues = context.Entry(blog).OriginalValues;
        //                        }

        //                        c.DBTrack = new DBTrack
        //                        {
        //                            CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                            CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                            Action = "M",
        //                            ModifiedBy = "0029",
        //                            ModifiedOn = DateTime.Now
        //                        };

        //                        int a = EditS(data, c, c.DBTrack);

        //                        await db.SaveChangesAsync();

        //                        using (var context = new DataBaseContext())
        //                        {

        //                            //To save data in history table 
        //                            var Obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, c, "Skill", c.DBTrack);
        //                            DT_Skill DT_Corp = (DT_Skill)Obj;
        //                            db.DT_Skill.Add(DT_Corp);
        //                            db.SaveChanges();
        //                        }

        //                        ts.Complete();


        //                       // return Json(new Object[] { status = true, responseText = "Record Updated", JsonRequestBehavior.AllowGet });
        //                        return Json(new { status = true, responseText = "Record Updated", JsonRequestBehavior.AllowGet });
        //                    }
        //                }
        //                catch (DbUpdateConcurrencyException ex)
        //                {
        //                    var entry = ex.Entries.Single();
        //                    var clientValues = (Skill)entry.Entity;
        //                    var databaseEntry = entry.GetDatabaseValues();
        //                    if (databaseEntry == null)
        //                    {
        //                        return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
        //                    }
        //                    else
        //                    {
        //                        var databaseValues = (Skill)databaseEntry.ToObject();
        //                        c.RowVersion = databaseValues.RowVersion;

        //                    }
        //                }

        //                return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
        //            }
        //        }
        //        else
        //        {
        //            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //            {
        //                Skill Old_Corp = db.Skill.Where(e => e.Id == data).SingleOrDefault();

        //                Skill Curr_Corp = c;
        //                c.DBTrack = new DBTrack
        //                {
        //                    CreatedBy = Old_Corp.DBTrack.CreatedBy == null ? null : Old_Corp.DBTrack.CreatedBy,
        //                    CreatedOn = Old_Corp.DBTrack.CreatedOn == null ? null : Old_Corp.DBTrack.CreatedOn,
        //                    Action = "M",
        //                    IsModified = Old_Corp.DBTrack.IsModified == true ? true : false,
        //                    //ModifiedBy = "0029",
        //                    //ModifiedOn = DateTime.Now
        //                };
        //                Old_Corp.DBTrack = c.DBTrack;

        //                db.Entry(Old_Corp).State = System.Data.Entity.EntityState.Modified;
        //                db.SaveChanges();
        //                using (var context = new DataBaseContext())
        //                {
        //                    //DBTrackFile.DBTrackSave("Core/P2b.Global", "M", Old_Corp, Curr_Corp, "Skill", c.DBTrack);
        //                }

        //                ts.Complete();
        //                return Json(new Object[] { Old_Corp.Id, c.Name, "Record Updated", JsonRequestBehavior.AllowGet });
        //            }

        //        }
        //        return View();

        //    }
        //}

        private int EditS(int data, Hobby c, DBTrack dBTrack)
        {
            throw new NotImplementedException();
        }
        public ActionResult AddOrEdit(Skill lkval, FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Add = form["Add"] != null && form["Add"] != "" ? Convert.ToBoolean(form["Add"]) : true;
                var Id = form["auth_id"] != null && form["auth_id"] != "" ? Convert.ToInt32(form["auth_id"]) : 0;
                if (Add == true)
                {
                    //Add
                    var returnobj = Create(lkval, form);
                    return Json(returnobj, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    //Edit
                    var returnobj = EditSave(lkval, Id, form);
                    return Json(returnobj, JsonRequestBehavior.AllowGet);
                }
            }
        }
        public async Task<ActionResult> AuthSave(int auth_id, Boolean isauth, String auth_action) //to save authorised data
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (auth_action == "C")
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {

                        Skill corp = db.Skill.FirstOrDefault(e => e.Id == auth_id);

                        corp.DBTrack = new DBTrack
                        {
                            Action = "C",
                            ModifiedBy = corp.DBTrack.ModifiedBy != null ? corp.DBTrack.ModifiedBy : null,
                            CreatedBy = corp.DBTrack.CreatedBy != null ? corp.DBTrack.CreatedBy : null,
                            CreatedOn = corp.DBTrack.CreatedOn != null ? corp.DBTrack.CreatedOn : null,
                            IsModified = corp.DBTrack.IsModified == true ? false : false,
                            AuthorizedBy = "0029",
                            AuthorizedOn = DateTime.Now
                        };

                        db.Skill.Attach(corp);
                        db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                        db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                        //db.SaveChanges();

                        await db.SaveChangesAsync();


                        using (var context = new DataBaseContext())
                        {

                            ////DBTrackFile.DBTrackSave("Core/P2b.Global", "M", corp, null, "Skill", corp.DBTrack);
                        }

                        ts.Complete();
                        return Json(new Object[] { corp.Id, corp.Name, "Record Authorised", JsonRequestBehavior.AllowGet });
                    }
                }
                else if (auth_action == "M")
                {

                    Skill Old_Corp = db.Skill.Where(e => e.Id == auth_id).SingleOrDefault();


                    DT_Skill Curr_Corp = db.DT_Skill
                                                .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                                                .OrderByDescending(e => e.Id)
                                                .FirstOrDefault();

                    Skill corp_hobby = new Skill();


                    corp_hobby.Name = Curr_Corp.Name == null ? Old_Corp.Name : Curr_Corp.Name;
                    //corp_hobby.Id = auth_id;

                    if (ModelState.IsValid)
                    {
                        try
                        {

                            //DbContextTransaction transaction = db.Database.BeginTransaction();

                            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                            {
                                // db.Configuration.AutoDetectChangesEnabled = false;
                                corp_hobby.DBTrack = new DBTrack
                                {
                                    CreatedBy = Old_Corp.DBTrack.CreatedBy == null ? null : Old_Corp.DBTrack.CreatedBy,
                                    CreatedOn = Old_Corp.DBTrack.CreatedOn == null ? null : Old_Corp.DBTrack.CreatedOn,
                                    Action = "M",
                                    ModifiedBy = Old_Corp.DBTrack.ModifiedBy == null ? null : Old_Corp.DBTrack.ModifiedBy,
                                    ModifiedOn = Old_Corp.DBTrack.ModifiedOn == null ? null : Old_Corp.DBTrack.ModifiedOn,
                                    AuthorizedBy = "0029",
                                    AuthorizedOn = DateTime.Now,
                                    IsModified = false
                                };

                                int a = EditS(auth_id, corp_hobby, corp_hobby.DBTrack);

                                await db.SaveChangesAsync();

                                ts.Complete();
                                return Json(new Object[] { corp_hobby.Id, corp_hobby.Name, "Record Updated", JsonRequestBehavior.AllowGet });
                            }
                        }
                        catch (DbUpdateConcurrencyException ex)
                        {
                            var entry = ex.Entries.Single();
                            var clientValues = (Skill)entry.Entity;
                            var databaseEntry = entry.GetDatabaseValues();
                            if (databaseEntry == null)
                            {
                                return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                            }
                            else
                            {
                                var databaseValues = (Hobby)databaseEntry.ToObject();
                                corp_hobby.RowVersion = databaseValues.RowVersion;
                            }
                        }

                        return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                    }
                }
                else if (auth_action == "D")
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        //Corporate corp = db.Corporate.Find(auth_id);
                        Skill corp = db.Skill.AsNoTracking().FirstOrDefault(e => e.Id == auth_id);

                        corp.DBTrack = new DBTrack
                        {
                            Action = "D",
                            ModifiedBy = corp.DBTrack.ModifiedBy != null ? corp.DBTrack.ModifiedBy : null,
                            CreatedBy = corp.DBTrack.CreatedBy != null ? corp.DBTrack.CreatedBy : null,
                            CreatedOn = corp.DBTrack.CreatedOn != null ? corp.DBTrack.CreatedOn : null,
                            IsModified = corp.DBTrack.IsModified == true ? false : false,
                            AuthorizedBy = "0029",
                            AuthorizedOn = DateTime.Now
                        };

                        db.Skill.Attach(corp);
                        db.Entry(corp).State = System.Data.Entity.EntityState.Deleted;
                        await db.SaveChangesAsync();
                        using (var context = new DataBaseContext())
                        {

                            ////DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corp, null, "Skill", corp.DBTrack);
                        }


                        db.Entry(corp).State = System.Data.Entity.EntityState.Detached;
                        ts.Complete();
                        return Json(new Object[] { "", "", "Record Authorised", JsonRequestBehavior.AllowGet });
                    }

                }
                return View();
            }
        }
        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                Skill skill = db.Skill.Where(e => e.Id == data).SingleOrDefault();

                //Corporate corporates = db.Corporate.Where(e => e.Id == data).SingleOrDefault();
                if (skill.DBTrack.IsModified == true)
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack, corporates, null, "Corporate");
                        DBTrack dbT = new DBTrack
                        {
                            Action = "D",
                            CreatedBy = skill.DBTrack.CreatedBy != null ? skill.DBTrack.CreatedBy : null,
                            CreatedOn = skill.DBTrack.CreatedOn != null ? skill.DBTrack.CreatedOn : null,
                            IsModified = skill.DBTrack.IsModified == true ? true : false
                        };
                        skill.DBTrack = dbT;
                        db.Entry(skill).State = System.Data.Entity.EntityState.Modified;
                        //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack);
                        await db.SaveChangesAsync();
                        using (var context = new DataBaseContext())
                        {
                            ////DBTrackFile.DBTrackSave("Core/P2b.Global", "D", skill, null, "Skill", skill.DBTrack);
                        }
                        ts.Complete();
                        return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
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
                                ModifiedBy = "0029",
                                ModifiedOn = DateTime.Now,
                                CreatedBy = skill.DBTrack.CreatedBy != null ? skill.DBTrack.CreatedBy : null,
                                CreatedOn = skill.DBTrack.CreatedOn != null ? skill.DBTrack.CreatedOn : null,
                                IsModified = skill.DBTrack.IsModified == true ? false : false,
                                AuthorizedBy = "0029",
                                AuthorizedOn = DateTime.Now
                            };

                            // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = "0029", ModifiedOn = DateTime.Now };

                            db.Entry(skill).State = System.Data.Entity.EntityState.Deleted;
                            await db.SaveChangesAsync();


                            using (var context = new DataBaseContext())
                            {

                                ////DBTrackFile.DBTrackSave("Core/P2b.Global", "D", skill, null, "Skill", dbT);
                            }
                            ts.Complete();
                            return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });

                        }
                        catch (RetryLimitExceededException /* dex */)
                        {
                            //Log the error (uncomment dex variable name and add a line here to write a log.)
                            //ModelState.AddModelError("", "Unable to delete. Try again, and if the problem persists, see your system administrator.");
                            //return RedirectToAction("Delete");
                            return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
                        }
                    }
                }

            }
        }

        public ActionResult GetMyEmpSkill()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Emp = Convert.ToInt32(SessionManager.EmpId);
                var qurey = db.Employee.Include(e => e.EmpAcademicInfo)
                 .Include(e => e.EmpAcademicInfo.Skill)

                 .Where(e => e.Id == Emp && e.EmpAcademicInfo != null).SingleOrDefault();

                var ListreturnDataClass = new List<returnDataClass>();
                if (qurey != null && qurey.EmpAcademicInfo != null && qurey.EmpAcademicInfo.Skill.Count > 0)
                {
                    foreach (var item in qurey.EmpAcademicInfo.Skill)
                    {

                        var Name = item.Name != null ? item.Name : null;

                        ListreturnDataClass.Add(new returnDataClass
                        {
                            EmpId = item.Id,
                            val =
                            "Name :" + Name +
                            ""
                        });
                    }
                }
                if (ListreturnDataClass != null && ListreturnDataClass.Count > 0)
                {
                    return Json(new { status = true, data = ListreturnDataClass, responseText = "Okk" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = false, data = ListreturnDataClass, responseText = "Error" }, JsonRequestBehavior.AllowGet);
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

        [HttpPost]
        public ActionResult GetSubInvPayLKDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.ProgramList.ToList();

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.ProgramList.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }
                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }
    }
}