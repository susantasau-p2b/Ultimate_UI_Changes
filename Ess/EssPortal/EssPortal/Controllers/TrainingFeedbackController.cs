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
using System.Web.Script.Serialization;
using Training;
using System.Diagnostics;
namespace EssPortal.Controllers.Core.MainController
{
    public class TrainingFeedbackController : Controller
    {
        public ActionResult Partial()
        {
            return View("~/Views/TrainingFeedback/Index.cshtml");
        }
        public ActionResult Partial1()
        {
            return View("~/Views/Shared/_TrainingFeedbackReqPartial.cshtml");
        }
        public ActionResult Partial_View()
        {
            return View("~/Views/Shared/_TrainingFeedbackView.cshtml");
        }
        public class returnDataClass
        {
            public Int32 EmpId { get; set; }
            public String val { get; set; }
            public Int32 Id2 { get; set; }
            public bool Id3 { get; set; }
            public Int32 LvHead_Id { get; set; }
        }
        public ActionResult GetSkill()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Emp = Convert.ToInt32(SessionManager.EmpId);
                var qurey = db.Employee
                    .Include(e => e.EmpAcademicInfo.Skill)
                    .Include(e => e.EmpAcademicInfo.Skill.Select(a => a.SkillType))
                    .Where(e => e.Id == Emp)
                    .SingleOrDefault();

                var ListreturnDataClass = new List<returnDataClass>();
                if (qurey != null && qurey.EmpAcademicInfo != null && qurey.EmpAcademicInfo.Skill != null && qurey.EmpAcademicInfo.Skill.Count > 0)
                {
                    foreach (var item in qurey.EmpAcademicInfo.Skill)
                    {
                        var Name = item.Name != null ? item.Name : null;
                        var SkillType = item.SkillType != null ? item.SkillType.LookupVal : null;
                        ListreturnDataClass.Add(new returnDataClass
                        {
                            EmpId = item.Id,
                            val =
                            "Name :" + Name +
                            ", SkillType :" + SkillType + ""
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
        public class ChildGetLvNewReqClass
        {
            public string LvNewReq { get; set; }
            public string EmpLVid { get; set; }
            public string Status { get; set; }
            public string LvHead_Id { get; set; }
            public string IsClose { get; set; }

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
            public string FacultyRating { get; set; }
            public string FacultyFeedback { get; set; }
            public string TrainingRating { get; set; }
            public string TrainingFeedback { get; set; }
            //public string Wf { get; set; }
            public Int32 Id { get; set; }
            //public bool TrClosed { get; set; }
            //public string EmployeeName { get; set; }

            //public int EmployeeId { get; set; }
        }

        public ActionResult GetEmpTrainingFeedBackData(string data)
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
                    FacultyFeedback = w.FacultyFeedback == null ? "" : w.FacultyFeedback,
                    FacultyRating = w.FaultyRating == null ? "" : w.FaultyRating,
                    TrainingFeedback = w.TrainingFeedback == null ? "" : w.TrainingFeedback,
                    TrainingRating = w.TrainingRating == null ? "" : w.TrainingRating
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

        public class GetEmptrainingNeeddata
        {
            public int Id { get; set; }
            public string SeesionDetails { get; set; }
            public string ProgrameDetails { get; set; }
            public string FacultyRating { get; set; }
            public string FacultyFeedBack { get; set; }
            public string TrainingRating { get; set; }
            public string TrainingFeedback { get; set; }

            public ChildGetLvNewReqClass RowData { get; set; }
        }
        public class GetLvNewReqClass
        {
            public string SeesionDetails { get; set; }
            public string ProgrameDetails { get; set; }
            public string FacultyRating { get; set; }
            public string FacultyFeedBack { get; set; }
            public string TrainingRating { get; set; }
            public string TrainingFeedback { get; set; }

            public ChildGetLvNewReqClass RowData { get; set; }
        }
        public ActionResult GetMyTrainingFeedback()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (string.IsNullOrEmpty(Convert.ToString(SessionManager.EmpLvId)))
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }

                var Id = Convert.ToInt32(SessionManager.EmpLvId);

                DateTime date = DateTime.Now.Date;
                var db_data = db.EmployeeTraining
                                         .Include(e => e.Employee)
                                         .Include(e => e.TrainingDetails)
                                         .Include(e => e.TrainingDetails.Select(r => r.TrainingSchedule))
                                         .Include(e => e.TrainingDetails.Select(r => r.TrainigDetailSessionInfo))
                                         .Include(e => e.TrainingDetails.Select(r => r.TrainigDetailSessionInfo.Select(t => t.TrainingSession.SessionType)))
                                         .Include(e => e.TrainingDetails.Select(r => r.TrainigDetailSessionInfo.Select(t => t.TrainingSession.TrainingProgramCalendar.ProgramList)))
                                        .Where(e => e.Employee.Id == Id).SingleOrDefault();

                // var LvCalendar = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "LEAVECALENDAR" && e.Default == true).SingleOrDefault();


                //if (listLvs != null && listLvs.Count() > 0)
                //{
                List<GetLvNewReqClass> returndata = new List<GetLvNewReqClass>();
                returndata.Add(new GetLvNewReqClass
                {
                    SeesionDetails = "Session",
                    ProgrameDetails = "Program Name",
                    FacultyRating = "FacultyRating",
                    FacultyFeedBack = "FacultyFeedBack",
                    TrainingRating = "TrainingRating",
                    TrainingFeedback = "TrainingFeedback",
                });
                //List<int> lvcode = listLvs.Where(e => e.LvCreditDate != null).Select(e => e.LeaveHead.Id).Distinct().ToList();
                //foreach (var item1 in lvcode)
                //{
                // DateTime? lvcrdate = listLvs.Where(a => a.LeaveHead.Id == item1 && a.LvCreditDate != null).Max(a => a.LvCreditDate.Value);
                //if (lvcrdate != null)
                //{

                foreach (var item in db_data.TrainingDetails)
                {
                    var CheckPresentyOfemp = item.TrainigDetailSessionInfo.Where(e => e.IsPresent == true).ToList();
                    foreach (var item1 in CheckPresentyOfemp)
                    {
                        if (item1.TrainingSession != null && item1.TrainingSession.TrainingProgramCalendar != null && item1.TrainingSession.TrainingProgramCalendar.ProgramList != null)
                        {
                            string ProgrameFulldetails = "Start Date:" + item1.TrainingSession.TrainingProgramCalendar.StartDate.Value.ToShortDateString() + ", End Date:"
                                                      + item1.TrainingSession.TrainingProgramCalendar.EndDate.Value.ToShortDateString() + ", Subject Details:" + item1.TrainingSession.TrainingProgramCalendar.ProgramList.SubjectDetails;
                            returndata.Add(new GetLvNewReqClass
                            {
                                RowData = new ChildGetLvNewReqClass
                                {
                                    LvNewReq = item1.Id.ToString(),
                                    EmpLVid = db_data.Id.ToString(),
                                    // IsClose = Status,
                                    // Status = Status,
                                    // LvHead_Id = item.LeaveHead.Id.ToString(),
                                },
                                FacultyFeedBack = item1.FacultyFeedback == null ? "" : item1.FacultyFeedback,
                                FacultyRating = item1.FaultyRating == null ? "" : item1.FaultyRating,
                                TrainingRating = item1.TrainingRating == null ? "" : item1.TrainingRating,
                                TrainingFeedback = item1.TrainingFeedback == null ? "" : item1.TrainingFeedback,
                                SeesionDetails = item1.TrainingSession == null ? "" : item1.TrainingSession.FullDetails,
                                ProgrameDetails = ProgrameFulldetails
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
                                         .Include(e => e.TrainingDetails.Select(r => r.TrainingSchedule))
                                         .Include(e => e.TrainingDetails.Select(r => r.TrainigDetailSessionInfo))
                                         .Include(e => e.TrainingDetails.Select(r => r.TrainigDetailSessionInfo.Select(t => t.TrainingSession.SessionType)))
                                         .Include(e => e.TrainingDetails.Select(r => r.TrainigDetailSessionInfo.Select(t => t.TrainingSession.TrainingProgramCalendar.ProgramList)))
                                        .Where(e => e.Employee.Id == Id).SingleOrDefault();
                //Calendar DefaultCalendaryearid = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "TRAININGCALENDAR" && e.Default == true).SingleOrDefault();
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
                    //var appliedprogrammeids = item.TrainigProgramCalendar.Where(e => !Appliedprogramlistid.Contains(e.ProgramList.Id) && !filtercurrentdateids.Contains(e.Id)).ToList();
                    var CheckPresentyOfemp = item.TrainigDetailSessionInfo.Where(e => e.IsPresent == true && e.FacultyFeedback == null && e.FaultyRating == null && e.TrainingRating == null && e.TrainingFeedback == null && e.IsCancelled == false).ToList();
                    foreach (var item1 in CheckPresentyOfemp)
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
                                FacultyFeedBack = item1.FacultyFeedback == null ? "" : item1.FacultyFeedback,
                                FacultyRating = item1.FaultyRating == null ? "" : item1.FaultyRating,
                                TrainingRating = item1.TrainingRating == null ? "" : item1.TrainingRating,
                                TrainingFeedback = item1.TrainingFeedback == null ? "" : item1.TrainingFeedback,
                                SeesionDetails = item1.TrainingSession == null ? "" : item1.TrainingSession.FullDetails,
                                ProgrameDetails = ProgrameFulldetails
                            });
                        }
                    }
                }
                return Json(new { status = true, data = returndata }, JsonRequestBehavior.AllowGet);

            }
        }
        public class EmpTrainingFeedBackClass
        {
            public int Id { get; set; }
            public string FaultyRating { get; set; }
            public string FacultyFeedback { get; set; }
            public string TrainingRating { get; set; }
            public string TrainingFeedback { get; set; }
        }
        [HttpPost]
        public ActionResult CreateFeedback(TrainingDetails LvReq, FormCollection form, String forwarddata, string TrainingData)
        {

            var serialize = new JavaScriptSerializer();
            List<EmpTrainingFeedBackClass> obj = new List<EmpTrainingFeedBackClass>();
            if (TrainingData != "")
            {
                obj = serialize.Deserialize<List<EmpTrainingFeedBackClass>>(TrainingData);

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
            string Emp = form["EmpFeedback_id"] == "0" ? "" : form["EmpFeedback_id"];
            List<string> Msg = new List<string>();
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

                if (ModelState.IsValid)
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                               new System.TimeSpan(0, 30, 0)))
                    {
                        try
                        {
                            //var OEmployeeTraining = db.EmployeeTraining
                            //       .Include(e => e.TrainingDetails)
                            //       .Include(e => e.TrainingDetails.Select(r => r.TrainingSchedule))
                            //       .Include(e => e.TrainingDetails.Select(r => r.TrainigDetailSessionInfo))
                            //       .Include(e => e.TrainingDetails.Select(r => r.TrainigDetailSessionInfo.Select(t => t.TrainingSession)))
                            //       .Where(e => e.Employee.Id == EmpId).SingleOrDefault();
                            foreach (var item in obj)
                            {
                                //List<Employee> Emp = new List<Employee>();
                                if (item.FaultyRating != "" && item.FacultyFeedback != "" && item.TrainingRating != "" && item.TrainingFeedback != "")
                                {
                                    if (item.Id != null)
                                    {
                                        var SessionLstInfo = db.TrainigDetailSessionInfo.Where(t => t.Id == item.Id).FirstOrDefault();
                                        if (SessionLstInfo != null)
                                        {
                                            SessionLstInfo.FacultyFeedback = item.FacultyFeedback;
                                            SessionLstInfo.FaultyRating = item.FaultyRating;
                                            SessionLstInfo.TrainingFeedback = item.TrainingFeedback;
                                            SessionLstInfo.TrainingRating = item.TrainingRating;
                                            db.TrainigDetailSessionInfo.Attach(SessionLstInfo);
                                            db.Entry(SessionLstInfo).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();
                                            db.Entry(SessionLstInfo).State = System.Data.Entity.EntityState.Detached;
                                        }

                                    }
                                    //}

                                }
                            }
                            ts.Complete();
                            Msg.Add("Record Updated");
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
        public ActionResult Create_CancelReq(TrainigDetailSessionInfo L, FormCollection form, String data) //Create submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    DBTrack DBTrack = new DBTrack { Action = "M", CreatedBy = SessionManager.UserName, IsModified = true };
                    string PresentList = form["IsCancel"] == "0" ? "" : form["IsCancel"];
                    string Trainingdetailssession = form["LvId"] == "0" ? "" : form["LvId"];
                    string FacultyRating_reason = form["FacultyRating_reason"] == "0" ? "" : form["FacultyRating_reason"];
                    string TrainingRating_reason = form["TrainingRating_reason"] == "0" ? "" : form["TrainingRating_reason"];
                    string FacultyFeedBack_reason = form["FacultyFeedBack_reason"] == "0" ? "" : form["FacultyFeedBack_reason"];
                    string TrainingFeedBack_reason = form["TrainingFeedBack_reason"] == "0" ? "" : form["TrainingFeedBack_reason"];
                    var PresentStatus = false;
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
                    //if (Reason != "")
                    //{
                    //    Reasoniscancel = Reason;
                    //}

                    //if (PresentList != null)
                    //{
                    //    PresentStatus = Convert.ToBoolean(PresentList);
                    //    //if (LvCancelchk == false)
                    //    //{
                    //    //    return Json(new { status = true, responseText = "Set cancel True" }, JsonRequestBehavior.AllowGet);
                    //    //}
                    //}
                    //else
                    //{
                    //    return Json(new { status = true, responseText = "Apply leave cancel" }, JsonRequestBehavior.AllowGet);
                    //}

                    //int lvnewreqid = Convert.ToInt32(data);

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {


                            var SessionLstInfo = db.TrainigDetailSessionInfo.Where(t => t.Id == Trainingdetailssessionid).FirstOrDefault();
                            if (SessionLstInfo != null)
                            {
                                SessionLstInfo.DBTrack = DBTrack;
                                SessionLstInfo.TrainingRating = TrainingRating_reason;
                                SessionLstInfo.TrainingFeedback = TrainingFeedBack_reason;
                                SessionLstInfo.FaultyRating = FacultyRating_reason;
                                SessionLstInfo.FacultyFeedback = FacultyFeedBack_reason;
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
    }
}