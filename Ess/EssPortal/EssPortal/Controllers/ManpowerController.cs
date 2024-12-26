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
using Appraisal;
using System.Web.Script.Serialization;
using Recruitment;

namespace EssPortal.Controllers
{
    public class ManpowerController : Controller
    {
        //
        // GET: /Manpower/
        public ActionResult Index()
        {
            return View("~/Views/Manpower/Index.cshtml");
        }
        public ActionResult Partial_View()
        {
            return View("~/Views/Shared/_AppraisalReqPartial.cshtml");
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
        public Object Create_Skill(Skill lkval, FormCollection form) //Create submit
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
            public string EmpLVid { get; set; }
            public string IsClose { get; set; }
            public string LvHead_Id { get; set; }

        }
        public class GetLvNewReqClass2
        {
            public string Emp { get; set; }
            public string Category { get; set; }
            public string SubCategory { get; set; }
            public string MaxPoint { get; set; }
            public string RatingPoint { get; set; }
            public string Comment { get; set; }
            public ChildGetLvNewReqClass2 RowData { get; set; }
        }

        [HttpPost]
        public ActionResult CreateManpower(ManpowerRequestPost p, FormCollection form, String forwarddata, string DebitDays)
        {

            using (DataBaseContext db = new DataBaseContext())
            {
                string JobPosition = form["JobPositionMlist"] == "0" ? "" : form["JobPositionMlist"];
                string Qualification = form["QualificationlistM"] == "0" ? "" : form["QualificationlistM"];
                string Skill = form["SkillList"] == "0" ? "" : form["SkillList"];
                string CategoryPost = form["CategoryPostlist"] == "0" ? "" : form["CategoryPostlist"];
                string CategorySplPost = form["CategorySplPostlist"] == "0" ? "" : form["CategorySplPostlist"];
                string ExpFilter_Id = form["ExpFilterlist"] == "0" ? "" : form["ExpFilterlist"];
                string RangeFilter_Id = form["RangeFilterlist"] == "0" ? "" : form["RangeFilterlist"];
                string Gender = form["Gender_drop"] == "0" ? "" : form["Gender_drop"];
                string MStatus = form["MaritalStatus_drop"] == "0" ? "" : form["MaritalStatus_drop"];
                string PostSourceTypelist = form["PostSourceTypelist"] == "0" ? "" : form["PostSourceTypelist"];


                Calendar recruitcalendar = db.Calendar.Include(e => e.Name).Where(e => e.Default == true && e.Name.LookupVal.ToString().ToUpper() == "RECRUITMENTCALENDAR").SingleOrDefault();


                RecruitYearlyCalendar recruityearlycalendar = db.RecruitYearlyCalendar
                                                              .Include(e => e.RecruitmentCalendar)
                                                              .Include(e => e.ManpowerRequestPost)
                                                            .Where(e => e.RecruitmentCalendar.Id == recruitcalendar.Id).SingleOrDefault();

                List<ManpowerRequestPost> OFAT = new List<ManpowerRequestPost>();
                List<String> Msg = new List<String>();
                try
                {
                    //p.FuncStruct.JobPosition = null;
                    //List<FuncStruct> job = new List<FuncStruct>();
                    //string val2 = form["JobPositionMlist"];

                    //if (val2 != null && val2 != "")
                    //{
                    //    var ids = Utility.StringIdsToListIds(val2);
                    //    foreach (var ca in ids)
                    //    {
                    //        var OBJ_val = db.FuncStruct.Find(ca);
                    //        job.Add(OBJ_val);
                    //        p.FuncStruct = job;
                    //    }
                    //}

                    if (JobPosition != null)
                    {
                        if (JobPosition != "")
                        {
                            int ContId = Convert.ToInt32(JobPosition);
                            var val = db.FuncStruct.Include(e => e.Job).Include(e => e.JobPosition)
                                                .Where(e => e.Id == ContId).SingleOrDefault();
                            p.FuncStruct = val;
                        }
                    }


                    p.Qualification = null;
                    List<Qualification> OBJ = new List<Qualification>();
                    string Values = form["QualificationlistM"];

                    if (Values != null && Values != "")
                    {
                        var ids = Utility.StringIdsToListIds(Values);
                        foreach (var ca in ids)
                        {
                            var OBJ_val = db.Qualification.Find(ca);
                            OBJ.Add(OBJ_val);
                            p.Qualification = OBJ;
                        }
                    }

                    p.Skill = null;
                    List<Skill> sk = new List<Skill>();
                    string Val1 = form["SkillList"];

                    if (Val1 != null && Val1 != "")
                    {
                        var ids = Utility.StringIdsToListIds(Val1);
                        foreach (var ca in ids)
                        {
                            var OBJ_val = db.Skill.Find(ca);
                            sk.Add(OBJ_val);
                            p.Skill = sk;
                        }
                    }

                    p.CategoryPost = null;
                    List<CategoryPost> cp = new List<CategoryPost>();
                    string val3 = form["CategoryPostlist"];

                    if (val3 != null && val3 != "")
                    {
                        var ids = Utility.StringIdsToListIds(val3);
                        foreach (var ca in ids)
                        {
                            var p_val = db.CategoryPost.Find(ca);
                            cp.Add(p_val);
                            p.CategoryPost = cp;
                        }
                    }

                    p.CategorySplPost = null;
                    List<CategorySplPost> csp = new List<CategorySplPost>();
                    string val4 = form["CategorySplPostlist"];

                    if (val4 != null && val4 != "")
                    {
                        var ids = Utility.StringIdsToListIds(val4);
                        foreach (var ca in ids)
                        {
                            var p_val = db.CategorySplPost.Find(ca);
                            csp.Add(p_val);
                            p.CategorySplPost = csp;
                        }
                    }

                    if (ExpFilter_Id != null && ExpFilter_Id != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(ExpFilter_Id));
                        p.ExpFilter = val;
                    }

                    if (RangeFilter_Id != null && RangeFilter_Id != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(RangeFilter_Id));
                        p.RangeFilter = val;
                    }

                    if (MStatus != null && MStatus != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(MStatus));
                        p.MaritalStatus = val;
                    }

                    if (Gender != null && Gender != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(Gender));
                        p.Gender = val;
                    }


                    var val1 = db.LookupValue.Where(e => e.LookupVal.ToString().ToUpper() == "DEPTREQUEST").SingleOrDefault();
                    p.PostSourceType = val1;


                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            //if (db.PostDetails.Any(o => o.FuncStruct.JobPosition == p.FuncStruct.JobPosition))
                            //{
                            //    Msg.Add("Code Already Exists.");
                            //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //}

                            p.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };                     //???????

                            ManpowerRequestPost Postdetails = new ManpowerRequestPost()
                            {
                                FuncStruct = p.FuncStruct,                     //true?//
                                RequestVacancies = p.RequestVacancies,
                                ExpYearFrom = p.ExpYearFrom,
                                ExpYearTo = p.ExpYearTo,
                                AgeFrom = p.AgeFrom,
                                AgeTo = p.AgeTo,
                                Narration = p.Narration == null ? "" : p.Narration.ToString(),
                                RangeFilter = p.RangeFilter,
                                ExpFilter = p.ExpFilter,
                                CategoryPost = p.CategoryPost,
                                CategorySplPost = p.CategorySplPost,
                                Skill = p.Skill,
                                Gender = p.Gender,
                                MaritalStatus = p.MaritalStatus,
                                Qualification = p.Qualification,
                                PostSourceType = p.PostSourceType,
                                PostCode = p.PostCode,
                                PostRequestDate = p.PostRequestDate,
                                DBTrack = p.DBTrack
                            };

                            db.ManpowerRequestPost.Add(Postdetails);

                            //var rtn_Obj = DBTrackFile.DBTrackSave("Recruitement/Recruitement", null, db.ChangeTracker, p.DBTrack);
                            //DT_ManpowerRequestPost DT_Post = (DT_PostDetails)rtn_Obj;
                            //DT_Post.Gender_Id = p.Gender == null ? 0 : p.Gender.Id;
                            //DT_Post.MaritalStatus_Id = p.MaritalStatus == null ? 0 : p.MaritalStatus.Id;
                            //DT_Post.ExpFilter_Id = p.ExpFilter == null ? 0 : p.ExpFilter.Id;
                            //DT_Post.RangeFilter_Id = p.RangeFilter == null ? 0 : p.RangeFilter.Id;

                            //db.Create(DT_Post);
                            db.SaveChanges();
                            OFAT.Add(db.ManpowerRequestPost.Find(Postdetails.Id));
                            if (recruityearlycalendar.ManpowerRequestPost != null)
                            {
                                OFAT.AddRange(recruityearlycalendar.ManpowerRequestPost);
                            }
                            recruityearlycalendar.ManpowerRequestPost = OFAT;
                            db.RecruitYearlyCalendar.Attach(recruityearlycalendar);
                            db.Entry(recruityearlycalendar).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            db.Entry(recruityearlycalendar).State = System.Data.Entity.EntityState.Detached;
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", "C", corporate, null, "Corporate", null);


                            ts.Complete();
                            Msg.Add("Data Saved Successfully.");
                            return Json(new { status = true, responseText = "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
                        }
                        //catch (DbUpdateConcurrencyException)
                        //{
                        //    return RedirectToAction("Create", new { concurrencyError = true, id = c.Id });
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
                        //return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
                        //return this.Json(new { msg = errorMsg });
                    }
                }
                catch (Exception e)
                {
                    LogFile Logfile = new LogFile();
                    ErrorLog Err = new ErrorLog()
                    {
                        ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
                        ExceptionMessage = e.Message,
                        ExceptionStackTrace = e.StackTrace,
                        LineNo = Convert.ToString(new System.Diagnostics.StackTrace(e, true).GetFrame(0).GetFileLineNumber()),
                        LogTime = DateTime.Now
                    };
                    Logfile.CreateLogFile(Err);
                    Msg.Add(e.Message);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        public ActionResult Create_CancelReq(EmpAppRating L, FormCollection form, String data) //Create submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    var ids = Utility.StringIdsToListString(data);
                    var empappratingid = Convert.ToInt32(ids[0]);
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
                    //if (Reason != "")
                    //{
                    //    Reasoniscancel = Reason;
                    //}

                    //if (LvCancellist != null)
                    //{
                    //    LvCancelchk = Convert.ToBoolean(LvCancellist);
                    //    if (LvCancelchk == false)
                    //    {
                    //        return Json(new { status = true, responseText = "Set cancel True" }, JsonRequestBehavior.AllowGet);
                    //    }
                    //}
                    //else
                    //{
                    //    return Json(new { status = true, responseText = "Apply leave cancel" }, JsonRequestBehavior.AllowGet);
                    //}

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            try
                            {

                                var empapprating = db.EmpAppRating.Where(e => e.Id == empappratingid).SingleOrDefault();

                                empapprating.RatingPoints = L.RatingPoints;
                                db.EmpAppRating.Attach(empapprating);//Bhagesh Added code 
                                db.Entry(empapprating).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(empapprating).State = System.Data.Entity.EntityState.Detached;

                                //LvWFDetails oLvWFDetails = new LvWFDetails
                                //{
                                //    WFStatus = 6,
                                //    Comments = Reasoniscancel,
                                //    DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                                //};
                                //List<LvWFDetails> oLvWFDetails_List = new List<LvWFDetails>();
                                //// db.LvWFDetails.Add(oLvWFDetails);
                                //// db.SaveChanges();
                                //// oLvWFDetails_List.Add(db.LvWFDetails.Find(oLvWFDetails.Id));
                                //oLvWFDetails_List.Add(oLvWFDetails);

                                //   qurey.TrClosed = true;
                                //   qurey.TrReject = true;
                                //   qurey.IsCancel = true;
                                //   // qurey.LvWFDetails = oLvWFDetails_List;
                                //   qurey.WFStatus = db.LookupValue.Where(e => e.LookupVal == "6").SingleOrDefault();


                                //   //var aa = db.EmpTrainingNeed.Find(qurey.Id);
                                // //  oLvWFDetails_List.AddRange(qurey.LvWFDetails);
                                ////   qurey.LvWFDetails = oLvWFDetails_List;
                                //   db.EmpTrainingNeed.Attach(qurey);//Bhagesh Added code 
                                //   db.Entry(qurey).State = System.Data.Entity.EntityState.Modified;
                                //   db.SaveChanges();
                                //   db.Entry(qurey).State = System.Data.Entity.EntityState.Detached;//bhagesh code end
                                ts.Complete();
                                return Json(new { status = true, responseText = "Data Updated Successfully." }, JsonRequestBehavior.AllowGet);
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
                List<EmployeeAppraisal> LvList = new List<EmployeeAppraisal>();
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
                    Category = "Category",
                    SubCategory = "Subcategory",
                    MaxPoint = "MaxPoint",
                    RatingPoint = "RatingPoint",
                    Comment = "Comment",
                });
                foreach (var item1 in EmpidsWithfunsub)
                {
                    //item.ReportingEmployee
                    if (item1.ReportingEmployee.Count() > 0)
                    {
                        List<string> Funcsubid = new List<string>();
                        var temp = db.EmployeeAppraisal
                          .Include(e => e.Employee)
                           .Include(e => e.Employee.EmpName)
                           .Include(e => e.EmpAppEvaluation)
                           .Include(e => e.EmpAppEvaluation.Select(t => t.EmpAppRatingConclusion))
                           .Include(e => e.EmpAppEvaluation.Select(t => t.EmpAppRatingConclusion.Select(x => x.AppraisalAssistance)))
                           .Include(e => e.EmpAppEvaluation.Select(t => t.EmpAppRatingConclusion.Select(x => x.EmpAppRating.Select(a => a.AppAssignment.AppCategory))))
                            .Include(e => e.EmpAppEvaluation.Select(t => t.EmpAppRatingConclusion.Select(x => x.EmpAppRating.Select(a => a.AppAssignment.AppSubCategory))))
                            .Where(e => item1.ReportingEmployee.Contains(e.Employee.Id)).ToList();



                        foreach (var item in temp)
                        {
                            foreach (var item2 in item.EmpAppEvaluation)
                            {
                                var empapprating = item2.EmpAppRatingConclusion.Where(e => e.AppraisalAssistance != null && e.AppraisalAssistance.LookupVal.ToString().ToUpper() == "APPRAISEE").ToList();
                                foreach (var item3 in empapprating)
                                {
                                    foreach (var item4 in item3.EmpAppRating)
                                    {
                                        ListreturnLvnewClass.Add(new GetLvNewReqClass2
                                        {
                                            RowData = new ChildGetLvNewReqClass2
                                            {
                                                LvNewReq = item4.Id.ToString(),
                                                EmpLVid = item.Employee.Id.ToString(),
                                                //IsClose = query.Employee.ReportingStructRights.Where(a => a.AccessRights != null && a.AccessRights.Id == item1.AccessRights && a.FuncModules.LookupVal.ToUpper() == item1.ModuleName.ToUpper())
                                                //.Select(a => a.AccessRights.IsClose).FirstOrDefault().ToString(),
                                                // LvHead_Id = LvReq.LeaveHead.Id.ToString(),
                                            },
                                            Emp = item.Employee.EmpCode + " " + item.Employee.EmpName.FullNameFML,
                                            Category = item4.AppAssignment == null ? "" : item4.AppAssignment.AppCategory == null ? "" : item4.AppAssignment.AppCategory.Name,
                                            SubCategory = item4.AppAssignment == null ? "" : item4.AppAssignment.AppSubCategory == null ? "" : item4.AppAssignment.AppSubCategory.Name,
                                            MaxPoint = item4.AppAssignment == null ? "" : item4.AppAssignment.MaxRatingPoints.ToString(),
                                            RatingPoint = item4.RatingPoints.ToString(),
                                            Comment = item4.Comments
                                        });
                                    }
                                }
                            }

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
        public ActionResult Get_Employelist(string geo_id)
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
                List<Employee> data = new List<Employee>();
                var compid = Convert.ToInt32(Session["CompId"].ToString());
                foreach (var item in EmpidsWithfunsub)
                {

                    var empdata = db.EmployeeAppraisal
                        .Include(e => e.Employee)
                        .Include(e => e.Employee.EmpName)
                        .Include(e => e.EmpAppEvaluation)
                        .Include(e => e.EmpAppEvaluation.Select(a => a.EmpAppRatingConclusion))
                        .Include(e => e.EmpAppEvaluation.Select(a => a.EmpAppRatingConclusion.Select(x => x.EmpAppRating)))
                        .Where(e => item.ReportingEmployee.Contains(e.Employee.Id)).ToList();

                    foreach (var item1 in empdata)
                    {
                        foreach (var item2 in item1.EmpAppEvaluation)
                        {
                            foreach (var item3 in item2.EmpAppRatingConclusion)
                            {
                                var empappratingp = item3.EmpAppRating.ToList();
                                if (empappratingp.Count() > 0)
                                {
                                    data.Add(item1.Employee);
                                }
                            }
                        }
                    }
                    //var emp = empdata.EmpAppEvaluation.ToList();
                    //if (emp != null && emp.Count != 0)
                    //{
                    //    foreach (var item in emp)
                    //    {
                    //        if (item.ServiceBookDates.ServiceLastDate == null)
                    //        {
                    //            data.Add(item);
                    //        }
                    //        if (item.ServiceBookDates.ServiceLastDate.Value.ToString("MM/yyyy") != monthyr)
                    //        {
                    //            data.Add(item);
                    //        }


                    //    }
                    //}
                }

                List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                if (data != null && data.Count != 0)
                {
                    foreach (var item in data.Distinct())
                    {
                        returndata.Add(new Utility.returndataclass
                        {
                            code = item.Id.ToString(),
                            value = item.FullDetails,
                        });
                    }
                    var returnjson = new
                    {
                        data = returndata,
                        tablename = "employee-table"
                    };
                    return Json(returnjson, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("No Record Found", JsonRequestBehavior.AllowGet);
                }
            }
        }
        [HttpPost]
        public ActionResult GetQualificationDetailLKDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.Qualification.ToList();
                IEnumerable<Qualification> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.Qualification.ToList().Where(d => d.QualificationShortName.Contains(data));

                }
                else
                {
                    //var data1 = db.QualificationDetails
                    //.Select(e => new
                    //{
                    //    value = "University :" + e.Qualification + ",Institute : " + e.University
                    //}).ToString();  
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                    return Json(r, JsonRequestBehavior.AllowGet);
                }

                ////string univer = db.QualificationDetails.Include(a => a.University).ToString();
                ////string inst =  db.QualificationDetails.Include(a=>a.Institute).ToString();  
                //string ca121 = "University :"+fall.Select(a=>a.Institute) +",Institute :"+fall.Select(a=>a.University) ;  

                var result = (from c in all
                              select new { c.Id, c.FullDetails }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public ActionResult GetSkillLKDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.Skill.ToList();
                IEnumerable<Skill> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.Skill.ToList().Where(d => d.Name.Contains(data));

                }
                else
                {
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.Name }).Distinct();

                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.Name }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public ActionResult GetCategoryDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.CategoryPost.Include(a => a.Category).ToList();
                IEnumerable<CategoryPost> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.CategoryPost.Include(a => a.Category).ToList();
                }
                else
                {
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.FullDetails }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public ActionResult GetCategorySplDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.CategorySplPost.Include(a => a.SpecialCategory).ToList();
                IEnumerable<CategorySplPost> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.CategorySplPost.Include(a => a.SpecialCategory).ToList();

                }
                else
                {
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.FullDetails }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }

        }
        [HttpPost]
        public ActionResult GetJobPositionDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.FuncStruct.Include(e => e.Job).Include(e => e.JobPosition).ToList();
                IEnumerable<FuncStruct> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.FuncStruct.Include(e => e.Job).Include(e => e.JobPosition).ToList().Where(d => d.FullDetails.Contains(data));                    // no full details 

                }
                else
                {
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.FullDetails }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
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
                var qurey = db.EmpTrainingNeed
                    .Include(e => e.WFStatus)
                    .Include(e => e.LvWFDetails)
                    .Where(e => e.Id == lvnewreqid).SingleOrDefault();

                var LvCalendar = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "LEAVECALENDAR" && e.Default == true).SingleOrDefault();
                if (authority.ToUpper() == "MYSELF")
                {
                    qurey.IsCancel = true;
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
                        oLvWFDetails_List.Add(oLvWFDetails);

                        qurey.WFStatus = db.LookupValue.Where(e => e.LookupVal == "2").SingleOrDefault();
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
                        var LvWFDetails = new LvWFDetails
                        {
                            WFStatus = 3,
                            Comments = ReasonApproval,
                            DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName }
                        };
                        qurey.TrClosed = Convert.ToBoolean(isClose) == true ? true : false;
                        qurey.LvWFDetails.Add(LvWFDetails);
                        qurey.WFStatus = db.LookupValue.Where(e => e.LookupVal == "3").SingleOrDefault();
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
                        qurey.WFStatus = db.LookupValue.Where(e => e.LookupVal == "4").SingleOrDefault();
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
                        return Json(new Utility.JsonClass { status = true, responseText = "Record Updated..!" }, JsonRequestBehavior.AllowGet);
                    }
                }
                catch (Exception e)
                {
                    return Json(new Utility.JsonClass { status = false, responseText = e.InnerException.Message.ToString() }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        public class EmpAppRatingConclusionDetails
        {
            public int ID { get; set; }
            public int EmpEvaluationId { get; set; }
            public string Editable { get; set; }
            public string Comments { get; set; }
            public string ObjectiveWordings { get; set; }
            public string CatName { get; set; }
            public string SubCatName { get; set; }
            public int MaxRatingPoints { get; set; }
            public int RatingPoints { get; set; }
            public int AppraiseePoints { get; set; }
            public string AppraiseeComments { get; set; }
            public int AppraiserPoints { get; set; }
            public string AppraiserComments { get; set; }
            public int HRPoints { get; set; }
            public string HRComments { get; set; }
            // public string Appriasalassistance { get; set; }
        }
        public ActionResult LoadEmp(P2BGrid_Parameters gp, string extraeditdata, FormCollection form)
        {
            List<string> Msg = new List<string>();
            int EmpId = 0;
            if (extraeditdata != null && extraeditdata != "0" && extraeditdata != "false")
            {
                EmpId = int.Parse(extraeditdata);
            }
            try
            {
                DataBaseContext db = new DataBaseContext();
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                var jsonData = (Object)null;
                // int EmpId = Convert.ToInt32(extraeditdata.Split(',')[0]);
                //   int AssId = Convert.ToInt32(extraeditdata.Split(',')[0]);

                List<EmpAppRatingConclusionDetails> EmpAppRatingConclusionDetailsList = new List<EmpAppRatingConclusionDetails>();
                List<int> empappracategory = new List<int>();
                List<int> empapprasubcategory = new List<int>();
                //var Sal = db.CompanyPayroll.Where(e => e.Company.Id == id).Include(e => e.SalHeadFormula).Select(e => e.SalHeadFormula).SingleOrDefault();
                var db_data = db.EmployeeAppraisal
                       .Where(e => e.Employee.Id == EmpId)
                       .Include(e => e.EmpAppEvaluation)
                       .Include(e => e.Employee)
                       .Include(e => e.EmpAppEvaluation.Select(a => a.EmpAppRatingConclusion))
                       .Include(e => e.EmpAppEvaluation.Select(a => a.EmpAppRatingConclusion.Select(t => t.AppraisalAssistance)))
                       .Include(e => e.EmpAppEvaluation.Select(a => a.EmpAppRatingConclusion.Select(t => t.EmpAppRating)))
                       .Include(e => e.EmpAppEvaluation.Select(a => a.EmpAppRatingConclusion.Select(t => t.EmpAppRating.Select(x => x.AppAssignment))))
                       .Include(e => e.EmpAppEvaluation.Select(a => a.EmpAppRatingConclusion.Select(t => t.EmpAppRating.Select(x => x.AppAssignment.AppCategory))))
                       .Include(e => e.EmpAppEvaluation.Select(a => a.EmpAppRatingConclusion.Select(t => t.EmpAppRating.Select(x => x.AppAssignment.AppSubCategory))))
                      .SingleOrDefault();


                if (db_data != null)
                {
                    foreach (var item in db_data.EmpAppEvaluation)
                    {
                        var empratingdata = item.EmpAppRatingConclusion.Where(e => e.AppraisalAssistance != null && e.AppraisalAssistance.LookupVal.ToString().ToUpper() == "EMPLOYEEMYSELF").ToList();
                        foreach (var item1 in empratingdata)
                        {
                            foreach (var item2 in item1.EmpAppRating)
                            {
                                empappracategory.Add(item2.AppAssignment.AppCategory.Id);
                                empapprasubcategory.Add(item2.AppAssignment.AppSubCategory.Id);
                            }
                        }
                    }

                }

                if (db_data != null)
                {
                    foreach (var item in db_data.EmpAppEvaluation)
                    {
                        //var empratingdata = item.EmpAppRatingConclusion.Where(e => e.AppraisalAssistance != null && e.AppraisalAssistance.LookupVal.ToString().ToUpper() == "EMPLOYEEMYSELF").ToList();
                        foreach (var item1 in item.EmpAppRatingConclusion)
                        {
                            var empappratingdata = item1.EmpAppRating.Where(e => !empappracategory.Contains(e.AppAssignment.AppCategory.Id) && !empapprasubcategory.Contains(e.AppAssignment.AppSubCategory.Id)).ToList();
                            foreach (var item2 in empappratingdata)
                            {
                                EmpAppRatingConclusionDetailsList.Add(new EmpAppRatingConclusionDetails
                                {
                                    CatName = item2.AppAssignment == null ? "" : item2.AppAssignment.AppCategory == null ? "" : item2.AppAssignment.AppCategory.Name,
                                    SubCatName = item2.AppAssignment == null ? "" : item2.AppAssignment.AppSubCategory == null ? "" : item2.AppAssignment.AppSubCategory.Name,
                                    MaxRatingPoints = item2.AppAssignment == null ? 0 : item2.AppAssignment.MaxRatingPoints,
                                    RatingPoints = 0,
                                    ID = item2.AppAssignment.Id,
                                    Comments = "",
                                    Editable = "true"
                                });
                            }
                        }
                    }

                }

                IEnumerable<EmpAppRatingConclusionDetails> IE = EmpAppRatingConclusionDetailsList;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = EmpAppRatingConclusionDetailsList;
                    if (gp.searchOper.Equals("eq"))
                    {
                        if (gp.searchField == "Id")
                        {
                            //  jsonData = IE.Select(a => new { a.ID, a.CatName, a.SubCatName, a.MaxRatingPoints,a.RatingPoints,a.ObjectiveWordings,a.Comments, a.Editable }).Where((e => (e.ID.ToString().Contains(gp.searchString)))).ToList();
                            jsonData = IE.Where(e => (e.ID.ToString().Contains(gp.searchString))
                               || (e.CatName.ToUpper().Contains(gp.searchString.ToUpper()))
                               || (e.SubCatName.ToUpper().Contains(gp.searchString.ToUpper()))
                               || (e.MaxRatingPoints.ToString().Contains(gp.searchString))
                               || (e.RatingPoints.ToString().Contains(gp.searchString))
                               ).Select(a => new { a.ID, a.CatName, a.SubCatName, a.MaxRatingPoints, a.RatingPoints, a.Comments, a.Editable }).ToList();
                        }
                        //else if (gp.searchField == "Code")
                        //    jsonData = IE.Select(a => new { a.Id, a.MaxRatingPoints, a.AppCategory }).Where((e => (e.MaxRatingPoints.ToString().Contains(gp.searchString)))).ToList();
                        //else if (gp.searchField == "Name")
                        //    jsonData = IE.Select(a => new { a.Id, a.MaxRatingPoints, a.AppCategory }).Where((e => (e.AppCategory.ToString().Contains(gp.searchString)))).ToList();

                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.ID, a.CatName, a.SubCatName, a.MaxRatingPoints, a.RatingPoints, a.Comments, a.Editable }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = EmpAppRatingConclusionDetailsList;
                    Func<EmpAppRatingConclusionDetails, dynamic> orderfuc;

                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.ID : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "AppCategory" ? c.CatName.ToString() :
                                         gp.sidx == "AppSubCategory" ? c.SubCatName.ToString() :
                                         gp.sidx == "MaxRatingPoints" ? c.MaxRatingPoints.ToString() :
                                         gp.sidx == "RatingPoints" ? c.RatingPoints.ToString() :
                                         gp.sidx == "Comments" ? c.Comments.ToString() :
                                         gp.sidx == "Editable" ? c.Editable.ToString() : "");

                    }
                    if (gp.sord == "asc")
                    {                                             //Convert.ToString(a.AppSubCategory.FullDetails),a.AppRatingObjective.Select(r=>r.ObjectiveWordings.LookupVal) !=null? Convert.ToString(a.AppRatingObjective.Select(r=>r.ObjectiveWordings.LookupVal.ToString())): null                                                                                                                                     
                        jsonData = IE.Select(a => new Object[] { a.ID, a.CatName, a.SubCatName, a.MaxRatingPoints, a.RatingPoints, a.Comments, a.Editable }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.ID, a.CatName, a.SubCatName, a.MaxRatingPoints, a.RatingPoints, a.Comments, a.Editable }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.ID, a.CatName, a.SubCatName, a.MaxRatingPoints, a.RatingPoints, a.Comments, a.Editable }).ToList();
                    }
                    totalRecords = EmpAppRatingConclusionDetailsList.Count();
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
            catch (Exception e)
            {
                LogFile Logfile = new LogFile();
                ErrorLog Err = new ErrorLog()
                {
                    ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
                    ExceptionMessage = e.Message,
                    ExceptionStackTrace = e.StackTrace,
                    LineNo = Convert.ToString(new System.Diagnostics.StackTrace(e, true).GetFrame(0).GetFileLineNumber()),
                    LogTime = DateTime.Now
                };
                Logfile.CreateLogFile(Err);
                Msg.Add(e.Message);
                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
            }

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
            public int Id { get; set; }
            public string Category { get; set; }
            public string Subcategory { get; set; }
            public string MaxPoint { get; set; }
            public string RatingPoint { get; set; }
            public string Comment { get; set; }

            public ChildGetLvNewReqClass RowData { get; set; }
        }

        public class GetManpowerClass
        {
            public string PostCode { get; set; }
            public string PostRequestDate { get; set; }
            public string Post { get; set; }
            public string RequestVacancies { get; set; }
            public ChildGetLvNewReqClass2 RowData { get; set; }
        }
        public ActionResult GetMyManpowerRequest()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (string.IsNullOrEmpty(Convert.ToString(SessionManager.EmpLvId)))
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                string Id = Convert.ToString(SessionManager.EmpLvId);
                var db_data = db.ManpowerRequestPost
                      .Include(e => e.FuncStruct)
                      .Include(e => e.FuncStruct.Job)
                      .Where(e => e.DBTrack.CreatedBy == Id)
                     .ToList();

                // var LvCalendar = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "LEAVECALENDAR" && e.Default == true).SingleOrDefault();


                //if (listLvs != null && listLvs.Count() > 0)
                //{
                List<GetManpowerClass> returndata = new List<GetManpowerClass>();
                returndata.Add(new GetManpowerClass
                {
                    PostCode = "PostCode",
                    PostRequestDate = "PostRequestDate",
                    Post = "Post",
                    RequestVacancies = "RequestVacancies"
                });
                //List<int> lvcode = listLvs.Where(e => e.LvCreditDate != null).Select(e => e.LeaveHead.Id).Distinct().ToList();
                //foreach (var item1 in lvcode)
                //{
                // DateTime? lvcrdate = listLvs.Where(a => a.LeaveHead.Id == item1 && a.LvCreditDate != null).Max(a => a.LvCreditDate.Value);
                //if (lvcrdate != null)
                //{

                foreach (var item in db_data)
                {
                    returndata.Add(new GetManpowerClass
                    {
                        RowData = new ChildGetLvNewReqClass2
                        {
                            LvNewReq = item.Id.ToString(),
                            EmpLVid = item.DBTrack.CreatedBy,
                            // IsClose = Status,
                            //Status = Status,
                            // LvHead_Id = item.LeaveHead.Id.ToString(),
                        },
                        PostCode = item.PostCode,
                        PostRequestDate = item.PostRequestDate.Value.ToShortDateString(),
                        Post = item.FuncStruct == null ? "" : item.FuncStruct.Job == null ? "" : item.FuncStruct.Job.Name,
                        RequestVacancies = item.RequestVacancies.ToString()
                    });
                    //var Status = "--";
                    //if (item.LvWFDetails.Count > 0)
                    //{
                    //    Status = Utility.GetStatusName().Where(e => e.Key == item.LvWFDetails.LastOrDefault().WFStatus.ToString())
                    //    .Select(e => e.Value).SingleOrDefault();
                    //}
                    //else
                    //{
                    //    Status = "Approved By HRM (M)";
                    //}


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

        public ActionResult EmployeeAppraisal(string employeeids)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (string.IsNullOrEmpty(Convert.ToString(SessionManager.EmpLvId)))
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }

                int Employeeid = 0;
                Employeeid = Convert.ToInt32(employeeids);
                var Id = Convert.ToInt32(SessionManager.EmpLvId);
                var db_data = db.EmployeeAppraisal
                      .Where(e => e.Id == Employeeid)
                      .Include(e => e.EmpAppEvaluation)
                      .Include(e => e.Employee)
                      .Include(e => e.EmpAppEvaluation.Select(a => a.EmpAppRatingConclusion))
                      .Include(e => e.EmpAppEvaluation.Select(a => a.EmpAppRatingConclusion.Select(t => t.AppraisalAssistance)))
                      .Include(e => e.EmpAppEvaluation.Select(a => a.EmpAppRatingConclusion.Select(t => t.EmpAppRating)))
                      .Include(e => e.EmpAppEvaluation.Select(a => a.EmpAppRatingConclusion.Select(t => t.EmpAppRating.Select(x => x.AppAssignment))))
                      .Include(e => e.EmpAppEvaluation.Select(a => a.EmpAppRatingConclusion.Select(t => t.EmpAppRating.Select(x => x.AppAssignment.AppCategory))))
                      .Include(e => e.EmpAppEvaluation.Select(a => a.EmpAppRatingConclusion.Select(t => t.EmpAppRating.Select(x => x.AppAssignment.AppSubCategory))))
                     .SingleOrDefault();

                // var LvCalendar = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "LEAVECALENDAR" && e.Default == true).SingleOrDefault();


                //if (listLvs != null && listLvs.Count() > 0)
                //{
                List<GetLvNewReqClass> returndata = new List<GetLvNewReqClass>();
                //returndata.Add(new GetLvNewReqClass
                //{
                //    Category = "Category Name",
                //    Subcategory = "Subcategory Name",
                //    MaxPoint = "Maxpoint",
                //    RatingPoint = "RatingPoint"
                //});
                //List<int> lvcode = listLvs.Where(e => e.LvCreditDate != null).Select(e => e.LeaveHead.Id).Distinct().ToList();
                //foreach (var item1 in lvcode)
                //{
                // DateTime? lvcrdate = listLvs.Where(a => a.LeaveHead.Id == item1 && a.LvCreditDate != null).Max(a => a.LvCreditDate.Value);
                //if (lvcrdate != null)
                //{

                foreach (var item in db_data.EmpAppEvaluation)
                {
                    var empratingdata = item.EmpAppRatingConclusion.Where(e => e.AppraisalAssistance == null).ToList();
                    foreach (var item1 in empratingdata)
                    {
                        foreach (var item2 in item1.EmpAppRating)
                        {
                            returndata.Add(new GetLvNewReqClass
                            {
                                //RowData = new ChildGetLvNewReqClass
                                //{
                                //    LvNewReq = item2.Id.ToString(),
                                //    EmpLVid = db_data.Employee.Id.ToString(),
                                //    // IsClose = Status,
                                //    //Status = Status,
                                //    // LvHead_Id = item.LeaveHead.Id.ToString(),
                                //},
                                Id = item2.AppAssignment.Id,
                                Category = item2.AppAssignment == null ? "" : item2.AppAssignment.AppCategory == null ? "" : item2.AppAssignment.AppCategory.Name,
                                Subcategory = item2.AppAssignment == null ? "" : item2.AppAssignment.AppSubCategory == null ? "" : item2.AppAssignment.AppSubCategory.Name,
                                MaxPoint = item2.AppAssignment == null ? "" : item2.AppAssignment.MaxRatingPoints.ToString(),
                                RatingPoint = "0",
                                Comment = ""
                            });
                        }
                    }
                    //var Status = "--";
                    //if (item.LvWFDetails.Count > 0)
                    //{
                    //    Status = Utility.GetStatusName().Where(e => e.Key == item.LvWFDetails.LastOrDefault().WFStatus.ToString())
                    //    .Select(e => e.Value).SingleOrDefault();
                    //}
                    //else
                    //{
                    //    Status = "Approved By HRM (M)";
                    //}


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

            public string CategoryName { get; set; }
            public string SubcategoryName { get; set; }
            public int Maxpoints { get; set; }
            public int RatingPoints { get; set; }
            public int EmployeeId { get; set; }
            //public string Req_Date { get; set; }
            //public string Branch { get; set; }
            //public string Department { get; set; }
            //public string Designation { get; set; }
            //public string SanctionCode { get; set; }
            //public string SanctionEmpname { get; set; }
            //public string SanctionComment { get; set; }
            //public string ApporavalComment { get; set; }
            //public string EmployeeComment { get; set; }
            //public string ProgrameName { get; set; }
            //public string Wf { get; set; }
            //public Int32 Id { get; set; }
            //public bool TrClosed { get; set; }
            //public string EmployeeName { get; set; }

            //public int EmployeeId { get; set; }
        }
        public class DeserializeClass
        {
            public string Id { get; set; }
            public string EmpEvaluationId { get; set; }
            public string AppriasalAssistance { get; set; }
            public string EmpAppRatingId { get; set; }
            public string Category { get; set; }
            public string SubCategory { get; set; }
            public string MaxPoints { get; set; }
            public string RatingPoints { get; set; }
            public string ObjectiveWordings { get; set; }
            public string Comments { get; set; }
            public string AppraiseePoints { get; set; }
            public string AppraiseeComments { get; set; }
            public string AppraiserPoints { get; set; }
            public string AppraiserComments { get; set; }
            public string HRPoints { get; set; }
            public string HRComments { get; set; }
        }
        public ActionResult Create(string forwarddata, FormCollection form) //Create submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string Emp = form["employee-table"] == null ? null : form["employee-table"];
                string AppraisalCalendar = form["AppCalendardrop"] == null ? null : form["AppCalendardrop"];
                //   string AppAssistance = form["AppraisalAssistancelist"] == null ? null : form["AppraisalAssistancelist"];
                string AppAssistance = form["AppraisalAssistancelist"] == "0" ? "" : form["AppraisalAssistancelist"];
                string AssistanceOverallCo = form["AssistanceOverallComments"] == "" ? "" : form["AssistanceOverallComments"];

                EmpAppRatingConclusion p = new EmpAppRatingConclusion();

                List<String> Msg = new List<String>();
                try
                {
                    List<int> ids1 = null;
                    if (Emp != null && Emp != "0" && Emp != "false")
                    {
                        ids1 = Utility.StringIdsToListIds(Emp);
                    }
                    else
                    {
                        List<string> Msgu = new List<string>();
                        Msgu.Add("  Kindly select employee  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msgu }, JsonRequestBehavior.AllowGet);
                    }
                    if (AssistanceOverallCo != null)
                    {
                        p.AssistanceOverallComments = AssistanceOverallCo.ToString();
                    }

                    var serialize = new JavaScriptSerializer();

                    var obj = serialize.Deserialize<List<DeserializeClass>>(forwarddata);

                    if (obj == null || obj.Count < 0)
                    {
                        return Json(new { sucess = true, responseText = "You have to change record to update." }, JsonRequestBehavior.AllowGet);
                    }
                    List<int> ids = obj.Select(e => int.Parse(e.Id)).ToList();
                    List<int> idss = obj.Select(e => int.Parse(e.Id)).ToList();
                    // List<int> idss2 = obj.Select(e => int.Parse(e.EmpEvaluationId)).ToList();
                    // var evalids = Convert.ToInt32(idss2[0]);
                    var iidd = Convert.ToInt32(idss[0]);

                    var iid = Convert.ToInt32(ids1[0]);
                    var q1 = db.Employee.Where(q => q.Id == (iid)).SingleOrDefault();

                    LookupValue val = db.LookupValue.Where(e => e.LookupVal.ToString().ToUpper() == "EMPLOYEEMYSELF").SingleOrDefault();
                    p.AppraisalAssistance = val;
                    //if (AppraisalCalendar != null && AppraisalCalendar != "")
                    //{
                    //    int AddId = Convert.ToInt32(AppraisalCalendar);
                    //    var val = db.Calendar.Include(e => e.Name).Where(e => e.Id == AddId).SingleOrDefault();
                    //    p.AppraisalPeriodCalendar = val;
                    //}
                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            p.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            List<EmpAppRating> AppRatingList = new List<EmpAppRating>();
                            int RatingPoints = 0;
                            int maxpoint = 0;
                            foreach (int ca in ids)
                            {
                                AppAssignment AppA = db.AppAssignment.Include(e => e.AppRatingObjective).Include(a => a.AppRatingObjective.Select(b => b.ObjectiveWordings)).Where(e => e.Id == ca).SingleOrDefault();
                                // var ep = db.EmpAppEvaluation.Include(a => a.EmpAppRatingConclusion.Select(b => b.EmpAppRating.Select(c => c.AppAssignment).Where(f => f.Id == AppA.Id))).SingleOrDefault();
                                RatingPoints = Convert.ToInt32(obj.Where(e => e.Id == ca.ToString()).Select(e => e.RatingPoints).Single());
                                maxpoint = Convert.ToInt32(obj.Where(e => e.Id == ca.ToString()).Select(e => e.MaxPoints).Single());
                                var AppRatingObj = AppA.AppRatingObjective.OrderBy(e => e.RatingPoints).ToList();

                                LookupValue wording = null;
                                foreach (var a in AppRatingObj)
                                {
                                    if (RatingPoints <= a.RatingPoints)
                                    {
                                        wording = a.ObjectiveWordings;
                                    }
                                }

                                EmpAppRating EmpAppR = new EmpAppRating()
                                {
                                    AppAssignment = AppA,
                                    Comments = obj.Where(e => e.Id == ca.ToString()).Select(e => e.Comments).Single(),
                                    ObjectiveWordings = wording,
                                    RatingPoints = RatingPoints,
                                    DBTrack = p.DBTrack,
                                };
                                AppRatingList.Add(EmpAppR);
                            }

                            EmpAppRatingConclusion Appcategory = new EmpAppRatingConclusion()
                            {
                                //AppraisalPeriodCalendar = p.AppraisalPeriodCalendar,
                                AppraisalAssistance = p.AppraisalAssistance,
                                DBTrack = p.DBTrack,
                                AssistanceOverallComments = p.AssistanceOverallComments,
                                EmpAppRating = AppRatingList
                            };

                            db.EmpAppRatingConclusion.Add(Appcategory);
                            //var rtn_Obj = DBTrackFile.DBTrackSave("Appraisal/Appraisal", null, db.ChangeTracker, p.DBTrack);
                            //     db.SaveChanges();

                            List<EmpAppRatingConclusion> EmpAppRatingConcl = new List<EmpAppRatingConclusion>();
                            EmpAppRatingConcl.Add(Appcategory);
                            //   db.SaveChanges();


                            //   var eavn = db.EmpAppEvaluation.Include(a => a.AppraisalPeriodCalendar).Include(a => a.EmpAppRatingConclusion).Where(a => a.Id == evalids).SingleOrDefault();
                            //  var AppAsslist1 = db.AppAssignment.Include(e => e.AppCategory).Include(e => e.AppSubCategory)
                            //              .Where(e => e.Id == evalids).SingleOrDefault();
                            //EmpAppEvaluation empappeval = new EmpAppEvaluation()
                            //{

                            //};
                            //if (eavn != null)
                            //{
                            //    eavn.SecurePoints = RatingPoints;
                            //    eavn.DBTrack = p.DBTrack;
                            //    eavn.EmpAppRatingConclusion = EmpAppRatingConcl,
                            //    db.EmpAppEvaluation.Attach(eavn);
                            //    db.Entry(eavn).State = System.Data.Entity.EntityState.Modified;
                            //    db.SaveChanges();
                            //}


                            EmpAppEvaluation evp = new EmpAppEvaluation()
                            {
                                EmpAppRatingConclusion = EmpAppRatingConcl,
                                SecurePoints = RatingPoints,
                                //AppraisalPeriodCalendar = ,
                                MaxPoints = maxpoint,
                                DBTrack = p.DBTrack,
                            };
                            db.EmpAppEvaluation.Add(evp);


                            db.SaveChanges();

                            //      var EmployeeAppraisal = new EmployeeAppraisal();


                            List<EmpAppEvaluation> AppcategoryLost = new List<EmpAppEvaluation>();
                            AppcategoryLost.Add(evp);



                            var empappr = db.EmployeeAppraisal.Where(a => a.Employee.Id == q1.Id).SingleOrDefault();
                            if (db.EmployeeAppraisal.Any(o => o.Employee.Id == q1.Id))
                            {

                                empappr.EmpAppEvaluation = AppcategoryLost;
                                empappr.DBTrack = p.DBTrack;
                                db.EmployeeAppraisal.Attach(empappr);
                                db.Entry(empappr).State = System.Data.Entity.EntityState.Modified;
                                //    db.SaveChanges();

                                //Msg.Add("Code Already Exists.");
                                //return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                empappr.DBTrack = p.DBTrack;
                                empappr.Employee = q1;
                                empappr.EmpAppEvaluation = AppcategoryLost;
                                db.EmployeeAppraisal.Add(empappr);
                            }

                            db.SaveChanges();

                            //if (db.EmployeeAppraisal.Any(o => o.Employee.Id == q1.Id))
                            //{
                            //    EmployeeAppraisal.EmpAppRatingConclusion = EmpAppRatingConcl;
                            //    db.EmployeeAppraisal.Attach(EmployeeAppraisal);
                            //    db.Entry(EmployeeAppraisal).State = System.Data.Entity.EntityState.Modified;

                            //}
                            //else
                            //{
                            //    EmployeeAppraisal.DBTrack = p.DBTrack;
                            //    EmployeeAppraisal.Employee = q1;
                            //    EmployeeAppraisal.EmpAppRatingConclusion = EmpAppRatingConcl;
                            //    db.EmployeeAppraisal.Add(EmployeeAppraisal);

                            //}

                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("Data Saved Successfully.");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
                    }
                }
                catch (Exception e)
                {
                    LogFile Logfile = new LogFile();
                    ErrorLog Err = new ErrorLog()
                    {
                        ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
                        ExceptionMessage = e.Message,
                        ExceptionStackTrace = e.StackTrace,
                        LineNo = Convert.ToString(new System.Diagnostics.StackTrace(e, true).GetFrame(0).GetFileLineNumber()),
                        LogTime = DateTime.Now
                    };
                    Logfile.CreateLogFile(Err);
                    Msg.Add(e.Message);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        public class RoasterClass
        {
            public int Id { get; set; }
            public string SNo { get; set; }
            public string CategoryName { get; set; }
            public string SubCategoryName { get; set; }
            public int MaxPoint { get; set; }
            public int RatingPoint { get; set; }
            public string Comment { get; set; }
        }
        public ActionResult CreateRoaster(List<RoasterClass> data, List<Int32> EmpId, bool overide = false)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> MSG = new List<string>();
                //List<returnObjClass> returnObjList = new List<returnObjClass>();
                EmpAppRatingConclusion p = new EmpAppRatingConclusion();

                LookupValue val = db.LookupValue.Where(e => e.LookupVal.ToString().ToUpper() == "APPRAISEE").SingleOrDefault();
                p.AppraisalAssistance = val;
                try
                {
                    foreach (var Id in EmpId)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            p.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            List<EmpAppRating> AppRatingList = new List<EmpAppRating>();
                            int RatingPoints = 0;
                            int maxpoint = 0;
                            var q1 = db.Employee.Where(q => q.Id == Id).SingleOrDefault();
                            foreach (RoasterClass item in data)
                            {
                                AppAssignment AppA = db.AppAssignment.Include(e => e.AppRatingObjective).Include(a => a.AppRatingObjective.Select(b => b.ObjectiveWordings)).Where(e => e.Id == item.Id).SingleOrDefault();
                                // var ep = db.EmpAppEvaluation.Include(a => a.EmpAppRatingConclusion.Select(b => b.EmpAppRating.Select(c => c.AppAssignment).Where(f => f.Id == AppA.Id))).SingleOrDefault();
                                RatingPoints = item.RatingPoint;
                                maxpoint = item.MaxPoint;
                                var AppRatingObj = AppA.AppRatingObjective.OrderBy(e => e.RatingPoints).ToList();

                                LookupValue wording = null;
                                foreach (var a in AppRatingObj)
                                {
                                    if (RatingPoints <= a.RatingPoints)
                                    {
                                        wording = a.ObjectiveWordings;
                                    }
                                }

                                EmpAppRating EmpAppR = new EmpAppRating()
                                {
                                    AppAssignment = AppA,
                                    Comments = item.Comment,
                                    ObjectiveWordings = wording,
                                    RatingPoints = RatingPoints,
                                    DBTrack = p.DBTrack,
                                };
                                AppRatingList.Add(EmpAppR);
                            }
                            EmpAppRatingConclusion Appcategory = new EmpAppRatingConclusion()
                            {
                                //AppraisalPeriodCalendar = p.AppraisalPeriodCalendar,
                                AppraisalAssistance = p.AppraisalAssistance,
                                DBTrack = p.DBTrack,
                                AssistanceOverallComments = p.AssistanceOverallComments,
                                EmpAppRating = AppRatingList
                            };

                            db.EmpAppRatingConclusion.Add(Appcategory);
                            //var rtn_Obj = DBTrackFile.DBTrackSave("Appraisal/Appraisal", null, db.ChangeTracker, p.DBTrack);
                            //     db.SaveChanges();

                            List<EmpAppRatingConclusion> EmpAppRatingConcl = new List<EmpAppRatingConclusion>();
                            EmpAppRatingConcl.Add(Appcategory);
                            //   db.SaveChanges();


                            //   var eavn = db.EmpAppEvaluation.Include(a => a.AppraisalPeriodCalendar).Include(a => a.EmpAppRatingConclusion).Where(a => a.Id == evalids).SingleOrDefault();
                            //  var AppAsslist1 = db.AppAssignment.Include(e => e.AppCategory).Include(e => e.AppSubCategory)
                            //              .Where(e => e.Id == evalids).SingleOrDefault();
                            //EmpAppEvaluation empappeval = new EmpAppEvaluation()
                            //{

                            //};
                            //if (eavn != null)
                            //{
                            //    eavn.SecurePoints = RatingPoints;
                            //    eavn.DBTrack = p.DBTrack;
                            //    eavn.EmpAppRatingConclusion = EmpAppRatingConcl,
                            //    db.EmpAppEvaluation.Attach(eavn);
                            //    db.Entry(eavn).State = System.Data.Entity.EntityState.Modified;
                            //    db.SaveChanges();
                            //}


                            EmpAppEvaluation evp = new EmpAppEvaluation()
                            {
                                EmpAppRatingConclusion = EmpAppRatingConcl,
                                SecurePoints = RatingPoints,
                                //AppraisalPeriodCalendar = ,
                                MaxPoints = maxpoint,
                                DBTrack = p.DBTrack,
                            };
                            db.EmpAppEvaluation.Add(evp);


                            db.SaveChanges();

                            //      var EmployeeAppraisal = new EmployeeAppraisal();


                            List<EmpAppEvaluation> AppcategoryLost = new List<EmpAppEvaluation>();
                            AppcategoryLost.Add(evp);



                            var empappr = db.EmployeeAppraisal.Where(a => a.Employee.Id == q1.Id).SingleOrDefault();
                            if (db.EmployeeAppraisal.Any(o => o.Employee.Id == q1.Id))
                            {

                                empappr.EmpAppEvaluation = AppcategoryLost;
                                empappr.DBTrack = p.DBTrack;
                                db.EmployeeAppraisal.Attach(empappr);
                                db.Entry(empappr).State = System.Data.Entity.EntityState.Modified;
                                //    db.SaveChanges();

                                //Msg.Add("Code Already Exists.");
                                //return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                empappr.DBTrack = p.DBTrack;
                                empappr.Employee = q1;
                                empappr.EmpAppEvaluation = AppcategoryLost;
                                db.EmployeeAppraisal.Add(empappr);
                            }

                            db.SaveChanges();

                            //if (db.EmployeeAppraisal.Any(o => o.Employee.Id == q1.Id))
                            //{
                            //    EmployeeAppraisal.EmpAppRatingConclusion = EmpAppRatingConcl;
                            //    db.EmployeeAppraisal.Attach(EmployeeAppraisal);
                            //    db.Entry(EmployeeAppraisal).State = System.Data.Entity.EntityState.Modified;

                            //}
                            //else
                            //{
                            //    EmployeeAppraisal.DBTrack = p.DBTrack;
                            //    EmployeeAppraisal.Employee = q1;
                            //    EmployeeAppraisal.EmpAppRatingConclusion = EmpAppRatingConcl;
                            //    db.EmployeeAppraisal.Add(EmployeeAppraisal);

                            //}

                            db.SaveChanges();
                            ts.Complete();
                            MSG.Add("Data Saved Successfully");
                        }
                        return Json(new { status = true, MSG = MSG }, JsonRequestBehavior.AllowGet);
                    }
                }
                catch (Exception e)
                {
                    MSG.Add(e.InnerException.Message.ToString());
                    return Json(MSG, JsonRequestBehavior.AllowGet);

                }
                return View();
            }
        }
        [HttpPost]
        public ActionResult Edit(string data)
        {
            //string tableName = "Corporate";

            //    // Fetch the table records dynamically
            //    var tableData = db.GetType()
            //    .GetProperty(tableName)
            //    .GetValue(db, null);
            var ids = Utility.StringIdsToListString(data);
            int emplvId = Convert.ToInt32(ids[0]);
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.ManpowerRequestPost.Include(e => e.FuncStruct)
                                                       .Include(e => e.FuncStruct.JobPosition)
                                                       .Include(e => e.FuncStruct.Job)
                                                       .Include(e => e.ExpFilter)
                                                       .Include(e => e.RangeFilter)
                                                       .Include(e => e.Qualification)
                                                       .Include(e => e.Skill)
                                                       .Include(e => e.Gender)
                                                       .Include(e => e.MaritalStatus)
                                                       .Include(e => e.CategoryPost)
                                                        .Include(e => e.PostSourceType)
                                                       .Include(e => e.CategorySplPost).Where(e => e.Id == emplvId).Select
                    (e => new
                    {
                        // JobPosition_Id = e.FuncStruct.JobPosition.Id==null? 0 : e.FuncStruct.JobPosition.Id ,
                        Vaccancy = e.RequestVacancies,
                        ExpFilter_Id = e.ExpFilter.Id == null ? 0 : e.ExpFilter.Id,
                        ExpYearFrom = e.ExpYearFrom,
                        ExpYearTo = e.ExpYearTo,
                        RangeFilter_Id = e.RangeFilter.Id == null ? 0 : e.RangeFilter.Id,
                        AgeFrom = e.AgeFrom,
                        AgeTo = e.AgeTo,
                        Gender_Id = e.Gender.Id == null ? 0 : e.Gender.Id,
                        PostSourceType = e.PostSourceType.Id == null ? 0 : e.PostSourceType.Id,
                        MaritalStatus_Id = e.MaritalStatus.Id == null ? 0 : e.MaritalStatus.Id,
                        Narration = e.Narration,
                        PostRequestDate = e.PostRequestDate,
                        PostCode = e.PostCode,
                        Action = e.DBTrack.Action
                    }).ToList();

                var add_data = db.ManpowerRequestPost.Include(e => e.FuncStruct)
                                                       .Include(e => e.FuncStruct.JobPosition)
                                                       .Include(e => e.FuncStruct.Job)
                                                       .Include(e => e.ExpFilter)
                                                       .Include(e => e.RangeFilter)
                                                       .Include(e => e.Qualification)
                                                       .Include(e => e.Skill)
                                                       .Include(e => e.Gender)
                                                       .Include(e => e.MaritalStatus)
                                                       .Include(e => e.CategoryPost)
                                                       .Include(e => e.CategorySplPost)
                                                       .Include(e => e.CategoryPost.Select(e1 => e1.Category))
                                                       .Include(e => e.CategorySplPost.Select(e1 => e1.SpecialCategory))
                                                       .Where(e => e.Id == emplvId).Select
                    (e => new
                    {
                        JobPos_FullAddress = e.FuncStruct == null ? "" : e.FuncStruct.FullDetails,
                        JobPos_Id = e.FuncStruct.Id == null ? "" : e.FuncStruct.Id.ToString(),


                        QualificationDetails_Id = e.Qualification.Select(b => b.Id.ToString()),
                        QualificationDetails_val = e.Qualification.Select(b => b.FullDetails),
                        SkillDetails_Id = e.Skill.Select(b => b.Id.ToString()),
                        SkillDetails_val = e.Skill.Select(b => b.Name),
                        CategoryDetails_Id = e.CategoryPost.Select(b => b.Id.ToString()),
                        CategoryDetails_val = e.CategoryPost.Select(b => b.FullDetails),
                        CategorySplDetails_Id = e.CategorySplPost.Select(b => b.Id.ToString()),
                        CategorySplDetails_val = e.CategorySplPost.Select(b => b.FullDetails),

                    }).ToList();

                //foreach (var ca in add_data)
                //{


                //}


                //List<PostDetailsD> pst =new List<PostDetailsD>();
                //var a = db.PostDetails.Include(e => e.FuncStruct)
                //                        .Include(e => e.FuncStruct.JobPosition)
                //                        .Include(e => e.FuncStruct.Job)
                //                        .Include(e => e.ExpFilter)
                //                        .Include(e => e.RangeFilter)
                //                        .Include(e => e.Qualification)
                //                        .Include(e => e.Skill)
                //                        .Include(e => e.Gender)
                //                        .Include(e => e.MaritalStatus)
                //                        .Include(e => e.CategoryPost.Select(e1=>e1.Category))
                //                        .Include(e => e.CategorySplPost.Select(e1=>e1.SpecialCategory)).Where(e => e.Id == data).ToList();
                //foreach (var ca in a)
                //{
                //    pst.Add(new PostDetailsD
                //    {

                //        QualificationDetails_Id = ca.Qualification.Select(e => e.Id.ToString()).ToArray(),
                //        QualificationDetails_val = ca.Qualification.Select(e => e.FullDetails).ToArray(),
                //        SkillDetails_Id = ca.Skill.Select(e => e.Id.ToString()).ToArray(),
                //        SkillDetails_val = ca.Skill.Select(e => e.Name).ToArray(),
                //        CategoryDetails_Id = ca.CategoryPost.Select(e => e.Id.ToString()).ToArray(),
                //        CategoryDetails_val = ca.CategoryPost.Select(e => e.FullDetails).ToArray(),
                //        CategorySplDetails_Id = ca.CategorySplPost.Select(e => e.Id.ToString()).ToArray(),
                //        CategorySplDetails_val = ca.CategorySplPost.Select(e => e.FullDetails).ToArray(),
                //     });
                //}

                //var W = db.DT_Corporate
                //     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                //     (e => new
                //     {
                //         DT_Id = e.Id,
                //         Code = e.Code == null ? "" : e.Code,
                //         Name = e.Name == null ? "" : e.Name,
                //         BusinessType_Val = e.BusinessType_Id == 0 ? "" : db.LookupValue
                //                    .Where(x => x.Id == e.BusinessType_Id)
                //                    .Select(x => x.LookupVal).FirstOrDefault(),

                //         Address_Val = e.Address_Id == 0 ? "" : db.Address.Where(x => x.Id == e.Address_Id).Select(x => x.FullAddress).FirstOrDefault(),
                //         Contact_Val = e.ContactDetails_Id == 0 ? "" : db.ContactDetails.Where(x => x.Id == e.ContactDetails_Id).Select(x => x.FullContactDetails).FirstOrDefault()
                //     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var Corp = db.ManpowerRequestPost.Find(emplvId);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, add_data, "", Auth, JsonRequestBehavior.AllowGet });
            }
        }

        [HttpPost]
        public async Task<ActionResult> EditSave(ManpowerRequestPost L, String data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            var ids = Utility.StringIdsToListString(data);
            int emplvId = Convert.ToInt32(ids[0]);
            using (DataBaseContext db = new DataBaseContext())
                try
                {
                    string JobPosition = form["JobPositionMlist"] == "0" ? "" : form["JobPositionMlist"];
                    string ExpFilter_Id = form["ExpFilterlist"] == "0" ? "" : form["ExpFilterlist"];
                    string RangeFilter_Id = form["RangeFilterlist"] == "0" ? "" : form["RangeFilterlist"];
                    string Gender = form["Gender_drop"] == "0" ? "" : form["Gender_drop"];
                    string MStatus = form["MaritalStatus_drop"] == "0" ? "" : form["MaritalStatus_drop"];
                    string PostSourceType = form["PostSourceTypelist"] == "0" ? "" : form["PostSourceTypelist"];
                    var blog1 = db.ManpowerRequestPost.Where(e => e.Id == emplvId).Include(e => e.FuncStruct)
                                                    .Include(e => e.FuncStruct.JobPosition)
                                                    .Include(e => e.FuncStruct.Job)
                                                    .Include(e => e.ExpFilter)
                                                    .Include(e => e.RangeFilter)
                                                    .Include(e => e.Qualification)
                                                    .Include(e => e.Skill)
                                                    .Include(e => e.Gender)
                                                    .Include(e => e.MaritalStatus)
                                                    .Include(e => e.CategoryPost)
                                                    .Include(e => e.CategoryPost.Select(q => q.Category))
                                                    .Include(e => e.CategorySplPost)
                                                    .Include(e => e.PostSourceType)
                                                    .Include(e => e.CategorySplPost.Select(q => q.SpecialCategory))
                                                              .SingleOrDefault();

                    blog1.FuncStruct = null;
                    blog1.Skill = null;
                    blog1.Qualification = null;
                    blog1.CategoryPost = null;
                    blog1.CategorySplPost = null;

                    if (L.Narration != null)
                    {
                        blog1.Narration = L.Narration.ToString();
                    }
                    // blog1.Narration = L.Narration;
                    blog1.RequestVacancies = L.RequestVacancies;
                    blog1.ExpYearFrom = L.ExpYearFrom;
                    blog1.ExpYearTo = L.ExpYearTo;
                    blog1.AgeFrom = L.AgeFrom;
                    blog1.AgeTo = L.AgeTo;
                    blog1.PostCode = L.PostCode;
                    blog1.PostRequestDate = L.PostRequestDate;


                    if (JobPosition != null)
                    {
                        if (JobPosition != "")
                        {
                            int ContId = Convert.ToInt32(JobPosition);
                            var val = db.FuncStruct.Include(e => e.Job).Include(e => e.JobPosition)
                                                .Where(e => e.Id == ContId).SingleOrDefault();
                            blog1.FuncStruct = val;
                        }
                    }

                    List<Skill> ObjITsection = new List<Skill>();
                    ManpowerRequestPost pd = null;
                    pd = db.ManpowerRequestPost.Include(e => e.Skill).Where(e => e.Id == emplvId).SingleOrDefault();
                    string Skill1 = form["SkillList"];
                    if (Skill1 != null && Skill1 != "")
                    {
                        var ids5 = Utility.StringIdsToListIds(Skill1);
                        foreach (var ca in ids5)
                        {
                            var value = db.Skill.Find(ca);
                            ObjITsection.Add(value);
                            pd.Skill = ObjITsection;

                        }
                    }
                    else
                    {
                        pd.Skill = null;
                    }

                    List<Qualification> ObjQualification = new List<Qualification>();
                    ManpowerRequestPost pd1 = null;
                    pd1 = db.ManpowerRequestPost.Include(e => e.Qualification).Where(e => e.Id == emplvId).SingleOrDefault();
                    string quali = form["QualificationlistM"];
                    if (quali != null && quali != "")
                    {
                        var ids4 = Utility.StringIdsToListIds(quali);
                        foreach (var ca in ids4)
                        {
                            var value = db.Qualification.Find(ca);
                            ObjQualification.Add(value);
                            pd1.Qualification = ObjQualification;

                        }
                    }
                    else
                    {
                        pd1.Qualification = null;
                    }

                    //List<Skill> AllergyVal = new List<Skill>();
                    //string Skill1 = form["SkillList"];
                    //if (Skill1 != null)
                    //{
                    //    var ids = Utility.StringIdsToListIds(Skill1);
                    //    foreach (var ca in ids)
                    //    {
                    //        var Allergy_val = db.Skill.Find(ca);
                    //        AllergyVal.Add(Allergy_val);
                    //        blog1.Skill = AllergyVal;
                    //    }
                    //}
                    //List<Qualification> qual = new List<Qualification>();
                    //string quali = form["QualificationlistM"];
                    //if (quali != null)
                    //{
                    //    var ids = Utility.StringIdsToListIds(quali);
                    //    foreach (var ca in ids)
                    //    {
                    //        var Allergy_val = db.Qualification.Find(ca);
                    //        qual.Add(Allergy_val);
                    //        blog1.Qualification = qual;
                    //    }
                    //}

                    List<CategoryPost> cat = new List<CategoryPost>();
                    string catr = form["CategoryPostlist"];
                    if (catr != null)
                    {
                        var ids2 = Utility.StringIdsToListIds(catr);
                        foreach (var ca in ids2)
                        {
                            var Allergy_val = db.CategoryPost.Find(ca);
                            cat.Add(Allergy_val);
                            blog1.CategoryPost = cat;
                        }
                    }

                    List<CategorySplPost> cat1 = new List<CategorySplPost>();
                    string catr1 = form["CategorySplPostlist"];
                    if (catr1 != null)
                    {
                        var ids1 = Utility.StringIdsToListIds(catr1);
                        foreach (var ca in ids1)
                        {
                            var Allergy_val = db.CategorySplPost.Find(ca);
                            cat1.Add(Allergy_val);
                            blog1.CategorySplPost = cat1;
                        }
                    }

                    if (ModelState.IsValid)
                    {
                        try
                        {
                            //  using (DataBaseContext db = new DataBaseContext())
                            {
                                //DbContextTransaction transaction = db.Database.BeginTransaction();

                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    // PostDetails blog = null; // to retrieve old data
                                    // DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        //blog = context.PostDetails.Where(e => e.Id == data).Include(e => e.FuncStruct)
                                        //              .Include(e => e.FuncStruct.JobPosition)                                    
                                        //              .Include(e=>e.FuncStruct.Job)
                                        //              .Include(e => e.ExpFilter)
                                        //              .Include(e => e.RangeFilter)
                                        //              .Include(e => e.Qualification)
                                        //              .Include(e => e.Skill)
                                        //              .Include(e => e.Gender)
                                        //              .Include(e => e.MaritalStatus)
                                        //              .Include(e => e.CategoryPost)
                                        //              .Include(e => e.CategoryPost.Select(q=>q.Category))
                                        //              .Include(e => e.CategorySplPost)
                                        //              .Include(e => e.CategorySplPost.Select(q => q.SpecialCategory))
                                        //                        .SingleOrDefault();
                                        //originalBlogValues = context.Entry(blog).OriginalValues;
                                    }

                                    blog1.DBTrack = new DBTrack
                                    {
                                        CreatedBy = blog1.DBTrack.CreatedBy == null ? null : blog1.DBTrack.CreatedBy,
                                        CreatedOn = blog1.DBTrack.CreatedOn == null ? null : blog1.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = SessionManager.UserName,
                                        ModifiedOn = DateTime.Now
                                    };

                                    if (ExpFilter_Id != null)
                                    {
                                        if (ExpFilter_Id != "")
                                        {
                                            var val = db.LookupValue.Find(int.Parse(ExpFilter_Id));
                                            blog1.ExpFilter = val;

                                            var type = db.ManpowerRequestPost.Include(e => e.ExpFilter).Where(e => e.Id == emplvId).SingleOrDefault();
                                            IList<ManpowerRequestPost> typedetails = null;
                                            if (type.ExpFilter != null)
                                            {
                                                typedetails = db.ManpowerRequestPost.Where(x => x.ExpFilter.Id == type.ExpFilter.Id && x.Id == emplvId).ToList();
                                            }
                                            else
                                            {
                                                typedetails = db.ManpowerRequestPost.Where(x => x.Id == emplvId).ToList();
                                            }
                                            //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                                            foreach (var s in typedetails)
                                            {
                                                s.ExpFilter = blog1.ExpFilter;
                                                db.ManpowerRequestPost.Attach(s);
                                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                                //await db.SaveChangesAsync();
                                                db.SaveChanges();
                                                TempData["RowVersion"] = s.RowVersion;
                                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                            }
                                        }
                                        else
                                        {
                                            var WFTypeDetails = db.ManpowerRequestPost.Include(e => e.ExpFilter).Where(x => x.Id == emplvId).ToList();
                                            foreach (var s in WFTypeDetails)
                                            {
                                                s.ExpFilter = null;
                                                db.ManpowerRequestPost.Attach(s);
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
                                        var CreditdateypeDetails = db.ManpowerRequestPost.Include(e => e.ExpFilter).Where(x => x.Id == emplvId).ToList();
                                        foreach (var s in CreditdateypeDetails)
                                        {
                                            s.ExpFilter = null;
                                            db.ManpowerRequestPost.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }


                                    if (RangeFilter_Id != null)
                                    {
                                        if (RangeFilter_Id != "")
                                        {
                                            var val = db.LookupValue.Find(int.Parse(RangeFilter_Id));
                                            blog1.RangeFilter = val;

                                            var type = db.ManpowerRequestPost.Include(e => e.RangeFilter).Where(e => e.Id == emplvId).SingleOrDefault();
                                            IList<ManpowerRequestPost> typedetails = null;
                                            if (type.RangeFilter != null)
                                            {
                                                typedetails = db.ManpowerRequestPost.Where(x => x.RangeFilter.Id == type.RangeFilter.Id && x.Id == emplvId).ToList();
                                            }
                                            else
                                            {
                                                typedetails = db.ManpowerRequestPost.Where(x => x.Id == emplvId).ToList();
                                            }
                                            //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                                            foreach (var s in typedetails)
                                            {
                                                s.RangeFilter = blog1.RangeFilter;
                                                db.ManpowerRequestPost.Attach(s);
                                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                                //await db.SaveChangesAsync();
                                                db.SaveChanges();
                                                TempData["RowVersion"] = s.RowVersion;
                                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                            }
                                        }
                                        else
                                        {
                                            var WFTypeDetails = db.ManpowerRequestPost.Include(e => e.RangeFilter).Where(x => x.Id == emplvId).ToList();
                                            foreach (var s in WFTypeDetails)
                                            {
                                                s.RangeFilter = null;
                                                db.ManpowerRequestPost.Attach(s);
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
                                        var CreditdateypeDetails = db.ManpowerRequestPost.Include(e => e.RangeFilter).Where(x => x.Id == emplvId).ToList();
                                        foreach (var s in CreditdateypeDetails)
                                        {
                                            s.RangeFilter = null;
                                            db.ManpowerRequestPost.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }

                                    if (Gender != null)
                                    {
                                        if (Gender != "")
                                        {
                                            var val = db.LookupValue.Find(int.Parse(Gender));
                                            blog1.Gender = val;

                                            var type = db.ManpowerRequestPost.Include(e => e.Gender).Where(e => e.Id == emplvId).SingleOrDefault();
                                            IList<ManpowerRequestPost> typedetails = null;
                                            if (type.Gender != null)
                                            {
                                                typedetails = db.ManpowerRequestPost.Where(x => x.Gender.Id == type.Gender.Id && x.Id == emplvId).ToList();
                                            }
                                            else
                                            {
                                                typedetails = db.ManpowerRequestPost.Where(x => x.Id == emplvId).ToList();
                                            }
                                            //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                                            foreach (var s in typedetails)
                                            {
                                                s.Gender = blog1.Gender;
                                                db.ManpowerRequestPost.Attach(s);
                                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                                //await db.SaveChangesAsync();
                                                db.SaveChanges();
                                                TempData["RowVersion"] = s.RowVersion;
                                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                            }
                                        }
                                        else
                                        {
                                            var WFTypeDetails = db.ManpowerRequestPost.Include(e => e.Gender).Where(x => x.Id == emplvId).ToList();
                                            foreach (var s in WFTypeDetails)
                                            {
                                                s.Gender = null;
                                                db.ManpowerRequestPost.Attach(s);
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
                                        var CreditdateypeDetails = db.ManpowerRequestPost.Include(e => e.Gender).Where(x => x.Id == emplvId).ToList();
                                        foreach (var s in CreditdateypeDetails)
                                        {
                                            s.Gender = null;
                                            db.ManpowerRequestPost.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }

                                    if (PostSourceType != null)
                                    {
                                        if (PostSourceType != "")
                                        {
                                            var val = db.LookupValue.Where(e=>e.LookupVal.ToString().ToUpper()=="DEPTREQUEST").SingleOrDefault();
                                            blog1.PostSourceType = val;

                                            var type = db.ManpowerRequestPost.Include(e => e.PostSourceType).Where(e => e.Id == emplvId).SingleOrDefault();
                                            IList<ManpowerRequestPost> typedetails = null;
                                            if (type.PostSourceType != null)
                                            {
                                                typedetails = db.ManpowerRequestPost.Where(x => x.PostSourceType.Id == type.PostSourceType.Id && x.Id == emplvId).ToList();
                                            }
                                            else
                                            {
                                                typedetails = db.ManpowerRequestPost.Where(x => x.Id == emplvId).ToList();
                                            }
                                            //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                                            foreach (var s in typedetails)
                                            {
                                                s.PostSourceType = blog1.PostSourceType;
                                                db.ManpowerRequestPost.Attach(s);
                                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                                //await db.SaveChangesAsync();
                                                db.SaveChanges();
                                                TempData["RowVersion"] = s.RowVersion;
                                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                            }
                                        }
                                        else
                                        {
                                            var WFTypeDetails = db.ManpowerRequestPost.Include(e => e.PostSourceType).Where(x => x.Id == emplvId).ToList();
                                            foreach (var s in WFTypeDetails)
                                            {
                                                s.PostSourceType = null;
                                                db.ManpowerRequestPost.Attach(s);
                                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                                //await db.SaveChangesAsync();
                                                db.SaveChanges();
                                                TempData["RowVersion"] = s.RowVersion;
                                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                            }
                                        }
                                    }
                                    //else
                                    //{
                                    //    var CreditdateypeDetails = db.ManpowerRequestPost.Include(e => e.Gender).Where(x => x.Id == emplvId).ToList();
                                    //    foreach (var s in CreditdateypeDetails)
                                    //    {
                                    //        s.Gender = null;
                                    //        db.ManpowerRequestPost.Attach(s);
                                    //        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                    //        //await db.SaveChangesAsync();
                                    //        db.SaveChanges();
                                    //        TempData["RowVersion"] = s.RowVersion;
                                    //        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                    //    }
                                    //}

                                    if (MStatus != null)
                                    {
                                        if (MStatus != "")
                                        {
                                            var val = db.LookupValue.Find(int.Parse(MStatus));
                                            blog1.MaritalStatus = val;

                                            var type = db.ManpowerRequestPost.Include(e => e.MaritalStatus).Where(e => e.Id == emplvId).SingleOrDefault();
                                            IList<ManpowerRequestPost> typedetails = null;
                                            if (type.MaritalStatus != null)
                                            {
                                                typedetails = db.ManpowerRequestPost.Where(x => x.MaritalStatus.Id == type.MaritalStatus.Id && x.Id == emplvId).ToList();
                                            }
                                            else
                                            {
                                                typedetails = db.ManpowerRequestPost.Where(x => x.Id == emplvId).ToList();
                                            }
                                            //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                                            foreach (var s in typedetails)
                                            {
                                                s.MaritalStatus = blog1.MaritalStatus;
                                                db.ManpowerRequestPost.Attach(s);
                                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                                //await db.SaveChangesAsync();
                                                db.SaveChanges();
                                                TempData["RowVersion"] = s.RowVersion;
                                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                            }
                                        }
                                        else
                                        {
                                            var WFTypeDetails = db.ManpowerRequestPost.Include(e => e.MaritalStatus).Where(x => x.Id == emplvId).ToList();
                                            foreach (var s in WFTypeDetails)
                                            {
                                                s.MaritalStatus = null;
                                                db.ManpowerRequestPost.Attach(s);
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
                                        var CreditdateypeDetails = db.ManpowerRequestPost.Include(e => e.MaritalStatus).Where(x => x.Id == emplvId).ToList();
                                        foreach (var s in CreditdateypeDetails)
                                        {
                                            s.MaritalStatus = null;
                                            db.ManpowerRequestPost.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }

                                    if (JobPosition != null)
                                    {
                                        if (JobPosition != "")
                                        {
                                            var val = db.FuncStruct.Find(int.Parse(JobPosition));
                                            blog1.FuncStruct = val;

                                            var type = db.ManpowerRequestPost.Include(e => e.FuncStruct).Include(e => e.FuncStruct.Job).Include(e => e.FuncStruct.JobPosition).Where(e => e.Id == emplvId).SingleOrDefault();
                                            IList<ManpowerRequestPost> typedetails = null;
                                            if (type.FuncStruct != null)
                                            {
                                                typedetails = db.ManpowerRequestPost.Where(x => x.FuncStruct.Id == type.FuncStruct.Id && x.Id == emplvId).ToList();
                                            }
                                            else
                                            {
                                                typedetails = db.ManpowerRequestPost.Where(x => x.Id == emplvId).ToList();
                                            }
                                            //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                                            foreach (var s in typedetails)
                                            {
                                                s.FuncStruct = blog1.FuncStruct;
                                                db.ManpowerRequestPost.Attach(s);
                                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                                //await db.SaveChangesAsync();
                                                db.SaveChanges();
                                                TempData["RowVersion"] = s.RowVersion;
                                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                            }
                                        }
                                        else
                                        {
                                            var WFTypeDetails = db.ManpowerRequestPost.Include(e => e.FuncStruct).Include(e => e.FuncStruct.Job).Include(e => e.FuncStruct.JobPosition).Where(x => x.Id == emplvId).ToList();
                                            foreach (var s in WFTypeDetails)
                                            {
                                                s.FuncStruct = null;
                                                db.ManpowerRequestPost.Attach(s);
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
                                        var CreditdateypeDetails = db.ManpowerRequestPost.Include(e => e.FuncStruct).Include(e => e.FuncStruct.Job).Include(e => e.FuncStruct.JobPosition).Where(x => x.Id == emplvId).ToList();
                                        foreach (var s in CreditdateypeDetails)
                                        {
                                            s.FuncStruct = null;
                                            db.ManpowerRequestPost.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }


                                    var CurCorp = db.ManpowerRequestPost.Find(emplvId);
                                    TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                    db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {

                                        ManpowerRequestPost post = new ManpowerRequestPost()
                                        {
                                            RequestVacancies = blog1.RequestVacancies,
                                            ExpYearFrom = blog1.ExpYearFrom,
                                            ExpYearTo = blog1.ExpYearTo,
                                            AgeFrom = blog1.AgeFrom,
                                            AgeTo = blog1.AgeTo,
                                            Skill = blog1.Skill,
                                            Qualification = blog1.Qualification,
                                            CategoryPost = blog1.CategoryPost,
                                            CategorySplPost = blog1.CategorySplPost,
                                            Narration = blog1.Narration,
                                            FuncStruct = blog1.FuncStruct,
                                            PostCode = blog1.PostCode,
                                            PostRequestDate = blog1.PostRequestDate,
                                            Id = emplvId,
                                            DBTrack = blog1.DBTrack
                                        };
                                        db.ManpowerRequestPost.Attach(post);
                                        db.Entry(post).State = System.Data.Entity.EntityState.Modified;

                                        db.Entry(post).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                        db.SaveChanges();

                                        await db.SaveChangesAsync();
                                        db.Entry(post).State = System.Data.Entity.EntityState.Detached;
                                        ts.Complete();
                                        Msg.Add("Record Updated");
                                        //return Json(new Utility.JsonReturnClass { Id = blog1.Id, Val = blog1.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        // return Json(new Object[] { L.Id, L.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });
                                        return Json(new { status = true, responseText = "Record Updated." }, JsonRequestBehavior.AllowGet);

                                    }
                                }
                            }
                        }
                        catch (DbUpdateConcurrencyException ex)
                        {
                            var entry = ex.Entries.Single();
                            var clientValues = (PostDetails)entry.Entity;
                            var databaseEntry = entry.GetDatabaseValues();
                            if (databaseEntry == null)
                            {
                                Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                            }
                            else
                            {
                                var databaseValues = (PostDetails)databaseEntry.ToObject();
                                blog1.RowVersion = databaseValues.RowVersion;

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
                // var LvHeadId = ids.Count > 0 ? ids[3] : null;

                // var lvheadidint = Convert.ToInt32(LvHeadId);
                var EmpLvIdint = Convert.ToInt32(emplvId);

                var W = db.EmpAppRating
                    .Include(e => e.AppAssignment)
                    .Include(e => e.AppAssignment.AppCategory)
                    .Include(e => e.AppAssignment.AppSubCategory)
                    .Where(e => e.Id == id).Select(e => new EmpmLVdata
                    {
                        CategoryName = e.AppAssignment == null ? "" : e.AppAssignment.AppCategory == null ? "" : e.AppAssignment.AppCategory.Name,
                        SubcategoryName = e.AppAssignment == null ? "" : e.AppAssignment.AppSubCategory == null ? "" : e.AppAssignment.AppSubCategory.Name,
                        Maxpoints = e.AppAssignment == null ? 0 : e.AppAssignment.MaxRatingPoints,
                        RatingPoints = e.RatingPoints,
                        EmployeeId = EmpLvIdint,
                    }).SingleOrDefault();

                //var v = W.Select(s => new EmpmLVdata
                //{
                //    EmployeeId = W.Employee.Id,
                //    EmployeeName = W.Employee.EmpCode + " " + W.Employee.EmpName.FullNameFML,
                //    Branch = W.Employee.GeoStruct != null && W.Employee.GeoStruct.Location != null && W.Employee.GeoStruct.Location.LocationObj != null ? W.Employee.GeoStruct.Location.LocationObj.LocDesc.ToString() : null,
                //    Department = W.Employee.GeoStruct != null && W.Employee.GeoStruct.Department != null && W.Employee.GeoStruct.Department.DepartmentObj != null ? W.Employee.GeoStruct.Department.DepartmentObj.DeptDesc.ToString() : null,
                //    Designation = W.Employee.FuncStruct != null && W.Employee.FuncStruct.Job != null ? W.Employee.FuncStruct.Job.Name : null,
                //    Status = status,
                //    Id = s.Id,
                //    TrClosed = s.TrClosed,
                //    ProgrameName = s.ProgramList != null ? s.ProgramList.Subject : null,
                //    Req_Date = s.RequisitionDate.Value.ToShortDateString(),
                //    EmployeeComment = s.LvWFDetails != null && s.LvWFDetails.Count > 0 ? s.LvWFDetails.Where(z => z.WFStatus == 0).OrderByDescending(e => e.Id).Select(e => e.Comments).FirstOrDefault() : null,
                //    SanctionCode = s.LvWFDetails != null && s.LvWFDetails.Count > 0 ? s.LvWFDetails.Where(z => WfStatus.Contains(z.WFStatus)).OrderByDescending(e => e.Id).Select(e => e.DBTrack.CreatedBy).FirstOrDefault() : null,
                //    SanctionComment = s.LvWFDetails != null && s.LvWFDetails.Count > 0 ? s.LvWFDetails.Where(z => SanctionStatus.Contains(z.WFStatus)).OrderByDescending(e => e.Id).Select(e => e.Comments).FirstOrDefault() : null,
                //    ApporavalComment = s.LvWFDetails != null && s.LvWFDetails.Count > 0 ? s.LvWFDetails.Where(z => ApporvalStatus.Contains(z.WFStatus)).OrderByDescending(e => e.Id).Select(e => e.Comments).FirstOrDefault() : null,
                //    Wf = s.LvWFDetails != null && s.LvWFDetails.Count > 0 ? s.LvWFDetails.Where(z => WfStatus.Contains(z.WFStatus)).OrderByDescending(e => e.Id).Select(e => e.Comments).LastOrDefault() : null
                //}).SingleOrDefault();


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
                listOfObject.Add(W);
                return Json(listOfObject, JsonRequestBehavior.AllowGet);
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

        //public ActionResult Edit(string data)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var Emp = Convert.ToInt32(SessionManager.EmpId);
        //        var ids = Utility.StringIdsToListString(data);
        //        var id = Convert.ToInt32(ids[0]);
        //        var qurey = db.Employee.Include(e => e.EmpAcademicInfo)
        //            .Include(e => e.EmpAcademicInfo.Skill)

        //            .Where(e => e.Id == Emp && e.EmpAcademicInfo != null)
        //            .AsEnumerable().Select(e => new
        //            {
        //                Skill = e.EmpAcademicInfo.Skill.Where(w => w.Id == id).SingleOrDefault(),
        //                DBTrack = e.DBTrack
        //            }).SingleOrDefault();

        //        var returndata = (Object)null;
        //        var returnCurrentData = (Object)null;
        //        if (qurey != null)
        //        {
        //            if (qurey.Skill != null)
        //            {
        //                returndata = new
        //                {
        //                    id = qurey.Skill.Id,
        //                    Name = qurey.Skill.Name,
        //                    Action = qurey.Skill.DBTrack.Action,
        //                    isauth = true,
        //                    Add = false
        //                };

        //                //curr data
        //                var dt_data = db.DT_Skill.Where(e => e.Orig_Id == qurey.Skill.Id && e.DBTrack.IsAuthorized == 0).OrderByDescending(e => e.Id).FirstOrDefault();
        //                if (dt_data != null)
        //                {
        //                    returnCurrentData = new
        //                    {
        //                        Action = qurey.DBTrack.Action,
        //                    };
        //                }
        //            }
        //            else
        //            {
        //                returndata = new
        //                {
        //                    Add = true,
        //                };
        //            }

        //            return Json(new Object[] { returndata, returnCurrentData, "", JsonRequestBehavior.AllowGet });
        //        }
        //        return Json(new Object[] { returndata, returnCurrentData, "", JsonRequestBehavior.AllowGet });
        //    }
        //}


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



        //[HttpPost]
        //public Object EditSave(Skill c, int data, FormCollection form) // Edit submit
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {


        //        // bool Auth = form["autho_allow"] == "true" ? true : false;
        //        bool Auth = true;


        //        if (Auth == false)
        //        {
        //            if (ModelState.IsValid)
        //            {
        //                try
        //                {
        //                    var db_data = db.Skill.Include(e => e.SkillType)
        //                                           .Where(e => e.Id == data).SingleOrDefault();


        //                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                    {
        //                        db.Skill.Attach(db_data);
        //                        db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
        //                        db.SaveChanges();
        //                        TempData["RowVersion"] = db_data.RowVersion;
        //                        db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;

        //                        var Curr_Lookup = db.Skill.Find(data);
        //                        TempData["CurrRowVersion"] = Curr_Lookup.RowVersion;
        //                        db.Entry(Curr_Lookup).State = System.Data.Entity.EntityState.Detached;

        //                        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
        //                        {

        //                            Skill blog = null; // to retrieve old data
        //                            DbPropertyValues originalBlogValues = null;

        //                            using (var context = new DataBaseContext())
        //                            {
        //                                blog = context.Skill.Include(e => e.SkillType)
        //                                           .Where(e => e.Id == data).SingleOrDefault();

        //                                originalBlogValues = context.Entry(blog).OriginalValues;
        //                            }

        //                            c.DBTrack = new DBTrack
        //                            {
        //                                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                                Action = "M",
        //                                ModifiedBy = "0029",
        //                                ModifiedOn = DateTime.Now
        //                            };
        //                            Skill lk = new Skill
        //                            {
        //                                Id = data,
        //                                Name = c.Name,

        //                                DBTrack = c.DBTrack,
        //                                FullDetails = c.FullDetails
        //                            };


        //                            db.Skill.Attach(lk);
        //                            db.Entry(lk).State = System.Data.Entity.EntityState.Modified;

        //                            db.Entry(lk).OriginalValues["RowVersion"] = TempData["RowVersion"];


        //                            using (var context = new DataBaseContext())
        //                            {

        //                                var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
        //                                DT_Skill DT_Corp = (DT_Skill)obj;

        //                                db.Create(DT_Corp);
        //                                db.SaveChanges();
        //                            }
        //                            //  await db.SaveChangesAsync();
        //                            ts.Complete();


        //                            return new { status = true, responseText = "Record Updated" };
        //                        }
        //                    }
        //                }
        //                catch (DbUpdateConcurrencyException ex)
        //                {
        //                    var entry = ex.Entries.Single();
        //                    var clientValues = (Skill)entry.Entity;
        //                    var databaseEntry = entry.GetDatabaseValues();
        //                    if (databaseEntry == null)
        //                    {
        //                        return new { status = true, responseText = "Unable to save changes. The record was deleted by another user." };
        //                    }
        //                    else
        //                    {
        //                        var databaseValues = (Skill)databaseEntry.ToObject();
        //                        c.RowVersion = databaseValues.RowVersion;

        //                    }
        //                }

        //                return new { status = true, responseText = "Record modified by another user.So refresh it and try to save again." };
        //            }
        //        }
        //        else
        //        {
        //            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //            {

        //                Skill blog = null; // to retrieve old data
        //                DbPropertyValues originalBlogValues = null;
        //                Skill Old_Corp = null;

        //                using (var context = new DataBaseContext())
        //                {
        //                    blog = context.Skill.Where(e => e.Id == data).SingleOrDefault();
        //                    originalBlogValues = context.Entry(blog).OriginalValues;
        //                }
        //                c.DBTrack = new DBTrack
        //                {
        //                    CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                    CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                    Action = "M",
        //                    IsModified = blog.DBTrack.IsModified == true ? true : false,
        //                    ModifiedBy = "0029",
        //                    ModifiedOn = DateTime.Now
        //                };
        //                Skill qualificationDetails = new Skill()
        //                {

        //                    Id = data,

        //                    DBTrack = c.DBTrack,
        //                    RowVersion = (Byte[])TempData["RowVersion"]
        //                };

        //                using (var context = new DataBaseContext())
        //                {
        //                    var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, qualificationDetails, "Skill", c.DBTrack);
        //                    Old_Corp = context.Skill.Where(e => e.Id == data)
        //                      .SingleOrDefault();
        //                    DT_Skill DT_Corp = (DT_Skill)obj;
        //                    db.Create(DT_Corp);
        //                }
        //                blog.DBTrack = c.DBTrack;
        //                db.Skill.Attach(blog);
        //                db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
        //                // db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //                db.SaveChanges();
        //                ts.Complete();
        //                return new { status = true, responseText = "Record Updated" };
        //            }
        //        }
        //        return new Object[] { };
        //    }
        //}


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
        //public ActionResult AddOrEdit(Skill lkval, FormCollection form)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var Add = form["Add"] != null && form["Add"] != "" ? Convert.ToBoolean(form["Add"]) : true;
        //        var Id = form["auth_id"] != null && form["auth_id"] != "" ? Convert.ToInt32(form["auth_id"]) : 0;
        //        if (Add == true)
        //        {
        //            //Add
        //            var returnobj = Create_Skill(lkval, form);
        //            return Json(returnobj, JsonRequestBehavior.AllowGet);
        //        }
        //        else
        //        {
        //            //Edit
        //            var returnobj = EditSave(lkval, Id, form);
        //            return Json(returnobj, JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //}
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