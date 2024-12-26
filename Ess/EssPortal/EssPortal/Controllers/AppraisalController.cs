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

namespace EssPortal.Controllers
{
    public class AppraisalController : Controller
    {
        //
        // GET: /Appraisal/
        public ActionResult Partial()
        {
            return View("~/Views/Appraisal/Index.cshtml");
        }
        public ActionResult Partial1()
        {
            return View("~/Views/Shared/_AppraisalEmpReqPartial.cshtml");
        }
        public ActionResult Partial_View() // Commented on 02/01/2024 : Currently Edit not required 
        {
            return View("~/Views/Shared/_AppraisalReqPartial.cshtml");
        }
        public ActionResult Index() // Commented on 02/01/2024 : Currently Edit not required 
        {
            return View("~/Views/Home/Index.cshtml");
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
        //public class GetLvNewReqClass
        //{
        //    public string Session { get; set; }
        //    public string Program { get; set; }
        //    public string Present { get; set; }
        //    public ChildGetLvNewReqClass RowData { get; set; }
        //}
        public class GetEmptrainingNeeddata
        {
            public int ID { get; set; }
            public int EmpEvaluationId { get; set; }
            public string Editable { get; set; }
            public string Comments { get; set; }
            public string CatName { get; set; }
            public string SubCatName { get; set; }
            public int MaxRatingPoints { get; set; }
            public int RatingPoints { get; set; }

            public ChildGetLvNewReqClass RowData { get; set; }
        }

        public ActionResult GetObjectiveWordings(string data, string data2, string AppAssignId)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    int TempEmpId = 0;
                    if (data2 != "" && data2 != "0")
                    {
                        TempEmpId = Convert.ToInt32(data2);
                    }

                    var selected = (Object)null;
                    if (data2 != "" && data != "0" && data2 != "0")
                    {
                        selected = Convert.ToInt32(data2);
                    }
                    int AssignmentID = 0;
                    if (AppAssignId != "")
                    {
                        AssignmentID = Convert.ToInt32(AppAssignId);
                    }
                    var Id = Convert.ToInt32(SessionManager.EmpLvId);
                    Calendar DefaultCalendaryearid = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "APPRAISALCALENDAR" && e.Default == true).SingleOrDefault();

                    List<GetEmptrainingNeeddata> returndata = new List<GetEmptrainingNeeddata>();
                    List<EmpAppRatingConclusionDetails> EmpAppRatingConclusionDetailsList = new List<EmpAppRatingConclusionDetails>();
                    List<int> empappracategory = new List<int>();
                    List<int> empapprasubcategory = new List<int>();
                    bool appraisalpublishoverornot = false;
                    EmployeeAppraisal db_data = null;
                    Employee emplk = null;

                    if (TempEmpId == 0)
                    {
                        db_data = db.EmployeeAppraisal
                          .Where(e => e.Employee.Id == Id)
                          .Include(e => e.EmpAppEvaluation)
                          .Include(e => e.Employee)
                          .Include(e => e.EmpAppEvaluation.Select(a => a.EmpAppRatingConclusion))
                          .Include(e => e.EmpAppEvaluation.Select(a => a.AppraisalSchedule))
                          .Include(e => e.EmpAppEvaluation.Select(a => a.AppraisalSchedule.AppraisalPublish))
                          .Include(e => e.EmpAppEvaluation.Select(a => a.EmpAppRatingConclusion.Select(t => t.AppraisalAssistance)))
                          .Include(e => e.EmpAppEvaluation.Select(a => a.EmpAppRatingConclusion.Select(t => t.EmpAppRating)))
                          .Include(e => e.EmpAppEvaluation.Select(a => a.EmpAppRatingConclusion.Select(t => t.EmpAppRating.Select(x => x.AppAssignment))))
                          .Include(e => e.EmpAppEvaluation.Select(a => a.EmpAppRatingConclusion.Select(t => t.EmpAppRating.Select(x => x.AppAssignment.AppCategory))))
                          .Include(e => e.EmpAppEvaluation.Select(a => a.EmpAppRatingConclusion.Select(t => t.EmpAppRating.Select(x => x.AppAssignment.AppSubCategory))))
                         .SingleOrDefault();

                        emplk = db.Employee
                                .Include(e => e.GeoStruct)
                                .Include(e => e.FuncStruct)
                                .Include(e => e.PayStruct).Where(e => e.Id == Id).SingleOrDefault();
                    }
                    else
                    {
                        db_data = db.EmployeeAppraisal
                          .Where(e => e.Employee.Id == TempEmpId)
                          .Include(e => e.EmpAppEvaluation)
                          .Include(e => e.Employee)
                          .Include(e => e.EmpAppEvaluation.Select(a => a.EmpAppRatingConclusion))
                          .Include(e => e.EmpAppEvaluation.Select(a => a.AppraisalSchedule))
                          .Include(e => e.EmpAppEvaluation.Select(a => a.AppraisalSchedule.AppraisalPublish))
                          .Include(e => e.EmpAppEvaluation.Select(a => a.EmpAppRatingConclusion.Select(t => t.AppraisalAssistance)))
                          .Include(e => e.EmpAppEvaluation.Select(a => a.EmpAppRatingConclusion.Select(t => t.EmpAppRating)))
                          .Include(e => e.EmpAppEvaluation.Select(a => a.EmpAppRatingConclusion.Select(t => t.EmpAppRating.Select(x => x.AppAssignment))))
                          .Include(e => e.EmpAppEvaluation.Select(a => a.EmpAppRatingConclusion.Select(t => t.EmpAppRating.Select(x => x.AppAssignment.AppCategory))))
                          .Include(e => e.EmpAppEvaluation.Select(a => a.EmpAppRatingConclusion.Select(t => t.EmpAppRating.Select(x => x.AppAssignment.AppSubCategory))))
                         .SingleOrDefault();

                        emplk = db.Employee
                                .Include(e => e.GeoStruct)
                                .Include(e => e.FuncStruct)
                                .Include(e => e.PayStruct).Where(e => e.Id == TempEmpId).SingleOrDefault();
                    }


                    var AppAsslistFuncstruct = db.AppAssignment.Include(e => e.AppRatingObjective)
                                                  .Include(e => e.AppRatingObjective.Select(o => o.ObjectiveWordings))
                                                  .Include(e => e.AppCategory)
                                                  .Include(e => e.AppSubCategory).Include(e => e.FuncStruct).Include(e => e.GeoStruct)
                                                  .Include(e => e.PayStruct)
                                    .Where(e => e.FuncStruct.Id == emplk.FuncStruct.Id || e.GeoStruct.Id == emplk.GeoStruct.Id || e.PayStruct.Id == emplk.PayStruct.Id).ToList();

                    List<AppRatingObjective> TempObjectiveWords = new List<AppRatingObjective>();
                    if (AppAsslistFuncstruct.Count() > 0)
                    {
                        foreach (var AppAsslist in AppAsslistFuncstruct.Where(e => e.Id == AssignmentID))
                        {
                            TempObjectiveWords = AppAsslist.AppRatingObjective.ToList();


                            //var EmpAppEval = db_data.EmpAppEvaluation.Where(e => e.AppraisalPeriodCalendar.Id == AppAsslist.AppraisalCalendar.Id).FirstOrDefault();



                        }

                        if (TempObjectiveWords.Count() > 0)
                        {
                            var returndataa = new SelectList(TempObjectiveWords, "Id", "FullDetails", selected);

                            return Json(returndataa, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
                catch (Exception ex)
                {

                    throw ex;
                }
            }

            return null;

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
                Calendar DefaultCalendaryearid = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "APPRAISALCALENDAR" && e.Default == true).SingleOrDefault();

                List<GetEmptrainingNeeddata> returndata = new List<GetEmptrainingNeeddata>();
                List<EmpAppRatingConclusionDetails> EmpAppRatingConclusionDetailsList = new List<EmpAppRatingConclusionDetails>();
                List<int> empappracategory = new List<int>();
                List<int> empapprasubcategory = new List<int>();
                bool appraisalpublishoverornot = false;
                //var Sal = db.CompanyPayroll.Where(e => e.Company.Id == id).Include(e => e.SalHeadFormula).Select(e => e.SalHeadFormula).SingleOrDefault();
                var db_data = db.EmployeeAppraisal
                       .Where(e => e.Employee.Id == Id)
                       .Include(e => e.EmpAppEvaluation)
                       .Include(e => e.EmpAppEvaluation.Select(a => a.AppraisalPeriodCalendar))
                       .Include(e => e.Employee)
                       .Include(e => e.EmpAppEvaluation.Select(a => a.EmpAppRatingConclusion))
                       .Include(e => e.EmpAppEvaluation.Select(a => a.AppraisalSchedule))
                       .Include(e => e.EmpAppEvaluation.Select(a => a.AppraisalSchedule.AppraisalPublish))
                       .Include(e => e.EmpAppEvaluation.Select(a => a.EmpAppRatingConclusion.Select(t => t.AppraisalAssistance)))
                       .Include(e => e.EmpAppEvaluation.Select(a => a.EmpAppRatingConclusion.Select(t => t.EmpAppRating)))
                       .Include(e => e.EmpAppEvaluation.Select(a => a.EmpAppRatingConclusion.Select(t => t.EmpAppRating.Select(x => x.AppAssignment))))
                       .Include(e => e.EmpAppEvaluation.Select(a => a.EmpAppRatingConclusion.Select(t => t.EmpAppRating.Select(x => x.AppAssignment.AppCategory))))
                       .Include(e => e.EmpAppEvaluation.Select(a => a.EmpAppRatingConclusion.Select(t => t.EmpAppRating.Select(x => x.AppAssignment.AppSubCategory))))
                      .SingleOrDefault();

                var emplk = db.Employee
                        .Include(e => e.GeoStruct)
                        .Include(e => e.FuncStruct)
                        .Include(e => e.PayStruct).Where(e => e.Id == Id).SingleOrDefault();

                DateTime currentdate = DateTime.Now.Date;

                string AccessRight = "";
                if (!string.IsNullOrEmpty(Convert.ToString(Session["user-module"])))
                {
                    AccessRight = Convert.ToString(Session["auho"]);
                }

                string LookupAppDATA = string.Empty;
                if (AccessRight == "MySelf")
                {

                    LookupAppDATA = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1068").FirstOrDefault().LookupValues.Where(l => l.LookupVal.ToUpper() == "APPRAISEE".ToUpper()).FirstOrDefault().LookupVal;
                }




                if (db_data != null)
                {
                    var defaultcalendarwise = db_data.EmpAppEvaluation.Where(e => e.AppraisalPeriodCalendar.Id == DefaultCalendaryearid.Id).ToList();
                    foreach (var item in defaultcalendarwise)
                    {

                        //var empratingdata = item.EmpAppRatingConclusion.Where(e => e.AppraisalAssistance != null && e.AppraisalAssistance.LookupVal.ToString().ToUpper() == "APPRAISEE").ToList();
                        //foreach (var item1 in empratingdata)
                        //{
                        //    foreach (var item2 in item1.EmpAppRating)
                        //    {
                        //        empappracategory.Add(item2.AppAssignment.AppCategory.Id);
                        //        empapprasubcategory.Add(item2.AppAssignment.AppSubCategory.Id);
                        //    }
                        //}

                        var AppAssignmentList = db.AppAssignment.Include(e => e.AppCategory).Include(e => e.AppSubCategory).Include(e => e.AppraisalCalendar)
                                        .Where(e => e.AppraisalCalendar.Id == item.AppraisalPeriodCalendar.Id).ToList();

                        foreach (var getitem in AppAssignmentList)
                        {
                            if (getitem.AppCategory != null)
                            {
                                empappracategory.Add(getitem.AppCategory.Id);

                            }
                            if (getitem.AppSubCategory != null)
                            {
                                empapprasubcategory.Add(getitem.AppSubCategory.Id);
                            }

                        }
                        empappracategory = empappracategory.Distinct().ToList();
                        empapprasubcategory = empapprasubcategory.Distinct().ToList();
                    }

                }
                if (db_data.EmpAppEvaluation.Count() > 0)
                {

                    //var EmpAppLIST = db_data.EmpAppEvaluation.FirstOrDefault();
                    var EmpAppLIST = db_data.EmpAppEvaluation.Where(e => e.AppraisalPeriodCalendar.Id == DefaultCalendaryearid.Id).ToList();
                    List<EmpAppEvaluation> ObjempappLIST = new List<EmpAppEvaluation>();
                    ObjempappLIST.AddRange(EmpAppLIST);


                    List<int> appidslist = new List<int>();
                    AppAssignment chkAppassign = null;
                    foreach (var item in ObjempappLIST)
                    {

                        int spanperiod = item.AppraisalSchedule.AppraisalPublish.FirstOrDefault().SpanPeriod + item.AppraisalSchedule.AppraisalPublish.FirstOrDefault().Extension;
                        DateTime? date1 = item.AppraisalSchedule.AppraisalPublish.FirstOrDefault().PublishDate;
                        DateTime date2 = date1.Value.AddDays(spanperiod);
                        if (currentdate >= date1 && currentdate <= date2)
                        {
                            appraisalpublishoverornot = true;

                            //var AppAsslistgeostuct = db.AppAssignment.Include(e => e.AppCategory).Include(e => e.AppSubCategory).Include(e => e.GeoStruct)
                            //             .Where(e => e.GeoStruct.Id == emplk.GeoStruct.Id && empappracategory.Contains(e.AppCategory.Id) && empapprasubcategory.Contains(e.AppSubCategory.Id)).ToList();
                            var AppAsslistgeostuct = db.AppAssignment.Include(e => e.AppCategory).Include(e => e.AppSubCategory).Include(e => e.GeoStruct)
                                         .Where(e => e.GeoStruct.Id == emplk.GeoStruct.Id && e.PayStruct.Id == emplk.PayStruct.Id && e.FuncStruct.Id == emplk.FuncStruct.Id
                                         && empappracategory.Contains(e.AppCategory.Id) && empapprasubcategory.Contains(e.AppSubCategory.Id)).ToList();


                            #region Code for Removing Duplicate Record By Anandrao
                            if (AppAsslistgeostuct.Count() > 0)
                            {
                                foreach (var itemApp in AppAsslistgeostuct)
                                {
                                    int AppID = itemApp.AppCategory.Id;
                                    appidslist.Add(AppID);
                                }
                                for (int i = 0; i < appidslist.Count(); i++)
                                {
                                    for (int j = i + 1; j < appidslist.Count(); j++)
                                    {
                                        if (appidslist[i] == appidslist[j])
                                        {
                                            int chk = appidslist[j];
                                            chkAppassign = AppAsslistgeostuct.Where(e => e.AppCategory.Id == chk).FirstOrDefault();
                                            AppAsslistgeostuct.Remove(chkAppassign);
                                        }
                                    }
                                }
                            }
                            #endregion

                            if (AppAsslistgeostuct.Count() != 0)
                            {

                                foreach (var AppAsslist in AppAsslistgeostuct)
                                {

                                    var EmpAppEval = db_data.EmpAppEvaluation.Where(e => e.AppraisalPeriodCalendar.Id == AppAsslist.AppraisalCalendar.Id).Select(c => new
                                    {
                                        EmpAppEvalId = c.Id,
                                        EmpAppRatingConclusionVal = c.EmpAppRatingConclusion.Select(r => new
                                        {
                                            EmpAppRatingConclId = r.Id,
                                            AppraisalAssistanceVal = r.AppraisalAssistance.LookupVal,
                                            eraatateVal = r.EmpAppRating.Select(p => new
                                            {
                                                EmpAppAssignmentId = p.AppAssignment.Id,
                                                EmpAppCOmments = p.Comments,
                                                EmpAppRatePoint = p.RatingPoints,
                                            }).Where(z => z.EmpAppAssignmentId == AppAsslist.Id).ToList(),
                                        }).Where(p => p.AppraisalAssistanceVal.ToUpper() == LookupAppDATA.ToUpper()).ToList(),

                                    }).Where(z => z.EmpAppEvalId == item.Id).ToList();


                                    foreach (var SingleEmpAppEval in EmpAppEval)
                                    {
                                        var GetEARvalue = SingleEmpAppEval.EmpAppRatingConclusionVal.Where(e => e.eraatateVal.Count() > 0).Select(s => s.eraatateVal).ToList();


                                        if (SingleEmpAppEval.EmpAppRatingConclusionVal.Count() > 0 && GetEARvalue.Count() > 0)
                                        {
                                            foreach (var SingleEmpCon in SingleEmpAppEval.EmpAppRatingConclusionVal)
                                            {
                                                foreach (var SingleEmpAppRating in SingleEmpCon.eraatateVal)
                                                {
                                                    returndata.Add(new GetEmptrainingNeeddata
                                                    {
                                                        ID = AppAsslist.Id,
                                                        EmpEvaluationId = SingleEmpAppEval.EmpAppEvalId,
                                                        CatName = AppAsslist == null ? "" : AppAsslist.AppCategory == null ? "" : AppAsslist.AppCategory.Name == null ? "" : AppAsslist.AppCategory.Name.ToString(),
                                                        SubCatName = AppAsslist == null ? "" : AppAsslist.AppSubCategory == null ? "" : AppAsslist.AppSubCategory.Name == null ? "" : AppAsslist.AppSubCategory.Name.ToString(),

                                                        MaxRatingPoints = AppAsslist == null ? 0 : AppAsslist.MaxRatingPoints == null ? 0 : AppAsslist.MaxRatingPoints,
                                                        RatingPoints = SingleEmpCon == null && SingleEmpAppRating == null && SingleEmpAppRating.EmpAppRatePoint == 0 ? 0 : SingleEmpAppRating.EmpAppRatePoint,
                                                        Comments = SingleEmpCon == null && SingleEmpAppRating == null && SingleEmpAppRating.EmpAppCOmments == null ? "" : SingleEmpAppRating.EmpAppCOmments,
                                                    });
                                                }
                                            }
                                        }
                                        else
                                        {
                                            returndata.Add(new GetEmptrainingNeeddata
                                            {
                                                ID = AppAsslist.Id,
                                                EmpEvaluationId = SingleEmpAppEval.EmpAppEvalId,
                                                CatName = AppAsslist == null ? "" : AppAsslist.AppCategory == null ? "" : AppAsslist.AppCategory.Name == null ? "" : AppAsslist.AppCategory.Name.ToString(),
                                                SubCatName = AppAsslist == null ? "" : AppAsslist.AppSubCategory == null ? "" : AppAsslist.AppSubCategory.Name == null ? "" : AppAsslist.AppSubCategory.Name.ToString(),

                                                MaxRatingPoints = AppAsslist == null ? 0 : AppAsslist.MaxRatingPoints == null ? 0 : AppAsslist.MaxRatingPoints,
                                                RatingPoints = 0,
                                                Comments = "",
                                            });
                                        }
                                    }

                                    //returndata.Add(new GetEmptrainingNeeddata
                                    //{
                                    //    ID = AppAsslist.Id,
                                    //    EmpEvaluationId = EmpAppEval.Id,
                                    //    CatName = AppAsslist == null ? "" : AppAsslist.AppCategory == null ? "" : AppAsslist.AppCategory.Name == null ? "" : AppAsslist.AppCategory.Name.ToString(),
                                    //    SubCatName = AppAsslist == null ? "" : AppAsslist.AppSubCategory == null ? "" : AppAsslist.AppSubCategory.Name == null ? "" : AppAsslist.AppSubCategory.Name.ToString(),

                                    //    MaxRatingPoints = AppAsslist == null ? 0 : AppAsslist.MaxRatingPoints == null ? 0 : AppAsslist.MaxRatingPoints,
                                    //    RatingPoints = 0,
                                    //    Comments = "",
                                    //});
                                }
                            }
                            //var AppAsslistFuncstruct = db.AppAssignment.Include(e => e.AppRatingObjective)
                            //                           .Include(e => e.AppRatingObjective.Select(o => o.ObjectiveWordings))
                            //                           .Include(e => e.AppCategory)
                            //                           .Include(e => e.AppSubCategory).Include(e => e.FuncStruct)
                            //             .Where(e => e.FuncStruct.Id == emplk.FuncStruct.Id && empappracategory.Contains(e.AppCategory.Id) && empapprasubcategory.Contains(e.AppSubCategory.Id)).Distinct().ToList();


                            var AppAsslistFuncstruct = db.AppAssignment.Include(e => e.AppCategory).Include(e => e.AppSubCategory).Include(e => e.GeoStruct)
                                         .Where(e => e.GeoStruct.Id == emplk.GeoStruct.Id && e.PayStruct.Id == emplk.PayStruct.Id && e.FuncStruct.Id == null
                                         && empappracategory.Contains(e.AppCategory.Id) && empapprasubcategory.Contains(e.AppSubCategory.Id)).ToList();

                            #region Code for Removing Duplicate Record By Anandrao
                            if (AppAsslistFuncstruct.Count() > 0)
                            {
                                foreach (var itemApp in AppAsslistFuncstruct)
                                {
                                    int AppID = itemApp.AppCategory.Id;
                                    appidslist.Add(AppID);
                                }
                                for (int i = 0; i < appidslist.Count(); i++)
                                {
                                    for (int j = i + 1; j < appidslist.Count(); j++)
                                    {
                                        if (appidslist[i] == appidslist[j])
                                        {
                                            int chk = appidslist[j];
                                            chkAppassign = AppAsslistFuncstruct.Where(e => e.AppCategory.Id == chk).FirstOrDefault();
                                            AppAsslistFuncstruct.Remove(chkAppassign);
                                        }
                                    }
                                }

                            }
                            #endregion

                            if (AppAsslistFuncstruct.Count() != 0)
                            {

                                foreach (var AppAsslist in AppAsslistFuncstruct)
                                {

                                    var EmpAppEval = db_data.EmpAppEvaluation.Where(e => e.AppraisalPeriodCalendar.Id == AppAsslist.AppraisalCalendar.Id).Select(c => new
                                    {
                                        EmpAppEvalId = c.Id,
                                        EmpAppRatingConclusionVal = c.EmpAppRatingConclusion.Select(r => new
                                        {
                                            EmpAppRatingConclId = r.Id,
                                            AppraisalAssistanceVal = r.AppraisalAssistance.LookupVal,
                                            eraatateVal = r.EmpAppRating.Select(p => new
                                            {
                                                EmpAppAssignmentId = p.AppAssignment.Id,
                                                EmpAppCOmments = p.Comments,
                                                EmpAppRatePoint = p.RatingPoints,
                                            }).Where(z => z.EmpAppAssignmentId == AppAsslist.Id).ToList(),
                                        }).Where(p => p.AppraisalAssistanceVal.ToUpper() == LookupAppDATA.ToUpper()).ToList(),

                                    }).Where(z => z.EmpAppEvalId == item.Id).ToList();


                                    foreach (var SingleEmpAppEval in EmpAppEval)
                                    {
                                        var GetEARvalue = SingleEmpAppEval.EmpAppRatingConclusionVal.Where(e => e.eraatateVal.Count() > 0).Select(s => s.eraatateVal).ToList();


                                        if (SingleEmpAppEval.EmpAppRatingConclusionVal.Count() > 0 && GetEARvalue.Count() > 0)
                                        {
                                            foreach (var SingleEmpCon in SingleEmpAppEval.EmpAppRatingConclusionVal)
                                            {
                                                foreach (var SingleEmpAppRating in SingleEmpCon.eraatateVal)
                                                {
                                                    returndata.Add(new GetEmptrainingNeeddata
                                                    {
                                                        ID = AppAsslist.Id,
                                                        EmpEvaluationId = SingleEmpAppEval.EmpAppEvalId,
                                                        CatName = AppAsslist == null ? "" : AppAsslist.AppCategory == null ? "" : AppAsslist.AppCategory.Name == null ? "" : AppAsslist.AppCategory.Name.ToString(),
                                                        SubCatName = AppAsslist == null ? "" : AppAsslist.AppSubCategory == null ? "" : AppAsslist.AppSubCategory.Name == null ? "" : AppAsslist.AppSubCategory.Name.ToString(),

                                                        MaxRatingPoints = AppAsslist == null ? 0 : AppAsslist.MaxRatingPoints == null ? 0 : AppAsslist.MaxRatingPoints,
                                                        RatingPoints = SingleEmpCon == null && SingleEmpAppRating == null && SingleEmpAppRating.EmpAppRatePoint == 0 ? 0 : SingleEmpAppRating.EmpAppRatePoint,
                                                        Comments = SingleEmpCon == null && SingleEmpAppRating == null && SingleEmpAppRating.EmpAppCOmments == null ? "" : SingleEmpAppRating.EmpAppCOmments,
                                                    });
                                                }
                                            }
                                        }
                                        else
                                        {

                                            returndata.Add(new GetEmptrainingNeeddata
                                            {
                                                ID = AppAsslist.Id,
                                                EmpEvaluationId = SingleEmpAppEval.EmpAppEvalId,
                                                CatName = AppAsslist == null ? "" : AppAsslist.AppCategory == null ? "" : AppAsslist.AppCategory.Name == null ? "" : AppAsslist.AppCategory.Name.ToString(),
                                                SubCatName = AppAsslist == null ? "" : AppAsslist.AppSubCategory == null ? "" : AppAsslist.AppSubCategory.Name == null ? "" : AppAsslist.AppSubCategory.Name.ToString(),

                                                MaxRatingPoints = AppAsslist == null ? 0 : AppAsslist.MaxRatingPoints == null ? 0 : AppAsslist.MaxRatingPoints,
                                                RatingPoints = 0,
                                                Comments = "",
                                            });

                                        }

                                    }

                                }
                            }

                            //var AppAsslistPaystruct = db.AppAssignment.Include(e => e.AppCategory).Include(e => e.AppSubCategory).Include(e => e.PayStruct)
                            //             .Where(e => e.PayStruct.Id == emplk.PayStruct.Id && empappracategory.Contains(e.AppCategory.Id) && empapprasubcategory.Contains(e.AppSubCategory.Id)).Distinct().ToList();

                            var AppAsslistPaystruct = db.AppAssignment.Include(e => e.AppCategory).Include(e => e.AppSubCategory).Include(e => e.GeoStruct)
                                         .Where(e => e.GeoStruct.Id == emplk.GeoStruct.Id && e.PayStruct.Id == null && e.FuncStruct.Id == emplk.FuncStruct.Id
                                         && empappracategory.Contains(e.AppCategory.Id) && empapprasubcategory.Contains(e.AppSubCategory.Id)).ToList();

                            if (AppAsslistPaystruct.Count() > 0)
                            {
                                foreach (var itemApp in AppAsslistPaystruct)
                                {
                                    int AppID = itemApp.AppCategory.Id;
                                    appidslist.Add(AppID);
                                }
                                for (int i = 0; i < appidslist.Count(); i++)
                                {
                                    for (int j = i + 1; j < appidslist.Count(); j++)
                                    {
                                        if (appidslist[i] == appidslist[j])
                                        {
                                            int chk = appidslist[j];
                                            chkAppassign = AppAsslistPaystruct.Where(e => e.AppCategory.Id == chk).FirstOrDefault();
                                            AppAsslistPaystruct.Remove(chkAppassign);
                                        }
                                    }
                                }


                            }


                            if (AppAsslistPaystruct.Count() != 0)
                            {

                                foreach (var AppAsslist in AppAsslistPaystruct)
                                {
                                    var EmpAppEval = db_data.EmpAppEvaluation.Where(e => e.AppraisalPeriodCalendar.Id == AppAsslist.AppraisalCalendar.Id).Select(c => new
                                    {
                                        EmpAppEvalId = c.Id,
                                        EmpAppRatingConclusionVal = c.EmpAppRatingConclusion.Select(r => new
                                        {
                                            EmpAppRatingConclId = r.Id,
                                            AppraisalAssistanceVal = r.AppraisalAssistance.LookupVal,
                                            eraatateVal = r.EmpAppRating.Select(p => new
                                            {
                                                EmpAppAssignmentId = p.AppAssignment.Id,
                                                EmpAppCOmments = p.Comments,
                                                EmpAppRatePoint = p.RatingPoints,
                                            }).Where(z => z.EmpAppAssignmentId == AppAsslist.Id).ToList(),
                                        }).Where(p => p.AppraisalAssistanceVal.ToUpper() == LookupAppDATA.ToUpper()).ToList(),

                                    }).Where(z => z.EmpAppEvalId == item.Id).ToList();


                                    foreach (var SingleEmpAppEval in EmpAppEval)
                                    {
                                        var GetEARvalue = SingleEmpAppEval.EmpAppRatingConclusionVal.Where(e => e.eraatateVal.Count() > 0).Select(s => s.eraatateVal).ToList();


                                        if (SingleEmpAppEval.EmpAppRatingConclusionVal.Count() > 0 && GetEARvalue.Count() > 0)
                                        {
                                            foreach (var SingleEmpCon in SingleEmpAppEval.EmpAppRatingConclusionVal)
                                            {
                                                foreach (var SingleEmpAppRating in SingleEmpCon.eraatateVal)
                                                {
                                                    returndata.Add(new GetEmptrainingNeeddata
                                                    {
                                                        ID = AppAsslist.Id,
                                                        EmpEvaluationId = SingleEmpAppEval.EmpAppEvalId,
                                                        CatName = AppAsslist == null ? "" : AppAsslist.AppCategory == null ? "" : AppAsslist.AppCategory.Name == null ? "" : AppAsslist.AppCategory.Name.ToString(),
                                                        SubCatName = AppAsslist == null ? "" : AppAsslist.AppSubCategory == null ? "" : AppAsslist.AppSubCategory.Name == null ? "" : AppAsslist.AppSubCategory.Name.ToString(),

                                                        MaxRatingPoints = AppAsslist == null ? 0 : AppAsslist.MaxRatingPoints == null ? 0 : AppAsslist.MaxRatingPoints,
                                                        RatingPoints = SingleEmpCon == null && SingleEmpAppRating == null && SingleEmpAppRating.EmpAppRatePoint == 0 ? 0 : SingleEmpAppRating.EmpAppRatePoint,
                                                        Comments = SingleEmpCon == null && SingleEmpAppRating == null && SingleEmpAppRating.EmpAppCOmments == null ? "" : SingleEmpAppRating.EmpAppCOmments,
                                                    });
                                                }
                                            }
                                        }
                                        else
                                        {
                                            returndata.Add(new GetEmptrainingNeeddata
                                            {
                                                ID = AppAsslist.Id,
                                                EmpEvaluationId = SingleEmpAppEval.EmpAppEvalId,
                                                CatName = AppAsslist == null ? "" : AppAsslist.AppCategory == null ? "" : AppAsslist.AppCategory.Name == null ? "" : AppAsslist.AppCategory.Name.ToString(),
                                                SubCatName = AppAsslist == null ? "" : AppAsslist.AppSubCategory == null ? "" : AppAsslist.AppSubCategory.Name == null ? "" : AppAsslist.AppSubCategory.Name.ToString(),

                                                MaxRatingPoints = AppAsslist == null ? 0 : AppAsslist.MaxRatingPoints == null ? 0 : AppAsslist.MaxRatingPoints,
                                                RatingPoints = 0,
                                                Comments = "",
                                            });
                                        }

                                    }



                                }
                            }

                            var AppAsslistPayFuncstruct = db.AppAssignment.Include(e => e.AppCategory).Include(e => e.AppSubCategory).Include(e => e.GeoStruct)
                                         .Where(e => e.GeoStruct.Id == null && e.PayStruct.Id == emplk.PayStruct.Id && e.FuncStruct.Id == emplk.FuncStruct.Id
                                         && empappracategory.Contains(e.AppCategory.Id) && empapprasubcategory.Contains(e.AppSubCategory.Id)).ToList();

                            if (AppAsslistPayFuncstruct.Count() > 0)
                            {
                                foreach (var itemApp in AppAsslistPaystruct)
                                {
                                    int AppID = itemApp.AppCategory.Id;
                                    appidslist.Add(AppID);
                                }
                                for (int i = 0; i < appidslist.Count(); i++)
                                {
                                    for (int j = i + 1; j < appidslist.Count(); j++)
                                    {
                                        if (appidslist[i] == appidslist[j])
                                        {
                                            int chk = appidslist[j];
                                            chkAppassign = AppAsslistPayFuncstruct.Where(e => e.AppCategory.Id == chk).FirstOrDefault();
                                            AppAsslistPayFuncstruct.Remove(chkAppassign);
                                        }
                                    }
                                }

                            }

                            if (AppAsslistPayFuncstruct.Count() != 0)
                            {

                                foreach (var AppAsslist in AppAsslistPayFuncstruct)
                                {
                                    var EmpAppEval = db_data.EmpAppEvaluation.Where(e => e.AppraisalPeriodCalendar.Id == AppAsslist.AppraisalCalendar.Id).Select(c => new
                                    {
                                        EmpAppEvalId = c.Id,
                                        EmpAppRatingConclusionVal = c.EmpAppRatingConclusion.Select(r => new
                                        {
                                            EmpAppRatingConclId = r.Id,
                                            AppraisalAssistanceVal = r.AppraisalAssistance.LookupVal,
                                            eraatateVal = r.EmpAppRating.Select(p => new
                                            {
                                                EmpAppAssignmentId = p.AppAssignment.Id,
                                                EmpAppCOmments = p.Comments,
                                                EmpAppRatePoint = p.RatingPoints,
                                            }).Where(z => z.EmpAppAssignmentId == AppAsslist.Id).ToList(),
                                        }).Where(p => p.AppraisalAssistanceVal.ToUpper() == LookupAppDATA.ToUpper()).ToList(),

                                    }).Where(z => z.EmpAppEvalId == item.Id).ToList();

                                    foreach (var SingleEmpAppEval in EmpAppEval)
                                    {
                                        var GetEARvalue = SingleEmpAppEval.EmpAppRatingConclusionVal.Where(e => e.eraatateVal.Count() > 0).Select(s => s.eraatateVal).ToList();


                                        if (SingleEmpAppEval.EmpAppRatingConclusionVal.Count() > 0 && GetEARvalue.Count() > 0)
                                        {
                                            foreach (var SingleEmpCon in SingleEmpAppEval.EmpAppRatingConclusionVal)
                                            {
                                                foreach (var SingleEmpAppRating in SingleEmpCon.eraatateVal)
                                                {
                                                    returndata.Add(new GetEmptrainingNeeddata
                                                    {
                                                        ID = AppAsslist.Id,
                                                        EmpEvaluationId = SingleEmpAppEval.EmpAppEvalId,
                                                        CatName = AppAsslist == null ? "" : AppAsslist.AppCategory == null ? "" : AppAsslist.AppCategory.Name == null ? "" : AppAsslist.AppCategory.Name.ToString(),
                                                        SubCatName = AppAsslist == null ? "" : AppAsslist.AppSubCategory == null ? "" : AppAsslist.AppSubCategory.Name == null ? "" : AppAsslist.AppSubCategory.Name.ToString(),

                                                        MaxRatingPoints = AppAsslist == null ? 0 : AppAsslist.MaxRatingPoints == null ? 0 : AppAsslist.MaxRatingPoints,
                                                        RatingPoints = SingleEmpCon == null && SingleEmpAppRating == null && SingleEmpAppRating.EmpAppRatePoint == 0 ? 0 : SingleEmpAppRating.EmpAppRatePoint,
                                                        Comments = SingleEmpCon == null && SingleEmpAppRating == null && SingleEmpAppRating.EmpAppCOmments == null ? "" : SingleEmpAppRating.EmpAppCOmments,
                                                    });
                                                }
                                            }
                                        }
                                        else
                                        {
                                            returndata.Add(new GetEmptrainingNeeddata
                                            {
                                                ID = AppAsslist.Id,
                                                EmpEvaluationId = SingleEmpAppEval.EmpAppEvalId,
                                                CatName = AppAsslist == null ? "" : AppAsslist.AppCategory == null ? "" : AppAsslist.AppCategory.Name == null ? "" : AppAsslist.AppCategory.Name.ToString(),
                                                SubCatName = AppAsslist == null ? "" : AppAsslist.AppSubCategory == null ? "" : AppAsslist.AppSubCategory.Name == null ? "" : AppAsslist.AppSubCategory.Name.ToString(),

                                                MaxRatingPoints = AppAsslist == null ? 0 : AppAsslist.MaxRatingPoints == null ? 0 : AppAsslist.MaxRatingPoints,
                                                RatingPoints = 0,
                                                Comments = "",
                                            });
                                        }

                                    }


                                }
                            }


                            var AppAsslistGEOstruct = db.AppAssignment.Include(e => e.AppCategory).Include(e => e.AppSubCategory).Include(e => e.GeoStruct)
                                                                     .Where(e => e.GeoStruct.Id == emplk.GeoStruct.Id && e.PayStruct.Id == null && e.FuncStruct.Id == null
                                                                     && empappracategory.Contains(e.AppCategory.Id) && empapprasubcategory.Contains(e.AppSubCategory.Id)).ToList();

                            if (AppAsslistGEOstruct.Count() > 0)
                            {
                                foreach (var itemApp in AppAsslistGEOstruct)
                                {
                                    int AppID = itemApp.AppCategory.Id;
                                    appidslist.Add(AppID);
                                }
                                for (int i = 0; i < appidslist.Count(); i++)
                                {
                                    for (int j = i + 1; j < appidslist.Count(); j++)
                                    {
                                        if (appidslist[i] == appidslist[j])
                                        {
                                            int chk = appidslist[j];
                                            chkAppassign = AppAsslistGEOstruct.Where(e => e.AppCategory.Id == chk).FirstOrDefault();
                                            AppAsslistGEOstruct.Remove(chkAppassign);
                                        }
                                    }
                                }


                            }

                            if (AppAsslistGEOstruct.Count() != 0)
                            {

                                foreach (var AppAsslist in AppAsslistGEOstruct)
                                {


                                    var EmpAppEval = db_data.EmpAppEvaluation.Where(e => e.AppraisalPeriodCalendar.Id == AppAsslist.AppraisalCalendar.Id).Select(c => new
                                    {
                                        EmpAppEvalId = c.Id,
                                        EmpAppRatingConclusionVal = c.EmpAppRatingConclusion.Select(r => new
                                        {
                                            EmpAppRatingConclId = r.Id,
                                            AppraisalAssistanceVal = r.AppraisalAssistance.LookupVal,
                                            eraatateVal = r.EmpAppRating.Select(p => new
                                            {
                                                EmpAppAssignmentId = p.AppAssignment.Id,
                                                EmpAppCOmments = p.Comments,
                                                EmpAppRatePoint = p.RatingPoints,
                                            }).Where(z => z.EmpAppAssignmentId == AppAsslist.Id).ToList(),
                                        }).Where(p => p.AppraisalAssistanceVal.ToUpper() == LookupAppDATA.ToUpper()).ToList(),

                                    }).Where(z => z.EmpAppEvalId == item.Id).ToList();


                                    foreach (var SingleEmpAppEval in EmpAppEval)
                                    {
                                        var GetEARvalue = SingleEmpAppEval.EmpAppRatingConclusionVal.Where(e => e.eraatateVal.Count() > 0).Select(s => s.eraatateVal).ToList();


                                        if (SingleEmpAppEval.EmpAppRatingConclusionVal.Count() > 0 && GetEARvalue.Count() > 0)
                                        {
                                            foreach (var SingleEmpCon in SingleEmpAppEval.EmpAppRatingConclusionVal)
                                            {
                                                foreach (var SingleEmpAppRating in SingleEmpCon.eraatateVal)
                                                {
                                                    returndata.Add(new GetEmptrainingNeeddata
                                                    {
                                                        ID = AppAsslist.Id,
                                                        EmpEvaluationId = SingleEmpAppEval.EmpAppEvalId,
                                                        CatName = AppAsslist == null ? "" : AppAsslist.AppCategory == null ? "" : AppAsslist.AppCategory.Name == null ? "" : AppAsslist.AppCategory.Name.ToString(),
                                                        SubCatName = AppAsslist == null ? "" : AppAsslist.AppSubCategory == null ? "" : AppAsslist.AppSubCategory.Name == null ? "" : AppAsslist.AppSubCategory.Name.ToString(),

                                                        MaxRatingPoints = AppAsslist == null ? 0 : AppAsslist.MaxRatingPoints == null ? 0 : AppAsslist.MaxRatingPoints,
                                                        RatingPoints = SingleEmpCon == null && SingleEmpAppRating == null && SingleEmpAppRating.EmpAppRatePoint == 0 ? 0 : SingleEmpAppRating.EmpAppRatePoint,
                                                        Comments = SingleEmpCon == null && SingleEmpAppRating == null && SingleEmpAppRating.EmpAppCOmments == null ? "" : SingleEmpAppRating.EmpAppCOmments,
                                                    });
                                                }
                                            }
                                        }
                                        else
                                        {
                                            returndata.Add(new GetEmptrainingNeeddata
                                            {
                                                ID = AppAsslist.Id,
                                                EmpEvaluationId = SingleEmpAppEval.EmpAppEvalId,
                                                CatName = AppAsslist == null ? "" : AppAsslist.AppCategory == null ? "" : AppAsslist.AppCategory.Name == null ? "" : AppAsslist.AppCategory.Name.ToString(),
                                                SubCatName = AppAsslist == null ? "" : AppAsslist.AppSubCategory == null ? "" : AppAsslist.AppSubCategory.Name == null ? "" : AppAsslist.AppSubCategory.Name.ToString(),

                                                MaxRatingPoints = AppAsslist == null ? 0 : AppAsslist.MaxRatingPoints == null ? 0 : AppAsslist.MaxRatingPoints,
                                                RatingPoints = 0,
                                                Comments = "",
                                            });

                                        }

                                    }



                                }
                            }


                            var AppAsslistFUNstruct = db.AppAssignment.Include(e => e.AppCategory).Include(e => e.AppSubCategory).Include(e => e.GeoStruct)
                                                                     .Where(e => e.GeoStruct.Id == null && e.PayStruct.Id == null && e.FuncStruct.Id == emplk.FuncStruct.Id
                                                                     && empappracategory.Contains(e.AppCategory.Id) && empapprasubcategory.Contains(e.AppSubCategory.Id)).ToList();

                            if (AppAsslistFUNstruct.Count() > 0)
                            {
                                foreach (var itemApp in AppAsslistFUNstruct)
                                {
                                    int AppID = itemApp.AppCategory.Id;
                                    appidslist.Add(AppID);
                                }
                                for (int i = 0; i < appidslist.Count(); i++)
                                {
                                    for (int j = i + 1; j < appidslist.Count(); j++)
                                    {
                                        if (appidslist[i] == appidslist[j])
                                        {
                                            int chk = appidslist[j];
                                            chkAppassign = AppAsslistFUNstruct.Where(e => e.AppCategory.Id == chk).FirstOrDefault();
                                            AppAsslistFUNstruct.Remove(chkAppassign);
                                        }
                                    }
                                }


                            }

                            if (AppAsslistFUNstruct.Count() != 0)
                            {

                                foreach (var AppAsslist in AppAsslistFUNstruct)
                                {

                                    var EmpAppEval = db_data.EmpAppEvaluation.Where(e => e.AppraisalPeriodCalendar.Id == AppAsslist.AppraisalCalendar.Id).Select(c => new
                                    {
                                        EmpAppEvalId = c.Id,
                                        EmpAppRatingConclusionVal = c.EmpAppRatingConclusion.Select(r => new
                                        {
                                            EmpAppRatingConclId = r.Id,
                                            AppraisalAssistanceVal = r.AppraisalAssistance.LookupVal,
                                            eraatateVal = r.EmpAppRating.Select(p => new
                                            {
                                                EmpAppAssignmentId = p.AppAssignment.Id,
                                                EmpAppCOmments = p.Comments,
                                                EmpAppRatePoint = p.RatingPoints,
                                            }).Where(z => z.EmpAppAssignmentId == AppAsslist.Id).ToList(),
                                        }).Where(p => p.AppraisalAssistanceVal.ToUpper() == LookupAppDATA.ToUpper()).ToList(),

                                    }).Where(z => z.EmpAppEvalId == item.Id).ToList();


                                    foreach (var SingleEmpAppEval in EmpAppEval)
                                    {
                                        var GetEARvalue = SingleEmpAppEval.EmpAppRatingConclusionVal.Where(e => e.eraatateVal.Count() > 0).Select(s => s.eraatateVal).ToList();


                                        if (SingleEmpAppEval.EmpAppRatingConclusionVal.Count() > 0 && GetEARvalue.Count() > 0)
                                        {
                                            foreach (var SingleEmpCon in SingleEmpAppEval.EmpAppRatingConclusionVal)
                                            {
                                                foreach (var SingleEmpAppRating in SingleEmpCon.eraatateVal)
                                                {
                                                    returndata.Add(new GetEmptrainingNeeddata
                                                    {
                                                        ID = AppAsslist.Id,
                                                        EmpEvaluationId = SingleEmpAppEval.EmpAppEvalId,
                                                        CatName = AppAsslist == null ? "" : AppAsslist.AppCategory == null ? "" : AppAsslist.AppCategory.Name == null ? "" : AppAsslist.AppCategory.Name.ToString(),
                                                        SubCatName = AppAsslist == null ? "" : AppAsslist.AppSubCategory == null ? "" : AppAsslist.AppSubCategory.Name == null ? "" : AppAsslist.AppSubCategory.Name.ToString(),

                                                        MaxRatingPoints = AppAsslist == null ? 0 : AppAsslist.MaxRatingPoints == null ? 0 : AppAsslist.MaxRatingPoints,
                                                        RatingPoints = SingleEmpCon == null && SingleEmpAppRating == null && SingleEmpAppRating.EmpAppRatePoint == 0 ? 0 : SingleEmpAppRating.EmpAppRatePoint,
                                                        Comments = SingleEmpCon == null && SingleEmpAppRating == null && SingleEmpAppRating.EmpAppCOmments == null ? "" : SingleEmpAppRating.EmpAppCOmments,
                                                    });
                                                }
                                            }

                                        }
                                        else
                                        {
                                            returndata.Add(new GetEmptrainingNeeddata
                                            {
                                                ID = AppAsslist.Id,
                                                EmpEvaluationId = SingleEmpAppEval.EmpAppEvalId,
                                                CatName = AppAsslist == null ? "" : AppAsslist.AppCategory == null ? "" : AppAsslist.AppCategory.Name == null ? "" : AppAsslist.AppCategory.Name.ToString(),
                                                SubCatName = AppAsslist == null ? "" : AppAsslist.AppSubCategory == null ? "" : AppAsslist.AppSubCategory.Name == null ? "" : AppAsslist.AppSubCategory.Name.ToString(),

                                                MaxRatingPoints = AppAsslist == null ? 0 : AppAsslist.MaxRatingPoints == null ? 0 : AppAsslist.MaxRatingPoints,
                                                RatingPoints = 0,
                                                Comments = "",
                                            });

                                        }

                                    }

                                }
                            }



                            var AppAsslistPAYYstruct = db.AppAssignment.Include(e => e.AppCategory).Include(e => e.AppSubCategory).Include(e => e.GeoStruct)
                                                                     .Where(e => e.GeoStruct.Id == null && e.PayStruct.Id == emplk.PayStruct.Id && e.FuncStruct.Id == null
                                                                     && empappracategory.Contains(e.AppCategory.Id) && empapprasubcategory.Contains(e.AppSubCategory.Id)).ToList();

                            if (AppAsslistPAYYstruct.Count() > 0)
                            {
                                foreach (var itemApp in AppAsslistPAYYstruct)
                                {
                                    int AppID = itemApp.AppCategory.Id;
                                    appidslist.Add(AppID);
                                }
                                for (int i = 0; i < appidslist.Count(); i++)
                                {
                                    for (int j = i + 1; j < appidslist.Count(); j++)
                                    {
                                        if (appidslist[i] == appidslist[j])
                                        {
                                            int chk = appidslist[j];
                                            chkAppassign = AppAsslistPAYYstruct.Where(e => e.AppCategory.Id == chk).FirstOrDefault();
                                            AppAsslistPAYYstruct.Remove(chkAppassign);
                                        }
                                    }
                                }


                            }

                            if (AppAsslistPAYYstruct.Count() != 0)
                            {

                                foreach (var AppAsslist in AppAsslistPAYYstruct)
                                {
                                    var EmpAppEval = db_data.EmpAppEvaluation.Where(e => e.AppraisalPeriodCalendar.Id == AppAsslist.AppraisalCalendar.Id).Select(c => new
                                    {
                                        EmpAppEvalId = c.Id,
                                        EmpAppRatingConclusionVal = c.EmpAppRatingConclusion.Select(r => new
                                        {
                                            EmpAppRatingConclId = r.Id,
                                            AppraisalAssistanceVal = r.AppraisalAssistance.LookupVal,
                                            eraatateVal = r.EmpAppRating.Select(p => new
                                            {
                                                EmpAppAssignmentId = p.AppAssignment.Id,
                                                EmpAppCOmments = p.Comments,
                                                EmpAppRatePoint = p.RatingPoints,
                                            }).Where(z => z.EmpAppAssignmentId == AppAsslist.Id).ToList(),
                                        }).Where(p => p.AppraisalAssistanceVal.ToUpper() == LookupAppDATA.ToUpper()).ToList(),

                                    }).Where(z => z.EmpAppEvalId == item.Id).ToList();


                                    foreach (var SingleEmpAppEval in EmpAppEval)
                                    {
                                        var GetEARvalue = SingleEmpAppEval.EmpAppRatingConclusionVal.Where(e => e.eraatateVal.Count() > 0).Select(s => s.eraatateVal).ToList();


                                        if (SingleEmpAppEval.EmpAppRatingConclusionVal.Count() > 0 && GetEARvalue.Count() > 0)
                                        {
                                            foreach (var SingleEmpCon in SingleEmpAppEval.EmpAppRatingConclusionVal)
                                            {
                                                foreach (var SingleEmpAppRating in SingleEmpCon.eraatateVal)
                                                {
                                                    returndata.Add(new GetEmptrainingNeeddata
                                                    {
                                                        ID = AppAsslist.Id,
                                                        EmpEvaluationId = SingleEmpAppEval.EmpAppEvalId,
                                                        CatName = AppAsslist == null ? "" : AppAsslist.AppCategory == null ? "" : AppAsslist.AppCategory.Name == null ? "" : AppAsslist.AppCategory.Name.ToString(),
                                                        SubCatName = AppAsslist == null ? "" : AppAsslist.AppSubCategory == null ? "" : AppAsslist.AppSubCategory.Name == null ? "" : AppAsslist.AppSubCategory.Name.ToString(),

                                                        MaxRatingPoints = AppAsslist == null ? 0 : AppAsslist.MaxRatingPoints == null ? 0 : AppAsslist.MaxRatingPoints,
                                                        RatingPoints = SingleEmpCon == null && SingleEmpAppRating == null && SingleEmpAppRating.EmpAppRatePoint == 0 ? 0 : SingleEmpAppRating.EmpAppRatePoint,
                                                        Comments = SingleEmpCon == null && SingleEmpAppRating == null && SingleEmpAppRating.EmpAppCOmments == null ? "" : SingleEmpAppRating.EmpAppCOmments,
                                                    });
                                                }
                                            }
                                        }
                                        else
                                        {
                                            returndata.Add(new GetEmptrainingNeeddata
                                            {
                                                ID = AppAsslist.Id,
                                                EmpEvaluationId = SingleEmpAppEval.EmpAppEvalId,
                                                CatName = AppAsslist == null ? "" : AppAsslist.AppCategory == null ? "" : AppAsslist.AppCategory.Name == null ? "" : AppAsslist.AppCategory.Name.ToString(),
                                                SubCatName = AppAsslist == null ? "" : AppAsslist.AppSubCategory == null ? "" : AppAsslist.AppSubCategory.Name == null ? "" : AppAsslist.AppSubCategory.Name.ToString(),

                                                MaxRatingPoints = AppAsslist == null ? 0 : AppAsslist.MaxRatingPoints == null ? 0 : AppAsslist.MaxRatingPoints,
                                                RatingPoints = 0,
                                                Comments = "",
                                            });
                                        }

                                    }



                                }
                            }





                        }
                    }

                }
                else
                {
                    return Json(new { status = 1, data = returndata }, JsonRequestBehavior.AllowGet);
                }
                if (appraisalpublishoverornot == false)
                {

                    return Json(new { status = 0, data = returndata }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = true, data = returndata }, JsonRequestBehavior.AllowGet);
                }

            }
        }

        public ActionResult CreateTrainingNeed(EmpTrainingNeed LvReq, FormCollection form, String forwarddata, string DebitDays)
        {

            string Emp = form["EmpAppraisal_id"] == "0" ? "" : form["EmpAppraisal_id"];
            string ProgramListlist = form["ProgramListlist"] == "0" ? "" : form["ProgramListlist"];
            string Comments = form["Comments_drop"] == "0" ? "" : form["Comments_drop"];
            string EmployeeTrainingSource = form["EmployeeTrainingSource_drop"] == "0" ? "" : form["EmployeeTrainingSource_drop"];
            string Tostat = form["ToStatlist"] == "0" ? "" : form["ToStatlist"];
            DebitDays = form["DebitDays"] == "0" ? "" : form["DebitDays"];
            string ddlIncharge = form["ddlIncharge"] == "" ? null : form["ddlIncharge"];


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
                if (ProgramListlist != null)
                {
                    var val = db.ProgramList.Find(int.Parse(ProgramListlist));
                    LvReq.ProgramList = val;
                }
                if (Comments != null)
                {
                    comment = Comments;
                }
                if (EmployeeTrainingSource != null && EmployeeTrainingSource != "")
                {
                    var val = db.LookupValue.Find(int.Parse(EmployeeTrainingSource));
                    LvReq.EmployeeTrainingSource = val;
                }

                LvReq.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                Employee OEmployee = null;
                EmployeeTraining Oemployeetraining = null;
                LvReq.TrainingCalendar = db.Calendar.Where(e => e.Name.LookupVal.ToUpper() == "TRAININGCALENDAR" && e.Default == true).SingleOrDefault();
                Oemployeetraining = db.EmployeeTraining.Include(e => e.EmpTrainingNeed).Include(e => e.Employee)
                .Where(e => e.Employee.Id == EmpId).SingleOrDefault();
                LvWFDetails oLvWFDetails = new LvWFDetails
                {
                    WFStatus = 0,
                    Comments = comment,
                    DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                };
                List<LvWFDetails> oLvWFDetails_List = new List<LvWFDetails>();
                oLvWFDetails_List.Add(oLvWFDetails);

                EmpTrainingNeed TrainingNeed = new EmpTrainingNeed()
                {
                    RequisitionDate = LvReq.RequisitionDate,
                    LvWFDetails = oLvWFDetails_List,
                    TrClosed = false,
                    DBTrack = LvReq.DBTrack,
                    EmployeeTrainingSource = LvReq.EmployeeTrainingSource,
                    ProgramList = LvReq.ProgramList,
                    TrainingCalendar = LvReq.TrainingCalendar,
                    WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "1").FirstOrDefault()
                };
                if (ModelState.IsValid)
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                               new System.TimeSpan(0, 30, 0)))
                    {
                        try
                        {

                            db.EmpTrainingNeed.Add(TrainingNeed);
                            db.SaveChanges();
                            List<EmpTrainingNeed> OFAT = new List<EmpTrainingNeed>();
                            OFAT.Add(db.EmpTrainingNeed.Find(TrainingNeed.Id));

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
                    //FuncModule = db.LookupValue.Where(e => e.LookupVal.ToUpper() == id.ToUpper()).SingleOrDefault();
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
                            //foreach (var item3 in item2.EmpAppRatingConclusion)
                            //{
                            // var empappratingp = item3.EmpAppRating.ToList();
                            if (item2 != null)
                            {
                                data.Add(item1.Employee);
                            }
                            //}
                        }
                    }

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
                    qurey.WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "1").FirstOrDefault();
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
                        qurey.WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "1").FirstOrDefault(); 
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

                        qurey.WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "2").FirstOrDefault(); 
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
                        qurey.WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "3").FirstOrDefault(); 
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

                DateTime currentdate = DateTime.Now;

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
                        int spanperiod = item.AppraisalSchedule.AppraisalPublish.FirstOrDefault().SpanPeriod + item.AppraisalSchedule.AppraisalPublish.FirstOrDefault().Extension;
                        DateTime? date1 = item.AppraisalSchedule.AppraisalPublish.FirstOrDefault().PublishDate;
                        DateTime date2 = date1.Value.AddDays(spanperiod);
                        if (currentdate >= date1 && currentdate <= date2)
                        {

                        }
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
            public string Id { get; set; }
            public string Category { get; set; }
            public string Subcategory { get; set; }
            public string MaxPoint { get; set; }
            public string RatingPoint { get; set; }
            public string Comment { get; set; }
            public string EmpEvaluationId { get; set; }


            public ChildGetLvNewReqClass RowData { get; set; }
        }
        public ActionResult GetMyAppraisal()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (string.IsNullOrEmpty(Convert.ToString(SessionManager.EmpLvId)))
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                var Id = Convert.ToInt32(SessionManager.EmpLvId);
                var db_data = db.EmployeeAppraisal
                      .Where(e => e.Id == Id)
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
                returndata.Add(new GetLvNewReqClass
                {
                    Id = "Id",
                    Category = "Category Name",
                    Subcategory = "Subcategory Name",
                    MaxPoint = "Maxpoint",
                    RatingPoint = "RatingPoint",
                    Comment = "Comment "
                });
                //List<int> lvcode = listLvs.Where(e => e.LvCreditDate != null).Select(e => e.LeaveHead.Id).Distinct().ToList();
                //foreach (var item1 in lvcode)
                //{
                // DateTime? lvcrdate = listLvs.Where(a => a.LeaveHead.Id == item1 && a.LvCreditDate != null).Max(a => a.LvCreditDate.Value);
                //if (lvcrdate != null)
                //{
                //string getDB
                foreach (var item in db_data.EmpAppEvaluation)
                {
                    var empratingdata = item.EmpAppRatingConclusion.Where(e => e.AppraisalAssistance != null && e.AppraisalAssistance.LookupVal.ToString().ToUpper() == "APPRAISEE").ToList();
                    foreach (var item1 in empratingdata)
                    {
                        foreach (var item2 in item1.EmpAppRating)
                        {
                            returndata.Add(new GetLvNewReqClass
                            {
                                RowData = new ChildGetLvNewReqClass
                                {
                                    LvNewReq = item2.Id.ToString(),
                                    EmpLVid = db_data.Employee.Id.ToString(),
                                    // IsClose = Status,
                                    //Status = Status,
                                    // LvHead_Id = item.LeaveHead.Id.ToString(),
                                },
                                Id = item2.Id.ToString(),
                                Category = item2.AppAssignment == null ? "" : item2.AppAssignment.AppCategory == null ? "" : item2.AppAssignment.AppCategory.Name,
                                Subcategory = item2.AppAssignment == null ? "" : item2.AppAssignment.AppSubCategory == null ? "" : item2.AppAssignment.AppSubCategory.Name,
                                MaxPoint = item2.AppAssignment == null ? "" : item2.AppAssignment.MaxRatingPoints.ToString(),
                                RatingPoint = item2.RatingPoints.ToString(),
                                Comment = item2.Comments,

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


        public class ChildGetEmpAppraisalReqClass
        {
            public string LvNewReq { get; set; }
            public string EmpLVid { get; set; }
            public string IsClose { get; set; }
            public string LvHead_Id { get; set; }
            public string OdReqId { get; set; }

        }
        public class GetEmpAppraisalClass1
        {
            public string EmpCode { get; set; }
            public string EmpName { get; set; }
            //public string Id { get; set; }
            //public string Category { get; set; }
            //public string Subcategory { get; set; }
            //public string MaxPoint { get; set; }
            //public string RatingPoint { get; set; }
            //public string Comment { get; set; }
            //public string Status { get; set; }

            public ChildGetEmpAppraisalReqClass RowData { get; set; }
        }

        #region Get Employee Appraisal request On Sanction Dropdown

        public ActionResult GetAppraisalReqOnSanction(FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                LookupValue FuncModule = new LookupValue();
                string AccessRight = "";
                if (!string.IsNullOrEmpty(Convert.ToString(Session["user-module"])))
                {
                    var id = Convert.ToString(Session["user-module"]);
                    FuncModule = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "601").FirstOrDefault()
                        .LookupValues.Where(e => e.LookupVal.ToUpper() == id.ToUpper()).FirstOrDefault();
                }
                if (FuncModule == null)
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                if (!string.IsNullOrEmpty(Convert.ToString(Session["user-module"])))
                {
                    AccessRight = Convert.ToString(Session["auho"]);
                }
                // var EmpIds = UserManager.GeEmployeeList(FuncModule.Id, 0);
                List<EssPortal.Process.PortalRights.AccessRightsData> EmpidsWithfunsub = UserManager.GeEmployeeListWithfunsub(FuncModule.Id, 0, AccessRight);
                if (EmpidsWithfunsub == null && EmpidsWithfunsub.Count == 0)
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }

                LookupValue val = null;
                if (AccessRight == "Sanction")
                {
                    val = db.LookupValue.Where(e => e.LookupVal.ToString().ToUpper() == "AppraiserSanction".ToUpper()).SingleOrDefault();
                }
                else
                {
                    val = db.LookupValue.Where(e => e.LookupVal.ToString().ToUpper() == "HR".ToUpper()).SingleOrDefault();
                }



                var returnDataClass = new List<returnDataClass>();

                List<GetEmpAppraisalClass1> returndata = new List<GetEmpAppraisalClass1>();
                returndata.Add(new GetEmpAppraisalClass1
                {
                    EmpCode = "EmpCode",
                    EmpName = "EmpName",
                    //Id = "Id",
                    //Category = "Category Name",
                    //Subcategory = "Subcategory Name",
                    //MaxPoint = "Maxpoint",
                    //RatingPoint = "RatingPoint",
                    //Comment = "Comment ",
                    // Status = "Status",

                    RowData = new ChildGetEmpAppraisalReqClass
                    {
                        LvNewReq = "0",
                        EmpLVid = "0",
                        IsClose = "0",
                        LvHead_Id = "",
                    }
                });
                foreach (var item1 in EmpidsWithfunsub)
                {

                    var Emps = db.EmployeeAppraisal
                         .Where(e => (item1.ReportingEmployee.Contains(e.Employee.Id)))
                        //.Where(e => e.Id == Id)
                          .Include(e => e.Employee.ReportingStructRights)
                          .Include(e => e.Employee.ReportingStructRights.Select(b => b.AccessRights.ActionName))
                          .Include(e => e.Employee.ReportingStructRights.Select(b => b.FuncModules))
                          .Include(e => e.EmpAppEvaluation)
                          .Include(e => e.Employee.EmpName)
                          .Include(e => e.Employee)
                          .Include(e => e.EmpAppEvaluation.Select(a => a.EmpAppRatingConclusion))
                          .Include(e => e.EmpAppEvaluation.Select(a => a.EmpAppRatingConclusion.Select(t => t.AppraisalAssistance)))
                          .Include(e => e.EmpAppEvaluation.Select(a => a.EmpAppRatingConclusion.Select(t => t.EmpAppRating)))
                          .Include(e => e.EmpAppEvaluation.Select(a => a.EmpAppRatingConclusion.Select(t => t.EmpAppRating.Select(x => x.AppAssignment))))
                          .Include(e => e.EmpAppEvaluation.Select(a => a.EmpAppRatingConclusion.Select(t => t.EmpAppRating.Select(x => x.AppAssignment.AppCategory))))
                          .Include(e => e.EmpAppEvaluation.Select(a => a.EmpAppRatingConclusion.Select(t => t.EmpAppRating.Select(x => x.AppAssignment.AppSubCategory))))
                     .ToList();


                    foreach (var item in Emps)
                    {
                        if (item.EmpAppEvaluation != null && item.EmpAppEvaluation.Count() > 0)
                        {
                            var LvIds = UserManager.FilterAppraisaL(item.EmpAppEvaluation.OrderByDescending(e => e.DBTrack.CreatedOn.Value.ToShortDateString()).ToList(),
                               Convert.ToString(Session["auho"]), Convert.ToInt32(SessionManager.CompanyId));

                            if (LvIds.Count() > 0)
                            {
                                var Appraisalreqdata = item.EmpAppEvaluation.Where(e => LvIds.Contains(e.Id)).ToList();
                                foreach (var singleAppraiDetails in Appraisalreqdata.Select(r => r.EmpAppRatingConclusion).ToList())
                                {
                                    foreach (var ItemApp in singleAppraiDetails.Where(e => e.AppraisalAssistance.Id == val.Id).Select(a => a.EmpAppRating).ToList())
                                    {
                                        foreach (var itemAPPrating in ItemApp)
                                        {
                                            if (itemAPPrating != null)
                                            {

                                                var session = Session["auho"].ToString().ToUpper();
                                                var EmpR = item.Employee.ReportingStructRights.Where(e => e.AccessRights != null && e.AccessRights.ActionName.LookupVal.ToUpper() == session && e.FuncModules.LookupVal.ToUpper() == item1.ModuleName.ToUpper())
                                                .Select(e => e.AccessRights.IsClose).FirstOrDefault();


                                                returndata.Add(new GetEmpAppraisalClass1
                                                {
                                                    RowData = new ChildGetEmpAppraisalReqClass
                                                    {
                                                        LvNewReq = itemAPPrating.Id.ToString(),
                                                        EmpLVid = item.Id.ToString(),
                                                        IsClose = EmpR.ToString(),
                                                        LvHead_Id = "",
                                                    },

                                                    EmpCode = item.Employee != null ? item.Employee.EmpCode.ToString() : "",
                                                    EmpName = item.Employee != null && item.Employee.EmpName != null ? item.Employee.EmpName.FullNameFML.ToString() : "",
                                                    //Id = itemAPPrating.Id.ToString(),
                                                    //Category = itemAPPrating.AppAssignment == null ? "" : itemAPPrating.AppAssignment.AppCategory == null ? "" : itemAPPrating.AppAssignment.AppCategory.Name,
                                                    //Subcategory = itemAPPrating.AppAssignment == null ? "" : itemAPPrating.AppAssignment.AppSubCategory == null ? "" : itemAPPrating.AppAssignment.AppSubCategory.Name,
                                                    //MaxPoint = itemAPPrating.AppAssignment == null ? "" : itemAPPrating.AppAssignment.MaxRatingPoints.ToString(),
                                                    //RatingPoint = itemAPPrating.RatingPoints.ToString(),
                                                    //Comment = itemAPPrating.Comments



                                                });


                                            }
                                        }

                                    }

                                }
                            }
                        }
                    }
                }

                var result = returndata.GroupBy(e => e.EmpCode).Select(r => r.First()).ToList();

                if (result != null && result.Count > 0)
                {

                    return Json(new { status = true, data = result, responseText = "Okk" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = false, data = result, responseText = "Error" }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        #endregion




        //#region Get Employee Appraisal request On Sanction Bind DATA

        //public ActionResult GetEmpAppraisalData(string data)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        string authority = Convert.ToString(Session["auho"]);
        //        var WfStatus = new List<Int32>();
        //        var SanctionStatus = new List<Int32>();
        //        var ApporvalStatus = new List<Int32>();
        //        var RecomendationStatus = new List<Int32>();
        //        if (authority.ToUpper() == "SANCTION")
        //        {
        //            WfStatus.Add(1);
        //            WfStatus.Add(2);
        //        }
        //        else if (authority.ToUpper() == "APPROVAL")
        //        {
        //            WfStatus.Add(1);
        //            WfStatus.Add(2);
        //            RecomendationStatus.Add(7);
        //            RecomendationStatus.Add(8);
        //        }
        //        else if (authority.ToUpper() == "RECOMENDATION")
        //        {
        //            WfStatus.Add(1);
        //            WfStatus.Add(2);
        //        }
        //        else if (authority.ToUpper() == "MYSELF")
        //        {
        //            SanctionStatus.Add(1);
        //            SanctionStatus.Add(2);

        //            ApporvalStatus.Add(3);
        //            ApporvalStatus.Add(4);
        //        }
        //        var ids = Utility.StringIdsToListString(data);
        //        var id = Convert.ToInt32(ids[0]);
        //        var status = ids.Count > 0 ? ids[2] : null;
        //        var emplvId = ids.Count > 0 ? ids[1] : null;
        //        //var LvHeadId = ids.Count > 0 ? ids[3] : null;

        //        //var lvheadidint = Convert.ToInt32(LvHeadId);
        //        var EmpLvIdint = Convert.ToInt32(emplvId);

        //        var Emps = db.EmployeeAppraisal
        //                 .Where(e => (item1.ReportingEmployee.Contains(e.Employee.Id)))
        //            //.Where(e => e.Id == Id)
        //                  .Include(e => e.Employee.ReportingStructRights)
        //                  .Include(e => e.Employee.ReportingStructRights.Select(b => b.AccessRights.ActionName))
        //                  .Include(e => e.Employee.ReportingStructRights.Select(b => b.FuncModules))
        //                  .Include(e => e.EmpAppEvaluation)
        //                  .Include(e => e.Employee)
        //                  .Include(e => e.EmpAppEvaluation.Select(a => a.EmpAppRatingConclusion))
        //                  .Include(e => e.EmpAppEvaluation.Select(a => a.EmpAppRatingConclusion.Select(t => t.AppraisalAssistance)))
        //                  .Include(e => e.EmpAppEvaluation.Select(a => a.EmpAppRatingConclusion.Select(t => t.EmpAppRating)))
        //                  .Include(e => e.EmpAppEvaluation.Select(a => a.EmpAppRatingConclusion.Select(t => t.EmpAppRating.Select(x => x.AppAssignment))))
        //                  .Include(e => e.EmpAppEvaluation.Select(a => a.EmpAppRatingConclusion.Select(t => t.EmpAppRating.Select(x => x.AppAssignment.AppCategory))))
        //                  .Include(e => e.EmpAppEvaluation.Select(a => a.EmpAppRatingConclusion.Select(t => t.EmpAppRating.Select(x => x.AppAssignment.AppSubCategory))))
        //             .ToList();
        //        var W = db.EmployeePayroll
        //            .Include(e => e.HotelBookingRequest)
        //            .Include(e => e.Employee.EmpName)
        //            .Include(e => e.HotelBookingRequest.Select(t => t.GeoStruct))
        //            .Include(e => e.HotelBookingRequest.Select(t => t.FuncStruct))
        //            .Include(e => e.HotelBookingRequest.Select(t => t.PayStruct))
        //            .Include(e => e.HotelBookingRequest.Select(t => t.HotBookReqDetails))
        //            .Include(e => e.HotelBookingRequest.Select(t => t.WFStatus))
        //            .Include(e => e.HotelBookingRequest.Select(t => t.HotelEligibilityPolicy))
        //            .Include(e => e.HotelBookingRequest.Select(t => t.City))
        //            .Include(e => e.HotelBookingRequest.Select(t => t.State))
        //            .Include(e => e.HotelBookingRequest.Select(t => t.Country))
        //            .Include(e => e.HotelBookingRequest.Select(t => t.FamilyDetails))
        //            .Include(e => e.HotelBookingRequest.Select(t => t.FamilyDetails.Select(a => a.MemberName)))


        //            .Where(e => e.Employee.Id == EmpLvIdint && e.HotelBookingRequest.Any(w => w.Id == id)).SingleOrDefault();

        //        var v = W.HotelBookingRequest.Where(e => e.Id == id).Select(s => new EmpHotelREquestdata
        //        {
        //            EmployeeId = W.Employee.Id,
        //            EmployeeName = W.Employee.EmpCode + " " + W.Employee.EmpName.FullNameFML,
        //            Lvnewreq = s.Id,
        //            Empcode = W.Employee.EmpCode,

        //            //Branch = W.Employee.GeoStruct != null && W.Employee.GeoStruct.Location != null && W.Employee.GeoStruct.Location.LocationObj != null ? W.Employee.GeoStruct.Location.LocationObj.LocDesc.ToString() : null,
        //            //Department = W.Employee.GeoStruct != null && W.Employee.GeoStruct.Department != null && W.Employee.GeoStruct.Department.DepartmentObj != null ? W.Employee.GeoStruct.Department.DepartmentObj.DeptDesc.ToString() : null,
        //            //Designation = W.Employee.FuncStruct != null && W.Employee.FuncStruct.Job != null ? W.Employee.FuncStruct.Job.Name : null,
        //            Hotelname = s.HotelName,
        //            Hoteldesc = s.HotelDesc,
        //            Country = s.Country.Id,
        //            City = s.City.Id,
        //            State = s.State.Id,
        //            IsFamilyIncl = s.IsFamilyIncl,
        //            FamilymembernameId = s.FamilyDetails != null && s.FamilyDetails.Count > 0 ? s.FamilyDetails.Select(m => m.MemberName.Id).FirstOrDefault() : 0,
        //            Familymembername = s.FamilyDetails != null && s.FamilyDetails.Count > 0 ? s.FamilyDetails.Select(m => m.MemberName.FullNameFML).FirstOrDefault() : null,

        //            Billno = s.BillNo,
        //            BillAmount = s.BillAmount,
        //            Eligible_BillAmount = s.Eligible_BillAmount,
        //            TotalAdults = s.TotalAdults,
        //            TotalChild = s.TotalChild,
        //            TotalInfant = s.TotalInfant,
        //            TotalSrCitizen = s.TotalSrCitizen,
        //            TotFamilyMembers = s.TotFamilyMembers,
        //            Start_Date = s.StartDate != null ? s.StartDate.Value.ToShortDateString() : null,
        //            End_Date = s.EndDate != null ? s.EndDate.Value.ToShortDateString() : null,
        //            NoOfRooms = s.NoOfRooms.ToString(),
        //            //Status = status,

        //            StdDiscount = s.StdDiscount,
        //            Taxes = s.Taxes,
        //            Narration = s.Narration,
        //            RatePerDay = s.RatePerDay,
        //            SpecialRemark = s.SpecialRemark,
        //            //EmpContactNO = s.ContactNo != null ? s.ContactNo.FullContactNumbers : null,
        //            Isclose = status.ToString(),
        //            //Id = s.Id,
        //            TrClosed = s.TrClosed,
        //            SanctionCode = s.HotBookReqDetails != null && s.HotBookReqDetails.Count > 0 ? s.HotBookReqDetails.Where(z => WfStatus.Contains(z.WFStatus)).OrderByDescending(e => e.Id).Select(e => e.DBTrack.CreatedBy).FirstOrDefault() : null,
        //            SanctionComment = s.HotBookReqDetails != null && s.HotBookReqDetails.Count > 0 ? s.HotBookReqDetails.Where(z => SanctionStatus.Contains(z.WFStatus)).OrderByDescending(e => e.Id).Select(e => e.Comments).FirstOrDefault() : null,
        //            ApporavalComment = s.HotBookReqDetails != null && s.HotBookReqDetails.Count > 0 ? s.HotBookReqDetails.Where(z => ApporvalStatus.Contains(z.WFStatus)).OrderByDescending(e => e.Id).Select(e => e.Comments).FirstOrDefault() : null,
        //            Wf = s.HotBookReqDetails != null && s.HotBookReqDetails.Count > 0 ? s.HotBookReqDetails.Where(z => WfStatus.Contains(z.WFStatus)).OrderByDescending(e => e.Id).Select(e => e.Comments).LastOrDefault() : null,
        //            RecomendationCode = s.HotBookReqDetails != null && s.HotBookReqDetails.Count > 0 ? s.HotBookReqDetails.Where(z => RecomendationStatus.Contains(z.WFStatus)).OrderByDescending(e => e.Id).Select(e => e.DBTrack.CreatedBy).FirstOrDefault() : null,
        //            RecomendationEmpname = s.HotBookReqDetails != null && s.HotBookReqDetails.Count > 0 ? s.HotBookReqDetails.Where(z => RecomendationStatus.Contains(z.WFStatus)).OrderByDescending(e => e.Id).Select(e => e.Comments).FirstOrDefault() : null,
        //            // Incharge = s.Incharge != null ? s.Incharge.EmpCode + ' ' + s.Incharge.EmpName.FullDetails.ToString() : null
        //        }).ToList();


        //        return Json(v, JsonRequestBehavior.AllowGet);
        //    }
        //}
        //#endregion


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


                Calendar DefaultCalendaryearid = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "APPRAISALCALENDAR" && e.Default == true).SingleOrDefault();

                //List<GetEmptrainingNeeddata> returndata = new List<GetEmptrainingNeeddata>();
                List<EmpAppRatingConclusionDetails> EmpAppRatingConclusionDetailsList = new List<EmpAppRatingConclusionDetails>();
                List<int> empappracategory = new List<int>();
                List<int> empapprasubcategory = new List<int>();
                bool appraisalpublishoverornot = false;
                //var Sal = db.CompanyPayroll.Where(e => e.Company.Id == id).Include(e => e.SalHeadFormula).Select(e => e.SalHeadFormula).SingleOrDefault();
                var db_data = db.EmployeeAppraisal
                       .Where(e => e.Employee.Id == Employeeid)
                       .Include(e => e.EmpAppEvaluation)
                       .Include(e => e.EmpAppEvaluation.Select(a => a.AppraisalPeriodCalendar))
                       .Include(e => e.Employee)
                       .Include(e => e.EmpAppEvaluation.Select(a => a.EmpAppRatingConclusion))
                       .Include(e => e.EmpAppEvaluation.Select(a => a.AppraisalSchedule))
                       .Include(e => e.EmpAppEvaluation.Select(a => a.AppraisalSchedule.AppraisalPublish))
                       .Include(e => e.EmpAppEvaluation.Select(a => a.EmpAppRatingConclusion.Select(t => t.AppraisalAssistance)))
                       .Include(e => e.EmpAppEvaluation.Select(a => a.EmpAppRatingConclusion.Select(t => t.EmpAppRating)))
                       .Include(e => e.EmpAppEvaluation.Select(a => a.EmpAppRatingConclusion.Select(t => t.EmpAppRating.Select(x => x.AppAssignment))))
                       .Include(e => e.EmpAppEvaluation.Select(a => a.EmpAppRatingConclusion.Select(t => t.EmpAppRating.Select(x => x.AppAssignment.AppCategory))))
                       .Include(e => e.EmpAppEvaluation.Select(a => a.EmpAppRatingConclusion.Select(t => t.EmpAppRating.Select(x => x.AppAssignment.AppSubCategory))))
                      .SingleOrDefault();



                var emplk = db.Employee
                        .Include(e => e.GeoStruct)
                        .Include(e => e.FuncStruct)
                        .Include(e => e.PayStruct).Where(e => e.Id == Employeeid).SingleOrDefault();

                DateTime currentdate = DateTime.Now.Date;


                List<GetLvNewReqClass> returndata = new List<GetLvNewReqClass>();

                string AccessRight = "";
                if (!string.IsNullOrEmpty(Convert.ToString(Session["user-module"])))
                {
                    AccessRight = Convert.ToString(Session["auho"]);
                }

                string LookupAppDATA = string.Empty;
                if (AccessRight == "Sanction")
                {

                    LookupAppDATA = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1068").FirstOrDefault().LookupValues.Where(l => l.LookupVal.ToUpper() == "AppraiserSanction".ToUpper()).FirstOrDefault().LookupVal;
                }
                else
                {

                    LookupAppDATA = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1068").FirstOrDefault().LookupValues.Where(l => l.LookupVal.ToUpper() == "HR".ToUpper()).FirstOrDefault().LookupVal;
                }

                List<EmpAppRatingConclusion> empratingdata = null;
                if (db_data != null)
                {
                    var defaultcalendarwise = db_data.EmpAppEvaluation.Where(e => e.AppraisalPeriodCalendar.Id == DefaultCalendaryearid.Id).ToList();

                    //foreach (var item in db_data.EmpAppEvaluation)
                    //{
                    foreach (var item in defaultcalendarwise)
                    {


                        var AppAssignmentList = db.AppAssignment.Include(e => e.AppCategory).Include(e => e.AppSubCategory).Include(e => e.AppraisalCalendar)
                                        .Where(e => e.AppraisalCalendar.Id == item.AppraisalPeriodCalendar.Id).ToList();

                        foreach (var getitem in AppAssignmentList)
                        {
                            if (getitem.AppCategory != null)
                            {
                                empappracategory.Add(getitem.AppCategory.Id);

                            }
                            if (getitem.AppSubCategory != null)
                            {
                                empapprasubcategory.Add(getitem.AppSubCategory.Id);
                            }

                        }
                        empappracategory = empappracategory.Distinct().ToList();
                        empapprasubcategory = empapprasubcategory.Distinct().ToList();
                    }
                    if (db_data.EmpAppEvaluation.Count() > 0)
                    {

                        //  var EmpAppLIST = db_data.EmpAppEvaluation.FirstOrDefault();
                        var EmpAppLIST = db_data.EmpAppEvaluation.Where(e => e.AppraisalPeriodCalendar.Id == DefaultCalendaryearid.Id).ToList();
                        List<EmpAppEvaluation> ObjempappLIST = new List<EmpAppEvaluation>();
                        ObjempappLIST.AddRange(EmpAppLIST);


                        List<int> appidslist = new List<int>();
                        AppAssignment chkAppassign = null;
                        foreach (var item in ObjempappLIST)
                        {

                            int spanperiod = item.AppraisalSchedule.AppraisalPublish.FirstOrDefault().SpanPeriod + item.AppraisalSchedule.AppraisalPublish.FirstOrDefault().Extension;
                            DateTime? date1 = item.AppraisalSchedule.AppraisalPublish.FirstOrDefault().PublishDate;
                            DateTime date2 = date1.Value.AddDays(spanperiod);
                            if (currentdate >= date1 && currentdate <= date2)
                            {
                                appraisalpublishoverornot = true;

                                //var AppAsslistgeostuct = db.AppAssignment.Include(e => e.AppCategory).Include(e => e.AppSubCategory).Include(e => e.GeoStruct)
                                //             .Where(e => e.GeoStruct.Id == emplk.GeoStruct.Id && empappracategory.Contains(e.AppCategory.Id) && empapprasubcategory.Contains(e.AppSubCategory.Id)).ToList();


                                var AppAsslistgeostuct = db.AppAssignment.Include(e => e.AppCategory).Include(e => e.AppSubCategory).Include(e => e.GeoStruct)
                                        .Where(e => e.GeoStruct.Id == emplk.GeoStruct.Id && e.PayStruct.Id == emplk.PayStruct.Id && e.FuncStruct.Id == emplk.FuncStruct.Id
                                        && empappracategory.Contains(e.AppCategory.Id) && empapprasubcategory.Contains(e.AppSubCategory.Id)).ToList();


                                if (AppAsslistgeostuct.Count() > 0)
                                {
                                    foreach (var itemApp in AppAsslistgeostuct)
                                    {
                                        int AppID = itemApp.AppCategory.Id;
                                        appidslist.Add(AppID);
                                    }
                                    for (int i = 0; i < appidslist.Count(); i++)
                                    {
                                        for (int j = i + 1; j < appidslist.Count(); j++)
                                        {
                                            if (appidslist[i] == appidslist[j])
                                            {
                                                int chk = appidslist[j];
                                                chkAppassign = AppAsslistgeostuct.Where(e => e.AppCategory.Id == chk).FirstOrDefault();
                                                AppAsslistgeostuct.Remove(chkAppassign);
                                            }
                                        }
                                    }
                                }

                                if (AppAsslistgeostuct.Count() != 0)
                                {

                                    foreach (var AppAsslist in AppAsslistgeostuct)
                                    {

                                        var EmpAppEval = db_data.EmpAppEvaluation.Where(e => e.AppraisalPeriodCalendar.Id == AppAsslist.AppraisalCalendar.Id).Select(c => new
                                        {
                                            EmpAppEvalId = c.Id,
                                            EmpAppRatingConclusionVal = c.EmpAppRatingConclusion.Select(r => new
                                            {
                                                EmpAppRatingConclId = r.Id,
                                                AppraisalAssistanceVal = r.AppraisalAssistance.LookupVal,
                                                eraatateVal = r.EmpAppRating.Select(p => new
                                                {
                                                    EmpAppAssignmentId = p.AppAssignment.Id,
                                                    EmpAppCOmments = p.Comments,
                                                    EmpAppRatePoint = p.RatingPoints,
                                                }).Where(z => z.EmpAppAssignmentId == AppAsslist.Id).ToList(),
                                            }).Where(p => p.AppraisalAssistanceVal.ToUpper() == LookupAppDATA.ToUpper()).ToList(),

                                        }).Where(z => z.EmpAppEvalId == item.Id).ToList();

                                        foreach (var SingleEmpAppEval in EmpAppEval)
                                        {
                                            var GetEARvalue = SingleEmpAppEval.EmpAppRatingConclusionVal.Where(e => e.eraatateVal.Count() > 0).Select(s => s.eraatateVal).ToList();


                                            if (SingleEmpAppEval.EmpAppRatingConclusionVal.Count() > 0 && GetEARvalue.Count() > 0)
                                            {
                                                foreach (var SingleEmpCon in SingleEmpAppEval.EmpAppRatingConclusionVal)
                                                {
                                                    foreach (var SingleEmpAppRating in SingleEmpCon.eraatateVal)
                                                    {
                                                        returndata.Add(new GetLvNewReqClass
                                                        {
                                                            Id = AppAsslist.Id.ToString(),
                                                            Category = AppAsslist == null ? "" : AppAsslist.AppCategory == null ? "" : AppAsslist.AppCategory.Name == null ? "" : AppAsslist.AppCategory.Name.ToString(),
                                                            Subcategory = AppAsslist == null ? "" : AppAsslist.AppSubCategory == null ? "" : AppAsslist.AppSubCategory.Name == null ? "" : AppAsslist.AppSubCategory.Name.ToString(),

                                                            MaxPoint = AppAsslist == null ? "0" : AppAsslist.MaxRatingPoints == null ? "0" : AppAsslist.MaxRatingPoints.ToString(),
                                                            RatingPoint = SingleEmpCon == null && SingleEmpAppRating == null && SingleEmpAppRating.EmpAppRatePoint == null ? "0" : SingleEmpAppRating.EmpAppRatePoint.ToString(),
                                                            Comment = SingleEmpCon == null && SingleEmpAppRating == null && SingleEmpAppRating.EmpAppCOmments == null ? "" : SingleEmpAppRating.EmpAppCOmments.ToString(),
                                                            EmpEvaluationId = SingleEmpAppEval.EmpAppEvalId.ToString(),

                                                        });
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                returndata.Add(new GetLvNewReqClass
                                                {
                                                    Id = AppAsslist.Id.ToString(),
                                                    Category = AppAsslist == null ? "" : AppAsslist.AppCategory == null ? "" : AppAsslist.AppCategory.Name == null ? "" : AppAsslist.AppCategory.Name.ToString(),
                                                    Subcategory = AppAsslist == null ? "" : AppAsslist.AppSubCategory == null ? "" : AppAsslist.AppSubCategory.Name == null ? "" : AppAsslist.AppSubCategory.Name.ToString(),

                                                    MaxPoint = AppAsslist == null ? "0" : AppAsslist.MaxRatingPoints == null ? "0" : AppAsslist.MaxRatingPoints.ToString(),
                                                    RatingPoint = "0",
                                                    Comment = "",
                                                    EmpEvaluationId = SingleEmpAppEval.EmpAppEvalId.ToString(),

                                                });
                                            }


                                        }


                                    }
                                }



                                var AppAsslistFuncstruct = db.AppAssignment.Include(e => e.AppCategory).Include(e => e.AppSubCategory).Include(e => e.GeoStruct)
                                        .Where(e => e.GeoStruct.Id == emplk.GeoStruct.Id && e.PayStruct.Id == emplk.PayStruct.Id && e.FuncStruct.Id == null
                                        && empappracategory.Contains(e.AppCategory.Id) && empapprasubcategory.Contains(e.AppSubCategory.Id)).ToList();


                                if (AppAsslistFuncstruct.Count() > 0)
                                {
                                    foreach (var itemApp in AppAsslistFuncstruct)
                                    {
                                        int AppID = itemApp.AppCategory.Id;
                                        appidslist.Add(AppID);
                                    }
                                    for (int i = 0; i < appidslist.Count(); i++)
                                    {
                                        for (int j = i + 1; j < appidslist.Count(); j++)
                                        {
                                            if (appidslist[i] == appidslist[j])
                                            {
                                                int chk = appidslist[j];
                                                chkAppassign = AppAsslistFuncstruct.Where(e => e.AppCategory.Id == chk).FirstOrDefault();
                                                AppAsslistFuncstruct.Remove(chkAppassign);
                                            }
                                        }
                                    }
                                }


                                if (AppAsslistFuncstruct.Count() != 0)
                                {

                                    foreach (var AppAsslist in AppAsslistFuncstruct)
                                    {

                                        var EmpAppEval = db_data.EmpAppEvaluation.Where(e => e.AppraisalPeriodCalendar.Id == AppAsslist.AppraisalCalendar.Id).Select(c => new
                                        {
                                            EmpAppEvalId = c.Id,
                                            EmpAppRatingConclusionVal = c.EmpAppRatingConclusion.Select(r => new
                                            {
                                                EmpAppRatingConclId = r.Id,
                                                AppraisalAssistanceVal = r.AppraisalAssistance.LookupVal,
                                                eraatateVal = r.EmpAppRating.Select(p => new
                                                {
                                                    EmpAppAssignmentId = p.AppAssignment.Id,
                                                    EmpAppCOmments = p.Comments,
                                                    EmpAppRatePoint = p.RatingPoints,
                                                }).Where(z => z.EmpAppAssignmentId == AppAsslist.Id).ToList(),
                                            }).Where(p => p.AppraisalAssistanceVal.ToUpper() == LookupAppDATA.ToUpper()).ToList(),

                                        }).Where(z => z.EmpAppEvalId == item.Id).ToList();



                                        foreach (var SingleEmpAppEval in EmpAppEval)
                                        {
                                            var GetEARvalue = SingleEmpAppEval.EmpAppRatingConclusionVal.Where(e => e.eraatateVal.Count() > 0).Select(s => s.eraatateVal).ToList();


                                            if (SingleEmpAppEval.EmpAppRatingConclusionVal.Count() > 0 && GetEARvalue.Count() > 0)
                                            {

                                                foreach (var SingleEmpCon in SingleEmpAppEval.EmpAppRatingConclusionVal)
                                                {

                                                    foreach (var SingleEmpAppRating in SingleEmpCon.eraatateVal)
                                                    {
                                                        returndata.Add(new GetLvNewReqClass
                                                        {
                                                            Id = AppAsslist.Id.ToString(),
                                                            Category = AppAsslist == null ? "" : AppAsslist.AppCategory == null ? "" : AppAsslist.AppCategory.Name == null ? "" : AppAsslist.AppCategory.Name.ToString(),
                                                            Subcategory = AppAsslist == null ? "" : AppAsslist.AppSubCategory == null ? "" : AppAsslist.AppSubCategory.Name == null ? "" : AppAsslist.AppSubCategory.Name.ToString(),

                                                            MaxPoint = AppAsslist == null ? "0" : AppAsslist.MaxRatingPoints == null ? "0" : AppAsslist.MaxRatingPoints.ToString(),
                                                            RatingPoint = SingleEmpCon == null && SingleEmpAppRating == null && SingleEmpAppRating.EmpAppRatePoint == null ? "0" : SingleEmpAppRating.EmpAppRatePoint.ToString(),
                                                            Comment = SingleEmpCon == null && SingleEmpAppRating == null && SingleEmpAppRating.EmpAppCOmments == null ? "" : SingleEmpAppRating.EmpAppCOmments.ToString(),
                                                            EmpEvaluationId = SingleEmpAppEval.EmpAppEvalId.ToString(),

                                                        });
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                returndata.Add(new GetLvNewReqClass
                                                {
                                                    Id = AppAsslist.Id.ToString(),
                                                    Category = AppAsslist == null ? "" : AppAsslist.AppCategory == null ? "" : AppAsslist.AppCategory.Name == null ? "" : AppAsslist.AppCategory.Name.ToString(),
                                                    Subcategory = AppAsslist == null ? "" : AppAsslist.AppSubCategory == null ? "" : AppAsslist.AppSubCategory.Name == null ? "" : AppAsslist.AppSubCategory.Name.ToString(),

                                                    MaxPoint = AppAsslist == null ? "0" : AppAsslist.MaxRatingPoints == null ? "0" : AppAsslist.MaxRatingPoints.ToString(),
                                                    RatingPoint = "0",
                                                    Comment = "",
                                                    EmpEvaluationId = SingleEmpAppEval.EmpAppEvalId.ToString(),

                                                });
                                            }


                                        }


                                    }
                                }

                                //var AppAsslistPaystruct = db.AppAssignment.Include(e => e.AppCategory).Include(e => e.AppSubCategory).Include(e => e.PayStruct)
                                //             .Where(e => e.PayStruct.Id == emplk.PayStruct.Id && empappracategory.Contains(e.AppCategory.Id) && empapprasubcategory.Contains(e.AppSubCategory.Id)).Distinct().ToList();

                                var AppAsslistPaystruct = db.AppAssignment.Include(e => e.AppCategory).Include(e => e.AppSubCategory).Include(e => e.GeoStruct)
                                         .Where(e => e.GeoStruct.Id == emplk.GeoStruct.Id && e.PayStruct.Id == null && e.FuncStruct.Id == emplk.FuncStruct.Id
                                         && empappracategory.Contains(e.AppCategory.Id) && empapprasubcategory.Contains(e.AppSubCategory.Id)).ToList();

                                if (AppAsslistPaystruct.Count() > 0)
                                {
                                    foreach (var itemApp in AppAsslistPaystruct)
                                    {
                                        int AppID = itemApp.AppCategory.Id;
                                        appidslist.Add(AppID);
                                    }
                                    for (int i = 0; i < appidslist.Count(); i++)
                                    {
                                        for (int j = i + 1; j < appidslist.Count(); j++)
                                        {
                                            if (appidslist[i] == appidslist[j])
                                            {
                                                int chk = appidslist[j];
                                                chkAppassign = AppAsslistPaystruct.Where(e => e.AppCategory.Id == chk).FirstOrDefault();
                                                AppAsslistPaystruct.Remove(chkAppassign);
                                            }
                                        }
                                    }


                                }


                                if (AppAsslistPaystruct.Count() != 0)
                                {

                                    foreach (var AppAsslist in AppAsslistPaystruct)
                                    {
                                        var EmpAppEval = db_data.EmpAppEvaluation.Where(e => e.AppraisalPeriodCalendar.Id == AppAsslist.AppraisalCalendar.Id).Select(c => new
                                        {
                                            EmpAppEvalId = c.Id,
                                            EmpAppRatingConclusionVal = c.EmpAppRatingConclusion.Select(r => new
                                            {
                                                EmpAppRatingConclId = r.Id,
                                                AppraisalAssistanceVal = r.AppraisalAssistance.LookupVal,
                                                eraatateVal = r.EmpAppRating.Select(p => new
                                                {
                                                    EmpAppAssignmentId = p.AppAssignment.Id,
                                                    EmpAppCOmments = p.Comments,
                                                    EmpAppRatePoint = p.RatingPoints,
                                                }).Where(z => z.EmpAppAssignmentId == AppAsslist.Id).ToList(),
                                            }).Where(p => p.AppraisalAssistanceVal.ToUpper() == LookupAppDATA.ToUpper()).ToList(),

                                        }).Where(z => z.EmpAppEvalId == item.Id).ToList();

                                        foreach (var SingleEmpAppEval in EmpAppEval)
                                        {
                                            var GetEARvalue = SingleEmpAppEval.EmpAppRatingConclusionVal.Where(e => e.eraatateVal.Count() > 0).Select(s => s.eraatateVal).ToList();


                                            if (SingleEmpAppEval.EmpAppRatingConclusionVal.Count() > 0 && GetEARvalue.Count() > 0)
                                            {
                                                foreach (var SingleEmpCon in SingleEmpAppEval.EmpAppRatingConclusionVal)
                                                {
                                                    foreach (var SingleEmpAppRating in SingleEmpCon.eraatateVal)
                                                    {
                                                        returndata.Add(new GetLvNewReqClass
                                                        {
                                                            Id = AppAsslist.Id.ToString(),
                                                            Category = AppAsslist == null ? "" : AppAsslist.AppCategory == null ? "" : AppAsslist.AppCategory.Name == null ? "" : AppAsslist.AppCategory.Name.ToString(),
                                                            Subcategory = AppAsslist == null ? "" : AppAsslist.AppSubCategory == null ? "" : AppAsslist.AppSubCategory.Name == null ? "" : AppAsslist.AppSubCategory.Name.ToString(),

                                                            MaxPoint = AppAsslist == null ? "0" : AppAsslist.MaxRatingPoints == null ? "0" : AppAsslist.MaxRatingPoints.ToString(),
                                                            RatingPoint = SingleEmpCon == null && SingleEmpAppRating == null && SingleEmpAppRating.EmpAppRatePoint == null ? "0" : SingleEmpAppRating.EmpAppRatePoint.ToString(),
                                                            Comment = SingleEmpCon == null && SingleEmpAppRating == null && SingleEmpAppRating.EmpAppCOmments == null ? "" : SingleEmpAppRating.EmpAppCOmments.ToString(),
                                                            EmpEvaluationId = SingleEmpAppEval.EmpAppEvalId.ToString(),

                                                        });
                                                    }
                                                }
                                            }
                                            else
                                            {

                                                returndata.Add(new GetLvNewReqClass
                                                {
                                                    Id = AppAsslist.Id.ToString(),
                                                    Category = AppAsslist == null ? "" : AppAsslist.AppCategory == null ? "" : AppAsslist.AppCategory.Name == null ? "" : AppAsslist.AppCategory.Name.ToString(),
                                                    Subcategory = AppAsslist == null ? "" : AppAsslist.AppSubCategory == null ? "" : AppAsslist.AppSubCategory.Name == null ? "" : AppAsslist.AppSubCategory.Name.ToString(),

                                                    MaxPoint = AppAsslist == null ? "0" : AppAsslist.MaxRatingPoints == null ? "0" : AppAsslist.MaxRatingPoints.ToString(),
                                                    RatingPoint = "0",
                                                    Comment = "",
                                                    EmpEvaluationId = SingleEmpAppEval.EmpAppEvalId.ToString(),

                                                });
                                            }

                                        }


                                    }
                                }

                                // Pay and Func
                                var AppAsslistPayFuncstruct = db.AppAssignment.Include(e => e.AppCategory).Include(e => e.AppSubCategory).Include(e => e.GeoStruct)
                                         .Where(e => e.GeoStruct.Id == null && e.PayStruct.Id == emplk.PayStruct.Id && e.FuncStruct.Id == emplk.FuncStruct.Id
                                         && empappracategory.Contains(e.AppCategory.Id) && empapprasubcategory.Contains(e.AppSubCategory.Id)).ToList();

                                if (AppAsslistPayFuncstruct.Count() > 0)
                                {
                                    foreach (var itemApp in AppAsslistPaystruct)
                                    {
                                        int AppID = itemApp.AppCategory.Id;
                                        appidslist.Add(AppID);
                                    }
                                    for (int i = 0; i < appidslist.Count(); i++)
                                    {
                                        for (int j = i + 1; j < appidslist.Count(); j++)
                                        {
                                            if (appidslist[i] == appidslist[j])
                                            {
                                                int chk = appidslist[j];
                                                chkAppassign = AppAsslistPayFuncstruct.Where(e => e.AppCategory.Id == chk).FirstOrDefault();
                                                AppAsslistPayFuncstruct.Remove(chkAppassign);
                                            }
                                        }
                                    }

                                }

                                if (AppAsslistPayFuncstruct.Count() != 0)
                                {

                                    foreach (var AppAsslist in AppAsslistPayFuncstruct)
                                    {
                                        var EmpAppEval = db_data.EmpAppEvaluation.Where(e => e.AppraisalPeriodCalendar.Id == AppAsslist.AppraisalCalendar.Id).Select(c => new
                                        {
                                            EmpAppEvalId = c.Id,
                                            EmpAppRatingConclusionVal = c.EmpAppRatingConclusion.Select(r => new
                                            {
                                                EmpAppRatingConclId = r.Id,
                                                AppraisalAssistanceVal = r.AppraisalAssistance.LookupVal,
                                                eraatateVal = r.EmpAppRating.Select(p => new
                                                {
                                                    EmpAppAssignmentId = p.AppAssignment.Id,
                                                    EmpAppCOmments = p.Comments,
                                                    EmpAppRatePoint = p.RatingPoints,
                                                }).Where(z => z.EmpAppAssignmentId == AppAsslist.Id).ToList(),
                                            }).Where(p => p.AppraisalAssistanceVal.ToUpper() == LookupAppDATA.ToUpper()).ToList(),

                                        }).Where(z => z.EmpAppEvalId == item.Id).ToList();

                                        foreach (var SingleEmpAppEval in EmpAppEval)
                                        {
                                            var GetEARvalue = SingleEmpAppEval.EmpAppRatingConclusionVal.Where(e => e.eraatateVal.Count() > 0).Select(s => s.eraatateVal).ToList();


                                            if (SingleEmpAppEval.EmpAppRatingConclusionVal.Count() > 0 && GetEARvalue.Count() > 0)
                                            {
                                                foreach (var SingleEmpCon in SingleEmpAppEval.EmpAppRatingConclusionVal)
                                                {
                                                    foreach (var SingleEmpAppRating in SingleEmpCon.eraatateVal)
                                                    {
                                                        returndata.Add(new GetLvNewReqClass
                                                        {
                                                            Id = AppAsslist.Id.ToString(),
                                                            Category = AppAsslist == null ? "" : AppAsslist.AppCategory == null ? "" : AppAsslist.AppCategory.Name == null ? "" : AppAsslist.AppCategory.Name.ToString(),
                                                            Subcategory = AppAsslist == null ? "" : AppAsslist.AppSubCategory == null ? "" : AppAsslist.AppSubCategory.Name == null ? "" : AppAsslist.AppSubCategory.Name.ToString(),

                                                            MaxPoint = AppAsslist == null ? "0" : AppAsslist.MaxRatingPoints == null ? "0" : AppAsslist.MaxRatingPoints.ToString(),
                                                            RatingPoint = SingleEmpCon == null && SingleEmpAppRating == null && SingleEmpAppRating.EmpAppRatePoint == null ? "0" : SingleEmpAppRating.EmpAppRatePoint.ToString(),
                                                            Comment = SingleEmpCon == null && SingleEmpAppRating == null && SingleEmpAppRating.EmpAppCOmments == null ? "" : SingleEmpAppRating.EmpAppCOmments.ToString(),
                                                            EmpEvaluationId = SingleEmpAppEval.EmpAppEvalId.ToString(),

                                                        });
                                                    }
                                                }
                                            }
                                            else
                                            {

                                                returndata.Add(new GetLvNewReqClass
                                                {
                                                    Id = AppAsslist.Id.ToString(),
                                                    Category = AppAsslist == null ? "" : AppAsslist.AppCategory == null ? "" : AppAsslist.AppCategory.Name == null ? "" : AppAsslist.AppCategory.Name.ToString(),
                                                    Subcategory = AppAsslist == null ? "" : AppAsslist.AppSubCategory == null ? "" : AppAsslist.AppSubCategory.Name == null ? "" : AppAsslist.AppSubCategory.Name.ToString(),

                                                    MaxPoint = AppAsslist == null ? "0" : AppAsslist.MaxRatingPoints == null ? "0" : AppAsslist.MaxRatingPoints.ToString(),
                                                    RatingPoint = "0",
                                                    Comment = "",
                                                    EmpEvaluationId = SingleEmpAppEval.EmpAppEvalId.ToString(),

                                                });
                                            }

                                        }


                                    }
                                }



                                var AppAsslistGEOstruct = db.AppAssignment.Include(e => e.AppCategory).Include(e => e.AppSubCategory).Include(e => e.GeoStruct)
                                                                    .Where(e => e.GeoStruct.Id == emplk.GeoStruct.Id && e.PayStruct.Id == null && e.FuncStruct.Id == null
                                                                    && empappracategory.Contains(e.AppCategory.Id) && empapprasubcategory.Contains(e.AppSubCategory.Id)).ToList();

                                if (AppAsslistGEOstruct.Count() > 0)
                                {
                                    foreach (var itemApp in AppAsslistGEOstruct)
                                    {
                                        int AppID = itemApp.AppCategory.Id;
                                        appidslist.Add(AppID);
                                    }
                                    for (int i = 0; i < appidslist.Count(); i++)
                                    {
                                        for (int j = i + 1; j < appidslist.Count(); j++)
                                        {
                                            if (appidslist[i] == appidslist[j])
                                            {
                                                int chk = appidslist[j];
                                                chkAppassign = AppAsslistGEOstruct.Where(e => e.AppCategory.Id == chk).FirstOrDefault();
                                                AppAsslistGEOstruct.Remove(chkAppassign);
                                            }
                                        }
                                    }
                                }


                                if (AppAsslistGEOstruct.Count() != 0)
                                {

                                    foreach (var AppAsslist in AppAsslistGEOstruct)
                                    {
                                        var EmpAppEval = db_data.EmpAppEvaluation.Where(e => e.AppraisalPeriodCalendar.Id == AppAsslist.AppraisalCalendar.Id).Select(c => new
                                        {
                                            EmpAppEvalId = c.Id,
                                            EmpAppRatingConclusionVal = c.EmpAppRatingConclusion.Select(r => new
                                            {
                                                EmpAppRatingConclId = r.Id,
                                                AppraisalAssistanceVal = r.AppraisalAssistance.LookupVal,
                                                eraatateVal = r.EmpAppRating.Select(p => new
                                                {
                                                    EmpAppAssignmentId = p.AppAssignment.Id,
                                                    EmpAppCOmments = p.Comments,
                                                    EmpAppRatePoint = p.RatingPoints,
                                                }).Where(z => z.EmpAppAssignmentId == AppAsslist.Id).ToList(),
                                            }).Where(p => p.AppraisalAssistanceVal.ToUpper() == LookupAppDATA.ToUpper()).ToList(),

                                        }).Where(z => z.EmpAppEvalId == item.Id).ToList();

                                        foreach (var SingleEmpAppEval in EmpAppEval)
                                        {
                                            var GetEARvalue = SingleEmpAppEval.EmpAppRatingConclusionVal.Where(e => e.eraatateVal.Count() > 0).Select(s => s.eraatateVal).ToList();


                                            if (SingleEmpAppEval.EmpAppRatingConclusionVal.Count() > 0 && GetEARvalue.Count() > 0)
                                            {
                                                foreach (var SingleEmpCon in SingleEmpAppEval.EmpAppRatingConclusionVal)
                                                {
                                                    foreach (var SingleEmpAppRating in SingleEmpCon.eraatateVal)
                                                    {
                                                        returndata.Add(new GetLvNewReqClass
                                                        {
                                                            Id = AppAsslist.Id.ToString(),
                                                            Category = AppAsslist == null ? "" : AppAsslist.AppCategory == null ? "" : AppAsslist.AppCategory.Name == null ? "" : AppAsslist.AppCategory.Name.ToString(),
                                                            Subcategory = AppAsslist == null ? "" : AppAsslist.AppSubCategory == null ? "" : AppAsslist.AppSubCategory.Name == null ? "" : AppAsslist.AppSubCategory.Name.ToString(),

                                                            MaxPoint = AppAsslist == null ? "0" : AppAsslist.MaxRatingPoints == null ? "0" : AppAsslist.MaxRatingPoints.ToString(),
                                                            RatingPoint = SingleEmpCon == null && SingleEmpAppRating == null && SingleEmpAppRating.EmpAppRatePoint == null ? "0" : SingleEmpAppRating.EmpAppRatePoint.ToString(),
                                                            Comment = SingleEmpCon == null && SingleEmpAppRating == null && SingleEmpAppRating.EmpAppCOmments == null ? "" : SingleEmpAppRating.EmpAppCOmments.ToString(),
                                                            EmpEvaluationId = SingleEmpAppEval.EmpAppEvalId.ToString(),

                                                        });
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                returndata.Add(new GetLvNewReqClass
                                                {
                                                    Id = AppAsslist.Id.ToString(),
                                                    Category = AppAsslist == null ? "" : AppAsslist.AppCategory == null ? "" : AppAsslist.AppCategory.Name == null ? "" : AppAsslist.AppCategory.Name.ToString(),
                                                    Subcategory = AppAsslist == null ? "" : AppAsslist.AppSubCategory == null ? "" : AppAsslist.AppSubCategory.Name == null ? "" : AppAsslist.AppSubCategory.Name.ToString(),

                                                    MaxPoint = AppAsslist == null ? "0" : AppAsslist.MaxRatingPoints == null ? "0" : AppAsslist.MaxRatingPoints.ToString(),
                                                    RatingPoint = "0",
                                                    Comment = "",
                                                    EmpEvaluationId = SingleEmpAppEval.EmpAppEvalId.ToString(),

                                                });
                                            }

                                        }


                                    }
                                }


                                var AppAsslistFUNstruct = db.AppAssignment.Include(e => e.AppCategory).Include(e => e.AppSubCategory).Include(e => e.GeoStruct)
                                                                     .Where(e => e.GeoStruct.Id == null && e.PayStruct.Id == null && e.FuncStruct.Id == emplk.FuncStruct.Id
                                                                     && empappracategory.Contains(e.AppCategory.Id) && empapprasubcategory.Contains(e.AppSubCategory.Id)).ToList();

                                if (AppAsslistFUNstruct.Count() > 0)
                                {
                                    foreach (var itemApp in AppAsslistFUNstruct)
                                    {
                                        int AppID = itemApp.AppCategory.Id;
                                        appidslist.Add(AppID);
                                    }
                                    for (int i = 0; i < appidslist.Count(); i++)
                                    {
                                        for (int j = i + 1; j < appidslist.Count(); j++)
                                        {
                                            if (appidslist[i] == appidslist[j])
                                            {
                                                int chk = appidslist[j];
                                                chkAppassign = AppAsslistFUNstruct.Where(e => e.AppCategory.Id == chk).FirstOrDefault();
                                                AppAsslistFUNstruct.Remove(chkAppassign);
                                            }
                                        }
                                    }
                                }


                                if (AppAsslistFUNstruct.Count() != 0)
                                {

                                    foreach (var AppAsslist in AppAsslistFUNstruct)
                                    {
                                        var EmpAppEval = db_data.EmpAppEvaluation.Where(e => e.AppraisalPeriodCalendar.Id == AppAsslist.AppraisalCalendar.Id).Select(c => new
                                        {
                                            EmpAppEvalId = c.Id,
                                            EmpAppRatingConclusionVal = c.EmpAppRatingConclusion.Select(r => new
                                            {
                                                EmpAppRatingConclId = r.Id,
                                                AppraisalAssistanceVal = r.AppraisalAssistance.LookupVal,
                                                eraatateVal = r.EmpAppRating.Select(p => new
                                                {
                                                    EmpAppAssignmentId = p.AppAssignment.Id,
                                                    EmpAppCOmments = p.Comments,
                                                    EmpAppRatePoint = p.RatingPoints,
                                                }).Where(z => z.EmpAppAssignmentId == AppAsslist.Id).ToList(),
                                            }).Where(p => p.AppraisalAssistanceVal.ToUpper() == LookupAppDATA.ToUpper()).ToList(),

                                        }).Where(z => z.EmpAppEvalId == item.Id).ToList();

                                        foreach (var SingleEmpAppEval in EmpAppEval)
                                        {
                                            var GetEARvalue = SingleEmpAppEval.EmpAppRatingConclusionVal.Where(e => e.eraatateVal.Count() > 0).Select(s => s.eraatateVal).ToList();


                                            if (SingleEmpAppEval.EmpAppRatingConclusionVal.Count() > 0 && GetEARvalue.Count() > 0)
                                            {
                                                foreach (var SingleEmpCon in SingleEmpAppEval.EmpAppRatingConclusionVal)
                                                {
                                                    foreach (var SingleEmpAppRating in SingleEmpCon.eraatateVal)
                                                    {
                                                        returndata.Add(new GetLvNewReqClass
                                                        {
                                                            Id = AppAsslist.Id.ToString(),
                                                            Category = AppAsslist == null ? "" : AppAsslist.AppCategory == null ? "" : AppAsslist.AppCategory.Name == null ? "" : AppAsslist.AppCategory.Name.ToString(),
                                                            Subcategory = AppAsslist == null ? "" : AppAsslist.AppSubCategory == null ? "" : AppAsslist.AppSubCategory.Name == null ? "" : AppAsslist.AppSubCategory.Name.ToString(),

                                                            MaxPoint = AppAsslist == null ? "0" : AppAsslist.MaxRatingPoints == null ? "0" : AppAsslist.MaxRatingPoints.ToString(),
                                                            RatingPoint = SingleEmpCon == null && SingleEmpAppRating == null && SingleEmpAppRating.EmpAppRatePoint == null ? "0" : SingleEmpAppRating.EmpAppRatePoint.ToString(),
                                                            Comment = SingleEmpCon == null && SingleEmpAppRating == null && SingleEmpAppRating.EmpAppCOmments == null ? "" : SingleEmpAppRating.EmpAppCOmments.ToString(),
                                                            EmpEvaluationId = SingleEmpAppEval.EmpAppEvalId.ToString(),

                                                        });
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                returndata.Add(new GetLvNewReqClass
                                                {
                                                    Id = AppAsslist.Id.ToString(),
                                                    Category = AppAsslist == null ? "" : AppAsslist.AppCategory == null ? "" : AppAsslist.AppCategory.Name == null ? "" : AppAsslist.AppCategory.Name.ToString(),
                                                    Subcategory = AppAsslist == null ? "" : AppAsslist.AppSubCategory == null ? "" : AppAsslist.AppSubCategory.Name == null ? "" : AppAsslist.AppSubCategory.Name.ToString(),

                                                    MaxPoint = AppAsslist == null ? "0" : AppAsslist.MaxRatingPoints == null ? "0" : AppAsslist.MaxRatingPoints.ToString(),
                                                    RatingPoint = "0",
                                                    Comment = "",
                                                    EmpEvaluationId = SingleEmpAppEval.EmpAppEvalId.ToString(),

                                                });
                                            }

                                        }


                                    }
                                }


                                var AppAsslistPAYYstruct = db.AppAssignment.Include(e => e.AppCategory).Include(e => e.AppSubCategory).Include(e => e.GeoStruct)
                                                                    .Where(e => e.GeoStruct.Id == null && e.PayStruct.Id == emplk.PayStruct.Id && e.FuncStruct.Id == null
                                                                    && empappracategory.Contains(e.AppCategory.Id) && empapprasubcategory.Contains(e.AppSubCategory.Id)).ToList();

                                if (AppAsslistPAYYstruct.Count() > 0)
                                {
                                    foreach (var itemApp in AppAsslistPAYYstruct)
                                    {
                                        int AppID = itemApp.AppCategory.Id;
                                        appidslist.Add(AppID);
                                    }
                                    for (int i = 0; i < appidslist.Count(); i++)
                                    {
                                        for (int j = i + 1; j < appidslist.Count(); j++)
                                        {
                                            if (appidslist[i] == appidslist[j])
                                            {
                                                int chk = appidslist[j];
                                                chkAppassign = AppAsslistPAYYstruct.Where(e => e.AppCategory.Id == chk).FirstOrDefault();
                                                AppAsslistPAYYstruct.Remove(chkAppassign);
                                            }
                                        }
                                    }
                                }


                                if (AppAsslistPAYYstruct.Count() != 0)
                                {

                                    foreach (var AppAsslist in AppAsslistPAYYstruct)
                                    {
                                        var EmpAppEval = db_data.EmpAppEvaluation.Where(e => e.AppraisalPeriodCalendar.Id == AppAsslist.AppraisalCalendar.Id).Select(c => new
                                        {
                                            EmpAppEvalId = c.Id,
                                            EmpAppRatingConclusionVal = c.EmpAppRatingConclusion.Select(r => new
                                            {
                                                EmpAppRatingConclId = r.Id,
                                                AppraisalAssistanceVal = r.AppraisalAssistance.LookupVal,
                                                eraatateVal = r.EmpAppRating.Select(p => new
                                                {
                                                    EmpAppAssignmentId = p.AppAssignment.Id,
                                                    EmpAppCOmments = p.Comments,
                                                    EmpAppRatePoint = p.RatingPoints,
                                                }).Where(z => z.EmpAppAssignmentId == AppAsslist.Id).ToList(),
                                            }).Where(p => p.AppraisalAssistanceVal.ToUpper() == LookupAppDATA.ToUpper()).ToList(),

                                        }).Where(z => z.EmpAppEvalId == item.Id).ToList();

                                        foreach (var SingleEmpAppEval in EmpAppEval)
                                        {
                                            var GetEARvalue = SingleEmpAppEval.EmpAppRatingConclusionVal.Where(e => e.eraatateVal.Count() > 0).Select(s => s.eraatateVal).ToList();


                                            if (SingleEmpAppEval.EmpAppRatingConclusionVal.Count() > 0 && GetEARvalue.Count() > 0)
                                            {
                                                foreach (var SingleEmpCon in SingleEmpAppEval.EmpAppRatingConclusionVal)
                                                {
                                                    foreach (var SingleEmpAppRating in SingleEmpCon.eraatateVal)
                                                    {
                                                        returndata.Add(new GetLvNewReqClass
                                                        {
                                                            Id = AppAsslist.Id.ToString(),
                                                            Category = AppAsslist == null ? "" : AppAsslist.AppCategory == null ? "" : AppAsslist.AppCategory.Name == null ? "" : AppAsslist.AppCategory.Name.ToString(),
                                                            Subcategory = AppAsslist == null ? "" : AppAsslist.AppSubCategory == null ? "" : AppAsslist.AppSubCategory.Name == null ? "" : AppAsslist.AppSubCategory.Name.ToString(),

                                                            MaxPoint = AppAsslist == null ? "0" : AppAsslist.MaxRatingPoints == null ? "0" : AppAsslist.MaxRatingPoints.ToString(),
                                                            RatingPoint = SingleEmpCon == null && SingleEmpAppRating == null && SingleEmpAppRating.EmpAppRatePoint == null ? "0" : SingleEmpAppRating.EmpAppRatePoint.ToString(),
                                                            Comment = SingleEmpCon == null && SingleEmpAppRating == null && SingleEmpAppRating.EmpAppCOmments == null ? "" : SingleEmpAppRating.EmpAppCOmments.ToString(),
                                                            EmpEvaluationId = SingleEmpAppEval.EmpAppEvalId.ToString(),

                                                        });
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                returndata.Add(new GetLvNewReqClass
                                                {
                                                    Id = AppAsslist.Id.ToString(),
                                                    Category = AppAsslist == null ? "" : AppAsslist.AppCategory == null ? "" : AppAsslist.AppCategory.Name == null ? "" : AppAsslist.AppCategory.Name.ToString(),
                                                    Subcategory = AppAsslist == null ? "" : AppAsslist.AppSubCategory == null ? "" : AppAsslist.AppSubCategory.Name == null ? "" : AppAsslist.AppSubCategory.Name.ToString(),

                                                    MaxPoint = AppAsslist == null ? "0" : AppAsslist.MaxRatingPoints == null ? "0" : AppAsslist.MaxRatingPoints.ToString(),
                                                    RatingPoint = "0",
                                                    Comment = "",
                                                    EmpEvaluationId = SingleEmpAppEval.EmpAppEvalId.ToString(),

                                                });
                                            }

                                        }


                                    }
                                }



                            }
                        }

                    }




                }

                if (returndata.Count() > 0)
                {
                    return Json(new { status = true, data = returndata }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = false, data = "Record Not Found." }, JsonRequestBehavior.AllowGet);
                }


            }
        }

        public class EmpmLVdata
        {

            public string CategoryName { get; set; }
            public string SubcategoryName { get; set; }
            public int Maxpoints { get; set; }
            public int RatingPoints { get; set; }
            public int EmpAppConcluId { get; set; }
            public int EmpAppEvalID { get; set; }
            public string Comments { get; set; }
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
            public string EmpAppEvaluationId { get; set; }
            public string AppriasalAssistance { get; set; }
            public string EmpAppRatingId { get; set; }
            public string CatName { get; set; }
            public string SubCatName { get; set; }
            public string MaxPoint { get; set; }
            public string RatingPoints { get; set; }
            public string ObjectiveWordings { get; set; }
            public string Comments { get; set; }
            public string AppraiseePoints { get; set; }
            public string AppraiseeComments { get; set; }
            public string AppraiserPoints { get; set; }
            public string AppraiserComments { get; set; }
            public string HRPoints { get; set; }
            public string HRComments { get; set; }
            public string SRNo { get; set; }
        }

        [HttpPost]
        public ActionResult Create(FormCollection form, String forwarddata, string TrainingData) //Create submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string Emp = form["EmpAppraisal_id"] == null ? null : form["EmpAppraisal_id"];
                string AppraisalCalendar = form["AppCalendardrop"] == null ? null : form["AppCalendardrop"];
                //   string AppAssistance = form["AppraisalAssistancelist"] == null ? null : form["AppraisalAssistancelist"];
                string AppAssistance = form["AppraisalAssistancelist"] == "0" ? "" : form["AppraisalAssistancelist"];
                string AssistanceOverallCo = form["AssistanceOverallComments"] == "" ? "" : form["AssistanceOverallComments"];
                var AppraisalCalendarr = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "AppraisalCalendar".ToUpper() && e.Default == true).SingleOrDefault();
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
                        //List<string> Msgu = new List<string>();
                        //Msgu.Add("  Kindly select employee  ");
                        //return Json(new Utility.JsonReturnClass { success = false, responseText = Msgu }, JsonRequestBehavior.AllowGet);
                        return Json(new Object[] { "", "", "Kindly select employee " }, JsonRequestBehavior.AllowGet);
                        // return Json(new { sucess = true, responseText = "Kindly select employee ." }, JsonRequestBehavior.AllowGet);
                    }
                    var iid = Convert.ToInt32(ids1[0]);


                    var serialize = new JavaScriptSerializer();
                    List<DeserializeClass> obj = new List<DeserializeClass>();
                    if (TrainingData != "")
                    {
                        var NewTrainingData = TrainingData.Replace("]", "]!");
                        string[] singleTRdata = NewTrainingData.Split('!');


                        for (var i = 0; i < singleTRdata.Count() - 1; i++)
                        {

                            List<DeserializeClass> a = serialize.Deserialize<List<DeserializeClass>>(singleTRdata[i]);
                            obj.AddRange(a);
                        }

                    }
                    if (TrainingData == "")
                    {
                        return Json(new { sucess = true, responseText = "You have to Modify record to update." }, JsonRequestBehavior.AllowGet);
                    }

                    List<string> CatNAME = obj.Where(e => e.RatingPoints != "" && e.Comments != "").Select(e => e.CatName).ToList();

                    // ============================== Data Exists Code Start =================================================
                    //var dataExistchk = db.EmpAppEvaluation.Include(e => e.AppraisalPeriodCalendar).Include(e => e.EmployeeAppraisal)
                    //                     .Include(e => e.EmpAppRatingConclusion)
                    //                     .Include(e => e.EmpAppRatingConclusion.Select(a => a.EmpAppRating))
                    //                     .Include(e => e.EmpAppRatingConclusion.Select(a => a.EmpAppRating.Select(r => r.AppAssignment)))
                    //                     .Include(e => e.EmpAppRatingConclusion.Select(a => a.EmpAppRating.Select(r => r.AppAssignment.AppCategory)))
                    //                     .Include(e => e.EmpAppRatingConclusion.Select(a => a.AppraisalAssistance))

                    //                     .Where(e => e.AppraisalPeriodCalendar.Id == AppraisalCalendarr.Id).ToList();


                    //foreach (var itemCal in dataExistchk)
                    //{
                    //    if (itemCal.EmployeeAppraisal != null && itemCal.EmployeeAppraisal.Count() > 0)
                    //    {
                    //        foreach (var item in itemCal.EmployeeAppraisal.Where(e => e.Id == iid))
                    //        {
                    //            var empAppIds = ids1.Contains(item.Id);
                    //            var appAssistval = item.EmpAppEvaluation.Where(e => e.EmpAppRatingConclusion.Count() > 0).Select(s => s.EmpAppRatingConclusion).ToList();


                    //            // var lookupval = (Object)null;
                    //            if (appAssistval.Count() > 0)
                    //            {
                    //                foreach (var lKitem in appAssistval)
                    //                {
                    //                    var getCategory = lKitem.SelectMany(c => c.EmpAppRating).FirstOrDefault().AppAssignment.AppCategory.Id;
                    //                    int getCatid = Convert.ToInt32(getCategory);
                    //                    //lookupval = lKitem.Select(r => r.AppraisalAssistance).FirstOrDefault().LookupVal;
                    //                    foreach (var itemCC in CatNAME)
                    //                    {
                    //                        if (lKitem.Count() > 1)
                    //                        {
                    //                            foreach (var Multi in lKitem)
                    //                            {
                    //                                var lookupval = Multi.EmpAppRating.Select(r => new
                    //                                {
                    //                                    Emprate = r.AppAssignment.AppCategory.Name,

                    //                                    Appassist = Multi.AppraisalAssistance

                    //                                }).FirstOrDefault();

                    //                                if (lookupval != null && lookupval.Appassist.LookupVal.ToUpper() == "APPRAISEE" && lookupval.Emprate.Count() > 0)
                    //                                {
                    //                                    var getc = lookupval.Emprate.ToString().Replace("\r\n", "").Trim();

                    //                                    if (getc == itemCC)
                    //                                    {
                    //                                        return Json(new Utility.JsonClass { status = false, responseText = " Appraisal for this employee, appcategory : " + getc + " already Exist. " }, JsonRequestBehavior.AllowGet);
                    //                                    }


                    //                                }
                    //                            }

                    //                        }
                    //                        else
                    //                        {
                    //                            var lookupval = lKitem.Select(r => new
                    //                            {
                    //                                Emprate = r.EmpAppRating.Select(d => new
                    //                                {
                    //                                    appcategory = d.AppAssignment.AppCategory.Name

                    //                                }).ToList(),

                    //                                Appassist = r.AppraisalAssistance

                    //                            }).FirstOrDefault();

                    //                            if (lookupval != null && lookupval.Appassist.LookupVal.ToUpper() == "APPRAISEE" && lookupval.Emprate.Count() > 0)
                    //                            {
                    //                                var getc = lookupval.Emprate.FirstOrDefault().appcategory.ToString().Replace("\r\n", "").Trim();

                    //                                if (getc == itemCC)
                    //                                {
                    //                                    return Json(new Utility.JsonClass { status = false, responseText = " Appraisal for this employee, appcategory : " + getc + " already Exist. " }, JsonRequestBehavior.AllowGet);
                    //                                }


                    //                            }
                    //                        }

                    //                    }
                    //                }

                    //            }

                    //        }
                    //    }
                    //}
                    // ============================== Data Exists Code End =================================================


                    if (AssistanceOverallCo != null)
                    {
                        p.AssistanceOverallComments = AssistanceOverallCo.ToString();
                    }


                    // var obj = serialize.Deserialize<List<DeserializeClass>>(TrainingData);

                    //if (obj == null || obj.Count < 0)
                    //{
                    //    //   return Json(new { sucess = true, responseText = "You have to change record to update." }, JsonRequestBehavior.AllowGet);
                    //    return Json(new Object[] { "", "", "You have to change record to update" }, JsonRequestBehavior.AllowGet);

                    //}

                    //foreach (var item in obj)
                    //{
                    //    item.RatingPoints
                    //}
                    List<int> ids = obj.Where(e => e.RatingPoints != "" && e.Comments != "").Select(e => int.Parse(e.Id)).ToList();
                    //List<int> idss = obj.Select(e => int.Parse(e.Id)).ToList();
                    List<int> idss2 = obj.Where(e => e.RatingPoints != "" && e.Comments != "").Select(e => int.Parse(e.EmpAppEvaluationId)).ToList();
                    var evalids = Convert.ToInt32(idss2[0]);
                    // var iidd = Convert.ToInt32(idss[0]);


                    var q1 = db.Employee.Where(q => q.Id == (iid)).SingleOrDefault();

                    //if (AppAssistance != null && AppAssistance != "")
                    //{
                    //LookupValue Lkval = db.LookupValue.Find(Convert.ToInt32(AppAssistance));
                    //p.AppraisalAssistance = Lkval;

                    LookupValue val = db.LookupValue.Where(e => e.LookupVal.ToString().ToUpper() == "APPRAISEE").SingleOrDefault();
                    if (val != null)
                    {
                        p.AppraisalAssistance = val;
                    }
                    else
                    {
                        return Json(new Object[] { "", "", "Kindly create lookupvalue : APPRAISEE " }, JsonRequestBehavior.AllowGet);
                    }

                    //  }

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
                            foreach (int ca in ids)
                            {
                                AppAssignment AppA = db.AppAssignment.Include(e => e.AppRatingObjective).Include(a => a.AppRatingObjective.Select(b => b.ObjectiveWordings)).Where(e => e.Id == ca).SingleOrDefault();
                                var getMaxPNT = AppA.MaxRatingPoints;
                                RatingPoints = Convert.ToInt32(obj.Where(e => e.Id == ca.ToString()).Select(e => e.RatingPoints).Single());
                                if (RatingPoints > getMaxPNT)
                                {
                                    return Json(new { status = false, responseText = "RatingPoints should not be greater than MaxPoints...!" }, JsonRequestBehavior.AllowGet);
                                }

                                var AppRatingObj = AppA.AppRatingObjective.OrderBy(e => e.RatingPoints).ToList();

                                LookupValue wording = null;
                                foreach (var a in AppRatingObj)
                                {
                                    if (RatingPoints == a.RatingPoints)
                                    {
                                        wording = a.ObjectiveWordings;
                                    }
                                }

                                // ================================== Edit Code Start ====================================

                                EmployeeAppraisal GetEmployeeAppraisal = db.EmployeeAppraisal.Include(e => e.Employee)
                                .Include(e => e.EmpAppEvaluation)
                                .Include(e => e.EmpAppEvaluation.Select(q => q.AppraisalPeriodCalendar))
                                .Include(e => e.EmpAppEvaluation.Select(q => q.EmpAppRatingConclusion))
                                .Include(e => e.EmpAppEvaluation.Select(q => q.EmpAppRatingConclusion.Select(z => z.EmpAppRating)))
                                .Include(e => e.EmpAppEvaluation.Select(q => q.EmpAppRatingConclusion.Select(z => z.EmpAppRating.Select(a => a.AppAssignment))))
                                .Include(e => e.EmpAppEvaluation.Select(q => q.EmpAppRatingConclusion.Select(z => z.EmpAppRating.Select(a => a.AppAssignment.AppCategory))))
                                .Include(e => e.EmpAppEvaluation.Select(q => q.EmpAppRatingConclusion.Select(z => z.EmpAppRating.Select(a => a.AppAssignment.AppSubCategory))))
                                .Include(e => e.EmpAppEvaluation.Select(q => q.EmpAppRatingConclusion.Select(z => z.AppraisalAssistance)))

                                .Where(e => e.Employee.Id == iid).FirstOrDefault();

                                var EmpAppEvaluationList = GetEmployeeAppraisal.EmpAppEvaluation.Where(e => e.AppraisalPeriodCalendar.Id == AppraisalCalendarr.Id).ToList();

                                foreach (var singleitemEval in EmpAppEvaluationList.Where(e => e.Id == evalids))
                                {
                                    var EmpAppRatingConclusList = singleitemEval.EmpAppRatingConclusion.Where(e => e.AppraisalAssistance.LookupVal.ToUpper() == "APPRAISEE".ToUpper()).ToList();

                                    var temp = EmpAppRatingConclusList.Select(z => new { aa = z.EmpAppRating.Select(d => d.AppAssignment) }).ToList();
                                    var EmpappAppAssign = temp.SelectMany(a => a.aa.Where(e => e.Id == ca)).FirstOrDefault();

                                    if (singleitemEval.EmpAppRatingConclusion.Count() > 0 && EmpappAppAssign != null)
                                    {
                                        foreach (var itemEmpAppRConclu in EmpAppRatingConclusList)
                                        {
                                            var ListEmpAppRating = itemEmpAppRConclu.EmpAppRating.ToList();

                                            foreach (var itemAppRate in ListEmpAppRating.Where(e => e.AppAssignment.Id == ca))
                                            {
                                                var DataExistsEmpAppRating = db.EmpAppRating.Include(e => e.AppAssignment).Include(e => e.ObjectiveWordings)
                                                    .Where(e => e.Id == itemAppRate.Id).FirstOrDefault();

                                                DataExistsEmpAppRating.Id = itemAppRate.Id;
                                                DataExistsEmpAppRating.ObjectiveWordings = wording;
                                                DataExistsEmpAppRating.RatingPoints = RatingPoints;
                                                DataExistsEmpAppRating.Comments = obj.Where(e => e.Id == ca.ToString()).Select(e => e.Comments).Single();

                                                DataExistsEmpAppRating.DBTrack = new DBTrack
                                                {
                                                    ModifiedBy = SessionManager.UserName,
                                                    ModifiedOn = DateTime.Now,
                                                    Action = "M",
                                                };

                                                db.EmpAppRating.Attach(DataExistsEmpAppRating);
                                                db.Entry(DataExistsEmpAppRating).State = System.Data.Entity.EntityState.Modified;
                                                db.SaveChanges();

                                            }
                                        }
                                    }
                                    else
                                    {
                                        EmpAppRating EmpAppR = new EmpAppRating()
                                        {
                                            AppAssignment = AppA,
                                            Comments = obj.Where(e => e.Id == ca.ToString()).Select(e => e.Comments).Single(),
                                            ObjectiveWordings = wording,
                                            RatingPoints = RatingPoints,
                                            DBTrack = p.DBTrack,
                                        };
                                        AppRatingList.Add(EmpAppR);
                                        var LastRecord = AppRatingList.ToList().LastOrDefault();

                                        List<EmpAppRating> LastRating = new List<EmpAppRating>();
                                        LastRating.Add(LastRecord);

                                        EmpAppRatingConclusion ObjEmpAppRatingCon = new EmpAppRatingConclusion()
                                        {
                                            //AppraisalPeriodCalendar = p.AppraisalPeriodCalendar,
                                            AppraisalAssistance = p.AppraisalAssistance,
                                            DBTrack = p.DBTrack,
                                            AssistanceOverallComments = p.AssistanceOverallComments,
                                            EmpAppRating = LastRating,
                                            IsTrClose = true
                                        };

                                        db.EmpAppRatingConclusion.Add(ObjEmpAppRatingCon);
                                        db.SaveChanges();

                                        List<EmpAppRatingConclusion> EmpAppRatingConcl = new List<EmpAppRatingConclusion>();
                                        EmpAppRatingConcl.Add(db.EmpAppRatingConclusion.Find(ObjEmpAppRatingCon.Id));

                                        //var eavn = db.EmpAppEvaluation.Include(a => a.AppraisalPeriodCalendar).Include(a => a.EmpAppRatingConclusion).Where(a => a.Id == evalids).SingleOrDefault();
                                        List<EmpAppEvaluation> OEmpEvalList = new List<EmpAppEvaluation>();
                                        var EmpAppraisal_val = db.EmployeeAppraisal.Include(e => e.EmpAppEvaluation).Include(e => e.EmpAppEvaluation.Select(s => s.AppraisalPeriodCalendar)).Where(e => e.Id == iid).FirstOrDefault();
                                        var EmpappEval_val = EmpAppraisal_val.EmpAppEvaluation.Where(e => e.AppraisalPeriodCalendar.Id == AppraisalCalendarr.Id).ToList();

                                        List<EmpAppRatingConclusion> EmpAppRatingConclListRange = new List<EmpAppRatingConclusion>();

                                        foreach (var itemEAE in EmpappEval_val)
                                        {
                                            if (itemEAE != null)
                                            {
                                                foreach (var itemEARC in itemEAE.EmpAppRatingConclusion)
                                                {
                                                    if (itemEARC != null)
                                                    {
                                                        EmpAppRatingConclListRange.Add(itemEARC);
                                                    }

                                                }
                                                EmpAppRatingConclListRange.AddRange(EmpAppRatingConcl);

                                            }

                                            itemEAE.Id = evalids;
                                            itemEAE.EmpAppRatingConclusion = EmpAppRatingConclListRange;
                                            itemEAE.DBTrack = p.DBTrack;
                                            db.EmpAppEvaluation.Attach(itemEAE);
                                            db.Entry(itemEAE).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();

                                            if (itemEAE != null)
                                            {
                                                OEmpEvalList.Add(itemEAE);
                                            }

                                        }
                                        


                                        if (OEmpEvalList.Count() > 0)
                                        {

                                            var empappr = db.EmployeeAppraisal.Where(a => a.Employee.Id == q1.Id).SingleOrDefault();
                                            if (db.EmployeeAppraisal.Any(o => o.Employee.Id == q1.Id))
                                            {

                                                empappr.EmpAppEvaluation = OEmpEvalList;
                                                empappr.DBTrack = p.DBTrack;
                                                db.EmployeeAppraisal.Attach(empappr);
                                                db.Entry(empappr).State = System.Data.Entity.EntityState.Modified;
                                                //    db.SaveChanges();

                                                //Msg.Add("Code Already Exists.");
                                                //return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                            }
                                            //if (eavn.EmpAppRatingConclusion != null)
                                            //{
                                            //    EmpAppRatingConcl.AddRange(eavn.EmpAppRatingConclusion);
                                            //}


                                            //eavn.SecurePoints = RatingPoints;
                                            //eavn.DBTrack = p.DBTrack;
                                            //eavn.EmpAppRatingConclusion = EmpAppRatingConcl;
                                            //db.EmpAppEvaluation.Attach(eavn);
                                            //db.Entry(eavn).State = System.Data.Entity.EntityState.Modified;
                                            //db.SaveChanges();
                                        }



                                        db.SaveChanges();
                                        //ts.Complete();

                                        // return Json(new { status = true, responseText = "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);

                                    }
                                }




                            }





                            // ================================== Edit Code End ====================================

                            //    EmpAppRating EmpAppR = new EmpAppRating()
                            //    {
                            //        AppAssignment = AppA,
                            //        Comments = obj.Where(e => e.Id == ca.ToString()).Select(e => e.Comments).Single(),
                            //        ObjectiveWordings = wording,
                            //        RatingPoints = RatingPoints,
                            //        DBTrack = p.DBTrack,
                            //    };
                            //    AppRatingList.Add(EmpAppR);
                            //}

                            //EmpAppRatingConclusion ObjEmpAppRatingCon = new EmpAppRatingConclusion()
                            //{
                            //    //AppraisalPeriodCalendar = p.AppraisalPeriodCalendar,
                            //    AppraisalAssistance = p.AppraisalAssistance,
                            //    DBTrack = p.DBTrack,
                            //    AssistanceOverallComments = p.AssistanceOverallComments,
                            //    EmpAppRating = AppRatingList,
                            //    IsTrClose = true
                            //};

                            //db.EmpAppRatingConclusion.Add(ObjEmpAppRatingCon);
                            //db.SaveChanges();

                            //List<EmpAppRatingConclusion> EmpAppRatingConcl = new List<EmpAppRatingConclusion>();

                            //EmpAppRatingConcl.Add(db.EmpAppRatingConclusion.Find(ObjEmpAppRatingCon.Id));

                            //var eavn = db.EmpAppEvaluation.Include(a => a.AppraisalPeriodCalendar).Include(a => a.EmpAppRatingConclusion).Where(a => a.Id == evalids).SingleOrDefault();



                            //if (eavn != null)
                            //{
                            //    if (eavn.EmpAppRatingConclusion != null)
                            //    {
                            //        EmpAppRatingConcl.AddRange(eavn.EmpAppRatingConclusion);
                            //    }

                            //    eavn.SecurePoints = RatingPoints;
                            //    eavn.DBTrack = p.DBTrack;
                            //    eavn.EmpAppRatingConclusion = EmpAppRatingConcl;
                            //    db.EmpAppEvaluation.Attach(eavn);
                            //    db.Entry(eavn).State = System.Data.Entity.EntityState.Modified;
                            //    db.SaveChanges();
                            //}



                            //db.SaveChanges();
                            //ts.Complete();

                            //return Json(new { status = true, responseText = "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
                            ts.Complete();
                            return Json(new { status = true, responseText = "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
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
            public string SRNo { get; set; }
            public string CatName { get; set; }
            public string SubCatName { get; set; }
            public int MaxPoint { get; set; }
            public int RatingPoints { get; set; }
            public string Comments { get; set; }
            public string AppConclusionId { get; set; }
        }
        public ActionResult CreateRoaster(List<RoasterClass> data, List<Int32> EmpId, string SRno, string cmnt, bool overide = false)
        {
            List<string> MSG = new List<string>();
            try
            {
                using (DataBaseContext db = new DataBaseContext())
                {

                    //List<returnObjClass> returnObjList = new List<returnObjClass>();


                    string AccessRight = "";
                    if (!string.IsNullOrEmpty(Convert.ToString(Session["user-module"])))
                    {
                        AccessRight = Convert.ToString(Session["auho"]);
                    }
                    // var EmpIds = UserManager.GeEmployeeList(FuncModule.Id, 0);
                    //List<EssPortal.Process.PortalRights.AccessRightsData> EmpidsWithfunsub = UserManager.GeEmployeeListWithfunsub(FuncModule.Id, 0, AccessRight);
                    //if (EmpidsWithfunsub == null && EmpidsWithfunsub.Count == 0)
                    //{
                    //    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                    //}

                    var serialize = new JavaScriptSerializer();
                    List<DeserializeClass> obj = new List<DeserializeClass>();
                    if (cmnt != "")
                    {
                        var NewAppraisalData = cmnt.Replace("]", "]!");
                        string[] singleAPPdata = NewAppraisalData.Split('!');


                        for (var i = 0; i < singleAPPdata.Count() - 1; i++)
                        {

                            List<DeserializeClass> a = serialize.Deserialize<List<DeserializeClass>>(singleAPPdata[i]);
                            obj.AddRange(a);
                        }

                    }

                    List<string> AppcatLIST = new List<string>();
                    if (data.Count() == 0)
                    {
                        foreach (var itemAppraisal in obj)
                        {
                            data.Add(new RoasterClass
                            {
                                SRNo = itemAppraisal.SRNo,
                                Id = Convert.ToInt32(itemAppraisal.Id),
                                CatName = itemAppraisal.CatName,
                                SubCatName = itemAppraisal.SubCatName,
                                MaxPoint = Convert.ToInt32(itemAppraisal.MaxPoint),
                                RatingPoints = Convert.ToInt32(itemAppraisal.RatingPoints),
                                Comments = itemAppraisal.Comments
                            });
                            AppcatLIST.Add(itemAppraisal.CatName);
                        }

                    }
                    //List<RoasterClass> data


                    EmpAppRatingConclusion p = new EmpAppRatingConclusion();

                    LookupValue val = null;
                    if (AccessRight == "Sanction")
                    {
                        val = db.LookupValue.Where(e => e.LookupVal.ToString().ToUpper() == "AppraiserSanction".ToUpper()).SingleOrDefault();
                    }
                    else
                    {
                        val = db.LookupValue.Where(e => e.LookupVal.ToString().ToUpper() == "HR".ToUpper()).SingleOrDefault();
                    }

                    if (val != null)
                    {
                        p.AppraisalAssistance = val;
                    }
                    else
                    {
                        return Json(new Object[] { "", "", "Kindly create lookupvalue " }, JsonRequestBehavior.AllowGet);
                    }




                    var AppraisalCalendarr = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "AppraisalCalendar".ToUpper() && e.Default == true).SingleOrDefault();
                    AppraisalSchedule AppSchedule = null;
                    //var dataExistchk = db.EmpAppEvaluation.Include(e => e.AppraisalPeriodCalendar).Include(e => e.AppraisalSchedule)
                    //                         .Include(e => e.EmployeeAppraisal)
                    //                         .Include(e => e.EmpAppRatingConclusion)
                    //                         .Include(e => e.EmpAppRatingConclusion.Select(r => r.AppraisalAssistance))
                    //                         .Include(e => e.EmpAppRatingConclusion.Select(a => a.EmpAppRating))
                    //                         .Include(e => e.EmpAppRatingConclusion.Select(a => a.EmpAppRating.Select(s => s.AppAssignment)))
                    //                         .Include(e => e.EmpAppRatingConclusion.Select(a => a.EmpAppRating.Select(s => s.AppAssignment.AppCategory)))
                    //                         .Where(e => e.AppraisalPeriodCalendar.Id == AppraisalCalendarr.Id).ToList();
                    //
                    //foreach (var itemchk in dataExistchk)
                    //{
                    //    if (itemchk.EmployeeAppraisal != null && itemchk.EmployeeAppraisal.Count() > 0)
                    //    {
                    //        foreach (var item in itemchk.EmployeeAppraisal)
                    //        {
                    //            var empAppIds = EmpId.Contains(item.Id);


                    //            foreach (var itemCon in itemchk.EmpAppRatingConclusion)
                    //            {
                    //                var AppCAt = itemCon.EmpAppRating.Select(r => new
                    //                {
                    //                    Cat = r.AppAssignment.AppCategory.Name,
                    //                }).FirstOrDefault();
                    //                var getAppcatName = AppCAt.Cat.Replace("\r\n", "");

                    //                if ((empAppIds == true) && (itemCon.AppraisalAssistance.Id == val.Id) && AppcatLIST.Contains(getAppcatName))
                    //                {
                    //                    MSG.Add(" AppraiserSanction for this employee already done. ");
                    //                    return Json(new Utility.JsonReturnClass { success = false, responseText = MSG }, JsonRequestBehavior.AllowGet);
                    //                    //return Json(new { status = false, responseText = ShowMessage }, JsonRequestBehavior.AllowGet);
                    //                    //return RedirectToAction("Index", "Appraisal");
                    //                }
                    //            }
                    //        }
                    //    }

                    //}

                    var empAPP = db.EmployeeAppraisal.Include(e => e.EmpAppEvaluation).Include(e => e.EmpAppEvaluation.Select(a => a.AppraisalSchedule))
                                                     .Where(e => EmpId.Contains(e.Id)).FirstOrDefault().EmpAppEvaluation;
                    if (empAPP != null)
                    {
                        foreach (var itema in empAPP)
                        {
                            AppSchedule = itema.AppraisalSchedule;
                        }
                    }


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
                                RatingPoints = item.RatingPoints;
                                maxpoint = item.MaxPoint;

                                if (RatingPoints > maxpoint)
                                {
                                    return Json(new { status = false, responseText = "RatingPoints should not be greater than MaxPoints...!" }, JsonRequestBehavior.AllowGet);
                                }
                                var AppRatingObj = AppA.AppRatingObjective.OrderBy(e => e.RatingPoints).ToList();

                                LookupValue wording = null;
                                foreach (var a in AppRatingObj)
                                {
                                    if (RatingPoints == a.RatingPoints)
                                    {
                                        wording = a.ObjectiveWordings;
                                    }
                                }


                                // ================================== Edit Code Start ====================================

                                EmployeeAppraisal GetEmployeeAppraisal = db.EmployeeAppraisal.Include(e => e.Employee)
                                .Include(e => e.EmpAppEvaluation)
                                .Include(e => e.EmpAppEvaluation.Select(q => q.AppraisalPeriodCalendar))
                                .Include(e => e.EmpAppEvaluation.Select(q => q.EmpAppRatingConclusion))
                                .Include(e => e.EmpAppEvaluation.Select(q => q.EmpAppRatingConclusion.Select(z => z.EmpAppRating)))
                                .Include(e => e.EmpAppEvaluation.Select(q => q.EmpAppRatingConclusion.Select(z => z.EmpAppRating.Select(a => a.AppAssignment))))
                                .Include(e => e.EmpAppEvaluation.Select(q => q.EmpAppRatingConclusion.Select(z => z.EmpAppRating.Select(a => a.AppAssignment.AppCategory))))
                                .Include(e => e.EmpAppEvaluation.Select(q => q.EmpAppRatingConclusion.Select(z => z.EmpAppRating.Select(a => a.AppAssignment.AppSubCategory))))
                                .Include(e => e.EmpAppEvaluation.Select(q => q.EmpAppRatingConclusion.Select(z => z.AppraisalAssistance)))

                                .Where(e => e.Employee.Id == Id).FirstOrDefault();

                                var EmpAppEvaluationList = GetEmployeeAppraisal.EmpAppEvaluation.Where(e => e.AppraisalPeriodCalendar.Id == AppraisalCalendarr.Id).ToList();

                                foreach (var singleitemEval in EmpAppEvaluationList)
                                {
                                    if (AccessRight == "Sanction")
                                    {
                                        //var EmpAppRatingAppraisee = singleitemEval.EmpAppRatingConclusion.Where(e => e.AppraisalAssistance.LookupVal.ToUpper() == "APPRAISEE".ToUpper()).ToList();
                                        //if (EmpAppRatingAppraisee.Count() > 0)
                                        //{
                                        //    continue;
                                        //}
                                        var EmpAppRatingConclusList = singleitemEval.EmpAppRatingConclusion.Where(e => e.AppraisalAssistance.LookupVal.ToUpper() == "AppraiserSanction".ToUpper()).ToList();

                                        var temp = EmpAppRatingConclusList.Select(z => new { aa = z.EmpAppRating.Select(d => d.AppAssignment) }).ToList();
                                        var EmpappAppAssign = temp.SelectMany(a => a.aa.Where(e => e.Id == item.Id)).FirstOrDefault();

                                        if (EmpAppRatingConclusList.Count() > 0 && EmpappAppAssign != null)
                                        {

                                            foreach (var itemEmpAppRConclu in EmpAppRatingConclusList)
                                            {
                                                var ListEmpAppRating = itemEmpAppRConclu.EmpAppRating.ToList();

                                                foreach (var itemAppRate in ListEmpAppRating.Where(e => e.AppAssignment.Id == item.Id))
                                                {
                                                    var DataExistsEmpAppRating = db.EmpAppRating.Include(e => e.AppAssignment).Include(e => e.ObjectiveWordings)
                                                        .Where(e => e.Id == itemAppRate.Id).FirstOrDefault();

                                                    DataExistsEmpAppRating.Id = itemAppRate.Id;
                                                    DataExistsEmpAppRating.ObjectiveWordings = wording;
                                                    DataExistsEmpAppRating.RatingPoints = RatingPoints;
                                                    DataExistsEmpAppRating.Comments = item.Comments;

                                                    DataExistsEmpAppRating.DBTrack = new DBTrack
                                                    {
                                                        ModifiedBy = SessionManager.UserName,
                                                        ModifiedOn = DateTime.Now,
                                                        Action = "M",
                                                    };

                                                    db.EmpAppRating.Attach(DataExistsEmpAppRating);
                                                    db.Entry(DataExistsEmpAppRating).State = System.Data.Entity.EntityState.Modified;
                                                    db.SaveChanges();

                                                }
                                            }
                                        }
                                        else
                                        {
                                            EmpAppRating EmpAppR = new EmpAppRating()
                                            {
                                                AppAssignment = AppA,
                                                Comments = item.Comments,
                                                ObjectiveWordings = wording,
                                                RatingPoints = RatingPoints,
                                                DBTrack = p.DBTrack,
                                            };
                                            AppRatingList.Add(EmpAppR);

                                            var LastRecord = AppRatingList.ToList().LastOrDefault();

                                            List<EmpAppRating> LastRating = new List<EmpAppRating>();
                                            LastRating.Add(LastRecord);
                                            EmpAppRatingConclusion Appcategory = new EmpAppRatingConclusion()
                                            {
                                                //AppraisalPeriodCalendar = p.AppraisalPeriodCalendar,
                                                AppraisalAssistance = p.AppraisalAssistance,
                                                DBTrack = p.DBTrack,
                                                AssistanceOverallComments = p.AssistanceOverallComments,
                                                EmpAppRating = LastRating,
                                                IsTrClose = true
                                            };

                                            db.EmpAppRatingConclusion.Add(Appcategory);
                                            //var rtn_Obj = DBTrackFile.DBTrackSave("Appraisal/Appraisal", null, db.ChangeTracker, p.DBTrack);
                                            db.SaveChanges();

                                            List<EmpAppRatingConclusion> EmpAppRatingConcl = new List<EmpAppRatingConclusion>();
                                            EmpAppRatingConcl.Add(Appcategory);

                                            //#region New Record of EmpAppEvaluation on Saction 
                                            //EmpAppEvaluation evp = new EmpAppEvaluation()
                                            //{
                                            //    EmpAppRatingConclusion = EmpAppRatingConcl,
                                            //    SecurePoints = RatingPoints,
                                            //    AppraisalPeriodCalendar = AppraisalCalendarr,
                                            //    AppraisalSchedule = AppSchedule,
                                            //    MaxPoints = maxpoint,
                                            //    DBTrack = p.DBTrack,
                                            //    IsTrClose = true
                                            //};
                                            //db.EmpAppEvaluation.Add(evp);
                                            //db.SaveChanges();
                                            //#endregion


                                            //      var EmployeeAppraisal = new EmployeeAppraisal();
                                            List<EmpAppEvaluation> OEmpEvalList = new List<EmpAppEvaluation>();
                                            if (EmpId != null)
                                            {
                                                //var ids = Utility.StringIdsToListIds(EmpId);
                                                foreach (var ca in EmpId)
                                                {
                                                    var EmpAppraisal_val = db.EmployeeAppraisal.Include(e => e.EmpAppEvaluation).Where(e => e.Id == ca).FirstOrDefault();
                                                    var EmpappEval_val = EmpAppraisal_val.EmpAppEvaluation.ToList();

                                                    List<EmpAppRatingConclusion> EmpAppRatingConclListRange = new List<EmpAppRatingConclusion>();

                                                    foreach (var itemEAE in EmpappEval_val)
                                                    {
                                                        if (itemEAE != null)
                                                        {
                                                            foreach (var itemEARC in itemEAE.EmpAppRatingConclusion)
                                                            {
                                                                if (itemEARC != null)
                                                                {
                                                                    EmpAppRatingConclListRange.Add(itemEARC);
                                                                }

                                                            }
                                                            EmpAppRatingConclListRange.AddRange(EmpAppRatingConcl);

                                                        }

                                                        itemEAE.Id = itemEAE.Id;
                                                        itemEAE.EmpAppRatingConclusion = EmpAppRatingConclListRange;
                                                        itemEAE.DBTrack = p.DBTrack;
                                                        db.EmpAppEvaluation.Attach(itemEAE);
                                                        db.Entry(itemEAE).State = System.Data.Entity.EntityState.Modified;
                                                        db.SaveChanges();

                                                        if (itemEAE != null)
                                                        {
                                                            OEmpEvalList.Add(itemEAE);
                                                        }

                                                    }

                                                }
                                            }

                                            //List<EmpAppEvaluation> AppcategoryLost = new List<EmpAppEvaluation>();
                                            //AppcategoryLost.Add(evp);

                                            //OEmpEvalList.AddRange(AppcategoryLost);



                                            var empappr = db.EmployeeAppraisal.Where(a => a.Employee.Id == q1.Id).SingleOrDefault();
                                            if (db.EmployeeAppraisal.Any(o => o.Employee.Id == q1.Id))
                                            {

                                                empappr.EmpAppEvaluation = OEmpEvalList;
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
                                                empappr.EmpAppEvaluation = OEmpEvalList;
                                                db.EmployeeAppraisal.Add(empappr);
                                            }
                                        }
                                    }
                                    if (AccessRight == "Approval")
                                    {
                                        //var EmpAppRatingAppraisee = singleitemEval.EmpAppRatingConclusion.Where(e => e.AppraisalAssistance.LookupVal.ToUpper() == "AppraiserSanction".ToUpper()).ToList();
                                        //if (EmpAppRatingAppraisee.Count() > 0)
                                        //{
                                        //    continue;
                                        //}
                                        var EmpAppRatingConclusList = singleitemEval.EmpAppRatingConclusion.Where(e => e.AppraisalAssistance.LookupVal.ToUpper() == "HR".ToUpper()).ToList();
                                        if (EmpAppRatingConclusList.Count() > 0)
                                        {

                                            foreach (var itemEmpAppRConclu in EmpAppRatingConclusList)
                                            {
                                                var ListEmpAppRating = itemEmpAppRConclu.EmpAppRating.ToList();

                                                foreach (var itemAppRate in ListEmpAppRating.Where(e => e.AppAssignment.Id == item.Id))
                                                {
                                                    var DataExistsEmpAppRating = db.EmpAppRating.Include(e => e.AppAssignment).Include(e => e.ObjectiveWordings)
                                                        .Where(e => e.Id == itemAppRate.Id).FirstOrDefault();

                                                    DataExistsEmpAppRating.Id = itemAppRate.Id;
                                                    DataExistsEmpAppRating.ObjectiveWordings = wording;
                                                    DataExistsEmpAppRating.RatingPoints = RatingPoints;
                                                    DataExistsEmpAppRating.Comments = item.Comments;

                                                    DataExistsEmpAppRating.DBTrack = new DBTrack
                                                    {
                                                        ModifiedBy = SessionManager.UserName,
                                                        ModifiedOn = DateTime.Now,
                                                        Action = "M",
                                                    };

                                                    db.EmpAppRating.Attach(DataExistsEmpAppRating);
                                                    db.Entry(DataExistsEmpAppRating).State = System.Data.Entity.EntityState.Modified;
                                                    db.SaveChanges();

                                                }
                                            }
                                        }
                                        else
                                        {
                                            EmpAppRating EmpAppR = new EmpAppRating()
                                            {
                                                AppAssignment = AppA,
                                                Comments = item.Comments,
                                                ObjectiveWordings = wording,
                                                RatingPoints = RatingPoints,
                                                DBTrack = p.DBTrack,
                                            };
                                            AppRatingList.Add(EmpAppR);
                                            var LastRecord = AppRatingList.ToList().LastOrDefault();

                                            List<EmpAppRating> LastRating = new List<EmpAppRating>();
                                            LastRating.Add(LastRecord);
                                            EmpAppRatingConclusion Appcategory = new EmpAppRatingConclusion()
                                            {
                                                //AppraisalPeriodCalendar = p.AppraisalPeriodCalendar,
                                                AppraisalAssistance = p.AppraisalAssistance,
                                                DBTrack = p.DBTrack,
                                                AssistanceOverallComments = p.AssistanceOverallComments,
                                                EmpAppRating = LastRating,
                                                IsTrClose = true
                                            };

                                            db.EmpAppRatingConclusion.Add(Appcategory);
                                            //var rtn_Obj = DBTrackFile.DBTrackSave("Appraisal/Appraisal", null, db.ChangeTracker, p.DBTrack);
                                            db.SaveChanges();

                                            List<EmpAppRatingConclusion> EmpAppRatingConcl = new List<EmpAppRatingConclusion>();
                                            EmpAppRatingConcl.Add(Appcategory);

                                            //#region Create New Record of EmpAppEvaluation on Approval
                                            //EmpAppEvaluation evp = new EmpAppEvaluation()
                                            //{
                                            //    EmpAppRatingConclusion = EmpAppRatingConcl,
                                            //    SecurePoints = RatingPoints,
                                            //    AppraisalPeriodCalendar = AppraisalCalendarr,
                                            //    AppraisalSchedule = AppSchedule,
                                            //    MaxPoints = maxpoint,
                                            //    DBTrack = p.DBTrack,
                                            //    IsTrClose = true
                                            //};
                                            //db.EmpAppEvaluation.Add(evp);
                                            //db.SaveChanges();
                                            //#endregion


                                            List<EmpAppEvaluation> OEmpEvalList = new List<EmpAppEvaluation>();
                                            if (EmpId != null)
                                            {
                                                //var ids = Utility.StringIdsToListIds(EmpId);
                                                foreach (var ca in EmpId)
                                                {
                                                    var EmpAppraisal_val = db.EmployeeAppraisal.Include(e => e.EmpAppEvaluation).Where(e => e.Id == ca).FirstOrDefault();
                                                    var EmpappEval_val = EmpAppraisal_val.EmpAppEvaluation.ToList();

                                                    List<EmpAppRatingConclusion> EmpAppRatingConclListRange = new List<EmpAppRatingConclusion>();

                                                    foreach (var itemEAE in EmpappEval_val)
                                                    {
                                                        if (itemEAE != null)
                                                        {
                                                            foreach (var itemEARC in itemEAE.EmpAppRatingConclusion)
                                                            {
                                                                if (itemEARC != null)
                                                                {
                                                                    EmpAppRatingConclListRange.Add(itemEARC);
                                                                }

                                                            }
                                                            EmpAppRatingConclListRange.AddRange(EmpAppRatingConcl);

                                                        }

                                                        itemEAE.Id = itemEAE.Id;
                                                        itemEAE.EmpAppRatingConclusion = EmpAppRatingConclListRange;
                                                        itemEAE.DBTrack = p.DBTrack;
                                                        db.EmpAppEvaluation.Attach(itemEAE);
                                                        db.Entry(itemEAE).State = System.Data.Entity.EntityState.Modified;
                                                        db.SaveChanges();

                                                        if (itemEAE != null)
                                                        {
                                                            OEmpEvalList.Add(itemEAE);
                                                        }

                                                    }

                                                }
                                            }

                                            //List<EmpAppEvaluation> AppcategoryLost = new List<EmpAppEvaluation>();
                                            //AppcategoryLost.Add(evp);

                                            //OEmpEvalList.AddRange(AppcategoryLost);

                                            var empappr = db.EmployeeAppraisal.Where(a => a.Employee.Id == q1.Id).SingleOrDefault();
                                            if (db.EmployeeAppraisal.Any(o => o.Employee.Id == q1.Id))
                                            {

                                                empappr.EmpAppEvaluation = OEmpEvalList;
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
                                                empappr.EmpAppEvaluation = OEmpEvalList;
                                                db.EmployeeAppraisal.Add(empappr);
                                            }
                                        }
                                    }


                                    //var EmpAppRatingConclusList = singleitemEval.EmpAppRatingConclusion.Where(e => e.AppraisalAssistance.LookupVal.ToUpper() == "AppraiserSanction".ToUpper()).ToList();
                                    //if (EmpAppRatingConclusList.Count() > 0)
                                    //{

                                    //    foreach (var itemEmpAppRConclu in EmpAppRatingConclusList)
                                    //    {
                                    //        var ListEmpAppRating = itemEmpAppRConclu.EmpAppRating.ToList();

                                    //        foreach (var itemAppRate in ListEmpAppRating.Where(e => e.AppAssignment.Id == item.Id))
                                    //        {
                                    //            var DataExistsEmpAppRating = db.EmpAppRating.Include(e => e.AppAssignment).Include(e => e.ObjectiveWordings)
                                    //                .Where(e => e.Id == itemAppRate.Id).FirstOrDefault();

                                    //            DataExistsEmpAppRating.Id = itemAppRate.Id;
                                    //            DataExistsEmpAppRating.ObjectiveWordings = wording;
                                    //            DataExistsEmpAppRating.RatingPoints = RatingPoints;
                                    //            DataExistsEmpAppRating.Comments = item.Comments;

                                    //            DataExistsEmpAppRating.DBTrack = new DBTrack
                                    //            {
                                    //                ModifiedBy = SessionManager.UserName,
                                    //                ModifiedOn = DateTime.Now,
                                    //                Action = "M",
                                    //            };

                                    //            db.EmpAppRating.Attach(DataExistsEmpAppRating);
                                    //            db.Entry(DataExistsEmpAppRating).State = System.Data.Entity.EntityState.Modified;
                                    //            db.SaveChanges();

                                    //        }
                                    //    }
                                    //}
                                    //else
                                    //{
                                    //    EmpAppRating EmpAppR = new EmpAppRating()
                                    //    {
                                    //        AppAssignment = AppA,
                                    //        Comments = item.Comments,
                                    //        ObjectiveWordings = wording,
                                    //        RatingPoints = RatingPoints,
                                    //        DBTrack = p.DBTrack,
                                    //    };
                                    //    AppRatingList.Add(EmpAppR);

                                    //    EmpAppRatingConclusion Appcategory = new EmpAppRatingConclusion()
                                    //    {
                                    //        //AppraisalPeriodCalendar = p.AppraisalPeriodCalendar,
                                    //        AppraisalAssistance = p.AppraisalAssistance,
                                    //        DBTrack = p.DBTrack,
                                    //        AssistanceOverallComments = p.AssistanceOverallComments,
                                    //        EmpAppRating = AppRatingList,
                                    //        IsTrClose = true
                                    //    };

                                    //    db.EmpAppRatingConclusion.Add(Appcategory);
                                    //    //var rtn_Obj = DBTrackFile.DBTrackSave("Appraisal/Appraisal", null, db.ChangeTracker, p.DBTrack);
                                    //    //     db.SaveChanges();

                                    //    List<EmpAppRatingConclusion> EmpAppRatingConcl = new List<EmpAppRatingConclusion>();
                                    //    EmpAppRatingConcl.Add(Appcategory);


                                    //    EmpAppEvaluation evp = new EmpAppEvaluation()
                                    //    {
                                    //        EmpAppRatingConclusion = EmpAppRatingConcl,
                                    //        SecurePoints = RatingPoints,
                                    //        AppraisalPeriodCalendar = AppraisalCalendarr,
                                    //        AppraisalSchedule = AppSchedule,
                                    //        MaxPoints = maxpoint,
                                    //        DBTrack = p.DBTrack,
                                    //        IsTrClose = true
                                    //    };
                                    //    db.EmpAppEvaluation.Add(evp);


                                    //    db.SaveChanges();

                                    //    //      var EmployeeAppraisal = new EmployeeAppraisal();
                                    //    List<EmpAppEvaluation> OEmpEvalList = new List<EmpAppEvaluation>();
                                    //    if (EmpId != null)
                                    //    {
                                    //        //var ids = Utility.StringIdsToListIds(EmpId);
                                    //        foreach (var ca in EmpId)
                                    //        {
                                    //            var EmpAppraisal_val = db.EmployeeAppraisal.Include(e => e.EmpAppEvaluation).Where(e => e.Id == ca).FirstOrDefault();
                                    //            var EmpappEval_val = EmpAppraisal_val.EmpAppEvaluation.ToList();

                                    //            OEmpEvalList.AddRange(EmpappEval_val);

                                    //        }
                                    //    }

                                    //    List<EmpAppEvaluation> AppcategoryLost = new List<EmpAppEvaluation>();
                                    //    AppcategoryLost.Add(evp);

                                    //    OEmpEvalList.AddRange(AppcategoryLost);

                                    //    var empappr = db.EmployeeAppraisal.Where(a => a.Employee.Id == q1.Id).SingleOrDefault();
                                    //    if (db.EmployeeAppraisal.Any(o => o.Employee.Id == q1.Id))
                                    //    {

                                    //        empappr.EmpAppEvaluation = OEmpEvalList;
                                    //        empappr.DBTrack = p.DBTrack;
                                    //        db.EmployeeAppraisal.Attach(empappr);
                                    //        db.Entry(empappr).State = System.Data.Entity.EntityState.Modified;
                                    //        //    db.SaveChanges();

                                    //        //Msg.Add("Code Already Exists.");
                                    //        //return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //    }
                                    //    else
                                    //    {
                                    //        empappr.DBTrack = p.DBTrack;
                                    //        empappr.Employee = q1;
                                    //        empappr.EmpAppEvaluation = OEmpEvalList;
                                    //        db.EmployeeAppraisal.Add(empappr);
                                    //    }
                                    //}
                                }




                                //EmpAppRating EmpAppR = new EmpAppRating()
                                //{
                                //    AppAssignment = AppA,
                                //    Comments = item.Comments,
                                //    ObjectiveWordings = wording,
                                //    RatingPoints = RatingPoints,
                                //    DBTrack = p.DBTrack,
                                //};
                                //AppRatingList.Add(EmpAppR);
                            }
                            //EmpAppRatingConclusion Appcategory = new EmpAppRatingConclusion()
                            //{
                            //    //AppraisalPeriodCalendar = p.AppraisalPeriodCalendar,
                            //    AppraisalAssistance = p.AppraisalAssistance,
                            //    DBTrack = p.DBTrack,
                            //    AssistanceOverallComments = p.AssistanceOverallComments,
                            //    EmpAppRating = AppRatingList,
                            //    IsTrClose = true
                            //};

                            //db.EmpAppRatingConclusion.Add(Appcategory);
                            ////var rtn_Obj = DBTrackFile.DBTrackSave("Appraisal/Appraisal", null, db.ChangeTracker, p.DBTrack);
                            ////     db.SaveChanges();

                            //List<EmpAppRatingConclusion> EmpAppRatingConcl = new List<EmpAppRatingConclusion>();
                            //EmpAppRatingConcl.Add(Appcategory);


                            //EmpAppEvaluation evp = new EmpAppEvaluation()
                            //{
                            //    EmpAppRatingConclusion = EmpAppRatingConcl,
                            //    SecurePoints = RatingPoints,
                            //    AppraisalPeriodCalendar = AppraisalCalendarr,
                            //    AppraisalSchedule = AppSchedule,
                            //    MaxPoints = maxpoint,
                            //    DBTrack = p.DBTrack,
                            //    IsTrClose = true
                            //};
                            //db.EmpAppEvaluation.Add(evp);


                            //db.SaveChanges();

                            ////      var EmployeeAppraisal = new EmployeeAppraisal();
                            //List<EmpAppEvaluation> OEmpEvalList = new List<EmpAppEvaluation>();
                            //if (EmpId != null)
                            //{
                            //    //var ids = Utility.StringIdsToListIds(EmpId);
                            //    foreach (var ca in EmpId)
                            //    {
                            //        var EmpAppraisal_val = db.EmployeeAppraisal.Include(e => e.EmpAppEvaluation).Where(e => e.Id == ca).FirstOrDefault();
                            //        var EmpappEval_val = EmpAppraisal_val.EmpAppEvaluation.ToList();

                            //        OEmpEvalList.AddRange(EmpappEval_val);

                            //    }
                            //}



                            //List<EmpAppEvaluation> AppcategoryLost = new List<EmpAppEvaluation>();
                            //AppcategoryLost.Add(evp);

                            //OEmpEvalList.AddRange(AppcategoryLost);

                            //var empappr = db.EmployeeAppraisal.Where(a => a.Employee.Id == q1.Id).SingleOrDefault();
                            //if (db.EmployeeAppraisal.Any(o => o.Employee.Id == q1.Id))
                            //{

                            //    empappr.EmpAppEvaluation = OEmpEvalList;
                            //    empappr.DBTrack = p.DBTrack;
                            //    db.EmployeeAppraisal.Attach(empappr);
                            //    db.Entry(empappr).State = System.Data.Entity.EntityState.Modified;
                            //    //    db.SaveChanges();

                            //    //Msg.Add("Code Already Exists.");
                            //    //return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //}
                            //else
                            //{
                            //    empappr.DBTrack = p.DBTrack;
                            //    empappr.Employee = q1;
                            //    empappr.EmpAppEvaluation = OEmpEvalList;
                            //    db.EmployeeAppraisal.Add(empappr);
                            //}

                            //db.SaveChanges();

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

                //return View();
            }
            catch (Exception e)
            {
                MSG.Add(e.InnerException.Message.ToString());


            }
            return Json(MSG, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetEmpLvData(string data)  // Commented on 30/12/2023 : Currently Edit not required 
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var ids = Utility.StringIdsToListString(data);
                var id = Convert.ToInt32(ids[0]);
                var status = ids.Count > 0 ? ids[2] : null;
                var emplvId = ids.Count > 0 ? ids[1] : null;
                // var LvHeadId = ids.Count > 0 ? ids[3] : null;
                List<EmpmLVdata> EmpmLVdataClassList = new List<EmpmLVdata>();
                // var lvheadidint = Convert.ToInt32(LvHeadId);
                var EmpAppraisalIdint = Convert.ToInt32(emplvId);
                var AppraisalCalendarr = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "AppraisalCalendar".ToUpper() && e.Default == true).SingleOrDefault();
                EmployeeAppraisal GetEmployeeAppraisal = db.EmployeeAppraisal.Include(e => e.EmpAppEvaluation)
                                 .Include(e => e.EmpAppEvaluation.Select(q => q.AppraisalPeriodCalendar))
                                 .Include(e => e.EmpAppEvaluation.Select(q => q.EmpAppRatingConclusion))
                                 .Include(e => e.EmpAppEvaluation.Select(q => q.EmpAppRatingConclusion.Select(z => z.EmpAppRating)))
                                 .Include(e => e.EmpAppEvaluation.Select(q => q.EmpAppRatingConclusion.Select(z => z.EmpAppRating.Select(a => a.AppAssignment))))
                                 .Include(e => e.EmpAppEvaluation.Select(q => q.EmpAppRatingConclusion.Select(z => z.EmpAppRating.Select(a => a.AppAssignment.AppCategory))))
                                 .Include(e => e.EmpAppEvaluation.Select(q => q.EmpAppRatingConclusion.Select(z => z.EmpAppRating.Select(a => a.AppAssignment.AppSubCategory))))
                                 .Include(e => e.EmpAppEvaluation.Select(q => q.EmpAppRatingConclusion.Select(z => z.AppraisalAssistance)))

                                 .Where(e => e.Id == EmpAppraisalIdint).FirstOrDefault();

                string authority = Convert.ToString(Session["auho"]);
                var WfStatus = new List<Int32>();
                var SanctionStatus = new List<Int32>();
                var ApporvalStatus = new List<Int32>();
                if (authority.ToUpper() == "SANCTION")
                {
                    WfStatus.Add(1);
                    WfStatus.Add(2);


                    var EmpAppEvalList = GetEmployeeAppraisal.EmpAppEvaluation.Where(e => e.AppraisalPeriodCalendar.Id == AppraisalCalendarr.Id).ToList();

                    foreach (var itemEmpEval in EmpAppEvalList)
                    {
                        var EmpAppRConclusionList = itemEmpEval.EmpAppRatingConclusion.Where(e => e.AppraisalAssistance.LookupVal.ToUpper() == "AppraiserSanction".ToUpper()).ToList();
                        foreach (var singleEmpappConclusionitem in EmpAppRConclusionList)
                        {
                            var ListEmpAppRATING = singleEmpappConclusionitem.EmpAppRating.ToList();

                            foreach (var itemApprate in ListEmpAppRATING)
                            {
                                EmpmLVdata AppraisalSantiondata = new EmpmLVdata()
                                {
                                    CategoryName = itemApprate.AppAssignment.AppCategory.Name,
                                    SubcategoryName = itemApprate.AppAssignment.AppSubCategory.Name,
                                    Maxpoints = itemApprate.AppAssignment.MaxRatingPoints,
                                    RatingPoints = itemApprate.RatingPoints,
                                    EmpAppConcluId = singleEmpappConclusionitem.Id,
                                    Comments = itemApprate.Comments,
                                    EmpAppEvalID = itemEmpEval.Id,
                                };


                                EmpmLVdataClassList.Add(AppraisalSantiondata);

                            }

                        }

                    }



                }
                else if (authority.ToUpper() == "APPROVAL")
                {
                    WfStatus.Add(1);
                    WfStatus.Add(2);

                    var EmpAppEvalList = GetEmployeeAppraisal.EmpAppEvaluation.Where(e => e.AppraisalPeriodCalendar.Id == AppraisalCalendarr.Id).ToList();

                    foreach (var itemEmpEval in EmpAppEvalList)
                    {
                        var EmpAppRConclusionList = itemEmpEval.EmpAppRatingConclusion.Where(e => e.AppraisalAssistance.LookupVal.ToUpper() == "HR".ToUpper()).ToList();
                        foreach (var singleEmpappConclusionitem in EmpAppRConclusionList)
                        {
                            var ListEmpAppRATING = singleEmpappConclusionitem.EmpAppRating.ToList();

                            foreach (var itemApprate in ListEmpAppRATING)
                            {
                                EmpmLVdata AppraisalApprovaldata = new EmpmLVdata()
                                {
                                    CategoryName = itemApprate.AppAssignment.AppCategory.Name,
                                    SubcategoryName = itemApprate.AppAssignment.AppSubCategory.Name,
                                    Maxpoints = itemApprate.AppAssignment.MaxRatingPoints,
                                    RatingPoints = itemApprate.RatingPoints,
                                    EmpAppConcluId = singleEmpappConclusionitem.Id,
                                    Comments = itemApprate.Comments,
                                    EmpAppEvalID = itemEmpEval.Id,
                                };


                                EmpmLVdataClassList.Add(AppraisalApprovaldata);

                            }

                        }

                    }
                }
                else if (authority.ToUpper() == "MYSELF")
                {
                    SanctionStatus.Add(1);
                    SanctionStatus.Add(2);

                    ApporvalStatus.Add(3);
                    ApporvalStatus.Add(4);
                }


                //var listOfObject = new List<dynamic>();
                //listOfObject.Add(W);
                return Json(EmpmLVdataClassList, JsonRequestBehavior.AllowGet);
            }
        }



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
                    var returnobj = Create_Skill(lkval, form);
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