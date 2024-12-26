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
using System.Reflection;
using P2BUltimate.Security;
using Appraisal;
using Payroll;
using System.Collections.Specialized;
namespace P2BUltimate.Controllers.Appraisal.MainController
{
    public class EmpAppRatingConclusionController : Controller
    {
        //    private DataBaseContext db = new DataBaseContext();
        public ActionResult Index()
        {
            return View("~/Views/Appraisal/MainViews/EmpAppRatingConclusion/Index.cshtml");
        }
        public ActionResult GridPartial()
        {
            return View("~/Views/Shared/Appraisal/_EmpAppRatingConclusionPartial.cshtml");
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

        public ActionResult Get_Employelist(string AppCal, string SchName)
        {
            using (DataBaseContext db = new DataBaseContext())
            {


                List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                List<Employee> data = new List<Employee>();
                var compid = Convert.ToInt32(Session["CompId"].ToString());
                db.Database.CommandTimeout = 3600;

                var empdata = db.EmployeeAppraisal.Include(e => e.Employee).Include(e => e.Employee.EmpName)
                    .Include(e => e.EmpAppEvaluation).Include(e => e.EmpAppEvaluation.Select(r => r.AppraisalSchedule))
                   .AsNoTracking().AsParallel().ToList();

                List<Employee> Emp = new List<Employee>();
                if (SchName != null)
                {


                    foreach (var item in empdata)
                    {
                        EmpAppEvaluation OAppEval = item.EmpAppEvaluation.Where(e => e.AppraisalSchedule.BatchName == SchName).FirstOrDefault();
                        if (OAppEval != null)
                        {
                            Emp.Add(item.Employee);
                        }

                    }
                }



                if (Emp != null && Emp.Count != 0)
                {
                    foreach (var item in Emp)
                    {
                        if (SchName != "")
                        {
                            returndata.Add(new Utility.returndataclass
                            {
                                code = item.Id.ToString(),
                                value = item.FullDetails,
                            });
                        }

                    }

                    var returnjson = new
                    {
                        data = returndata,
                        tablename = "Employee-Table"
                    };
                    return Json(new List<Object> { returnjson }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new Utility.JsonReturnClass { success = false, responseText = "No Record Found OR Data is already assigned for all employees in this batch!" }, JsonRequestBehavior.AllowGet);
                    //return Json("No Record Found", JsonRequestBehavior.AllowGet);
                }
            }
        }



        public ActionResult LoadEmp1(P2BGrid_Parameters gp, string extraeditdata)
        {
            List<string> Msg = new List<string>();

            try
            {
                DataBaseContext db = new DataBaseContext();
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                var jsonData = (Object)null;
                int EmpId = Convert.ToInt32(extraeditdata.Split(',')[0]);
                int AssId = Convert.ToInt32(extraeditdata.Split(',')[0]);

                List<EmpAppRatingConclusionDetails> EmpAppRatingConclusionDetailsList = new List<EmpAppRatingConclusionDetails>();

                //var Sal = db.CompanyPayroll.Where(e => e.Company.Id == id).Include(e => e.SalHeadFormula).Select(e => e.SalHeadFormula).SingleOrDefault();
                var OEmployee = db.Employee.Include(e => e.PayStruct).Include(e => e.GeoStruct).Include(e => e.FuncStruct)
                    .Where(e => e.Id == EmpId).SingleOrDefault();
                var AppAssignmentList = db.AppAssignment.GroupBy(e => e.AppSubCategory).Select(e => e.FirstOrDefault()).Include(e => e.AppCategory)
                    .Include(e => e.AppSubCategory).ToList();
                foreach (var i in AppAssignmentList)
                {
                    var AppAssignlist = db.AppAssignment.Include(e => e.AppCategory).Include(e => e.AppSubCategory)
                        .Where(e => e.AppCategory.Id == i.AppCategory.Id && e.AppSubCategory.Id == i.AppSubCategory.Id)
                                .ToList();

                    foreach (var j in AppAssignlist)
                    {
                        //var AppAsslist = db.AppAssignment.Include(e => e.AppCategory).Include(e => e.AppSubCategory).Include(e => e.FuncStruct).Include(e => e.GeoStruct).Include(e => e.PayStruct)
                        //                      .Where(e => e.AppCategory.Id == j.AppCategory.Id && e.AppSubCategory.Id == j.AppSubCategory.Id
                        //                          && (e.FuncStruct.Id == OEmployee.FuncStruct.Id || e.GeoStruct.Id == OEmployee.GeoStruct.Id || e.PayStruct.Id == OEmployee.PayStruct.Id))
                        //                              .SingleOrDefault();
                        var AppAsslist = db.AppAssignment.Include(e => e.AppCategory).Include(e => e.AppSubCategory)
                                        .Where(e => e.Id == OEmployee.Id).SingleOrDefault();

                        //var AppAsslist1 = db.AppAssignment.Include(e => e.AppCategory).Include(e => e.AppSubCategory).Include(e => e.FuncStruct).Include(e => e.GeoStruct).Include(e => e.PayStruct)
                        //             .Where(e => e.GeoStruct.Id == OEmployee.GeoStruct.Id).ToList();


                        if (AppAsslist != null)
                        {



                            EmpAppRatingConclusionDetailsList.Add(new EmpAppRatingConclusionDetails
                            {
                                ID = AppAsslist.Id,
                                CatName = AppAsslist.AppCategory.Name != null ? AppAsslist.AppCategory.Name : null,
                                SubCatName = AppAsslist.AppSubCategory.Name != null ? AppAsslist.AppSubCategory.Name : null,
                                MaxRatingPoints = AppAsslist.MaxRatingPoints,
                                //  Appriasalassistance = "",
                                RatingPoints = 0,
                                Comments = "",
                                Editable = "true"
                            });

                        }
                        break;
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
                            // gp.sidx == "Appriasalassistance" ? c.Appriasalassistance.ToString() :
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

        public ActionResult LoadEmp(P2BGrid_Parameters gp, string extraeditdata, FormCollection form)
        {
            List<string> Msg = new List<string>();

            try
            {
                DataBaseContext db = new DataBaseContext();
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                var jsonData = (Object)null;
                int EmpId = Convert.ToInt32(extraeditdata.Split(',')[0]);
                int AssId = Convert.ToInt32(extraeditdata.Split(',')[0]);

                List<EmpAppRatingConclusionDetails> EmpAppRatingConclusionDetailsList = new List<EmpAppRatingConclusionDetails>();

                //var Sal = db.CompanyPayroll.Where(e => e.Company.Id == id).Include(e => e.SalHeadFormula).Select(e => e.SalHeadFormula).SingleOrDefault();
                var OEmployee = db.Employee.Include(e => e.PayStruct).Include(e => e.GeoStruct).Include(e => e.FuncStruct)
                    .Where(e => e.Id == EmpId).SingleOrDefault();
                var emplk = db.EmployeeAppraisal.Include(e => e.EmpAppEvaluation)
                    .Include(e => e.EmpAppEvaluation.Select(r => r.AppraisalPeriodCalendar))
                    .Include(e => e.EmpAppEvaluation.Select(r => r.EmpAppRatingConclusion))
                    .Include(e => e.EmpAppEvaluation.Select(r => r.EmpAppRatingConclusion.Select(t => t.EmpAppRating)))
                    .Include(e => e.Employee).Where(e => e.Employee.Id == OEmployee.Id).FirstOrDefault();




                var LookupAppAssitanceDATA = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1068").FirstOrDefault().LookupValues.Where(l => l.LookupVal != null)
                      .Select(a => new
                      {
                          Lkvalues = a.LookupVal,
                      }).ToList();

                List<string> LKVAL = new List<string>();
                if (LookupAppAssitanceDATA.Count() > 0)
                {
                    LKVAL = LookupAppAssitanceDATA.Select(s => s.Lkvalues).ToList();
                }


                #region For AppAssignment GeoStruct, PayStruct and FuncStruct True

                if (emplk != null)
                {

                    var AppAsslist1 = db.AppAssignment.Include(e => e.AppCategory).Include(e => e.AppraisalCalendar).Include(e => e.AppSubCategory).Include(e => e.FuncStruct).Include(e => e.GeoStruct).Include(e => e.PayStruct)
                                          .Where(e => e.GeoStruct.Id == OEmployee.GeoStruct.Id && e.PayStruct.Id == OEmployee.PayStruct.Id && e.FuncStruct.Id == OEmployee.FuncStruct.Id).ToList();

                    if (AppAsslist1.Count() != 0)
                    {

                        foreach (var AppAsslist in AppAsslist1)
                        {
                            var EmpAppEval = emplk.EmpAppEvaluation.Where(e => e.AppraisalPeriodCalendar.Id == AppAsslist.AppraisalCalendar.Id).FirstOrDefault();
                            if (EmpAppEval != null)
                            {
                                var EARC = EmpAppEval.EmpAppRatingConclusion.Select(c => new
                                {
                                    EmpAppRatingConcId = c.Id,
                                    AppraisalAssistanceLk = c.AppraisalAssistance.LookupVal,
                                    EmpAppRateValue = c.EmpAppRating.Select(r => new
                                    {
                                        EARAppAssignmentId = r.AppAssignment.Id,
                                        EARComments = r.Comments,
                                        EARatingPNT = r.RatingPoints,
                                    }).Where(e => e.EARAppAssignmentId == AppAsslist.Id).ToList(),

                                }).Where(e => LKVAL.Contains(e.AppraisalAssistanceLk)).ToList();

                                var getEAR = EARC.Where(e => e.EmpAppRateValue.Count() > 0).Select(d => d.EmpAppRateValue).ToList();

                                if (EARC.Count() > 0 && getEAR.Count() > 0)
                                {
                                    foreach (var itemEARC in EARC)
                                    {
                                        foreach (var itemEAR in itemEARC.EmpAppRateValue)
                                        {
                                            EmpAppRatingConclusionDetailsList.Add(new EmpAppRatingConclusionDetails
                                            {
                                                ID = AppAsslist.Id,
                                                EmpEvaluationId = EmpAppEval.Id,
                                                CatName = AppAsslist == null ? "" : AppAsslist.AppCategory == null ? "" : AppAsslist.AppCategory.Name == null ? "" : AppAsslist.AppCategory.Name.ToString(),
                                                SubCatName = AppAsslist == null ? "" : AppAsslist.AppSubCategory == null ? "" : AppAsslist.AppSubCategory.Name == null ? "" : AppAsslist.AppSubCategory.Name.ToString(),

                                                MaxRatingPoints = AppAsslist == null ? 0 : AppAsslist.MaxRatingPoints == null ? 0 : AppAsslist.MaxRatingPoints,
                                                RatingPoints = itemEAR.EARatingPNT,
                                                Comments = itemEAR.EARComments,
                                                Editable = "true"
                                            });
                                        }
                                    }
                                }
                                else
                                {
                                    EmpAppRatingConclusionDetailsList.Add(new EmpAppRatingConclusionDetails
                                    {
                                        ID = AppAsslist.Id,
                                        EmpEvaluationId = EmpAppEval.Id,
                                        CatName = AppAsslist == null ? "" : AppAsslist.AppCategory == null ? "" : AppAsslist.AppCategory.Name == null ? "" : AppAsslist.AppCategory.Name.ToString(),
                                        SubCatName = AppAsslist == null ? "" : AppAsslist.AppSubCategory == null ? "" : AppAsslist.AppSubCategory.Name == null ? "" : AppAsslist.AppSubCategory.Name.ToString(),

                                        MaxRatingPoints = AppAsslist == null ? 0 : AppAsslist.MaxRatingPoints == null ? 0 : AppAsslist.MaxRatingPoints,
                                        RatingPoints = 0,
                                        Comments = "",
                                        Editable = "true"
                                    });
                                }

                            }
                        }
                    }
                }

                #endregion

                #region For AppAssignment GeoStruct and PayStruct True

                if (emplk != null)
                {
                    var AppAsslist1 = db.AppAssignment.Include(e => e.AppCategory).Include(e => e.AppraisalCalendar).Include(e => e.AppSubCategory).Include(e => e.FuncStruct).Include(e => e.GeoStruct).Include(e => e.PayStruct)
                                          .Where(e => e.GeoStruct.Id == OEmployee.GeoStruct.Id && e.PayStruct.Id == OEmployee.PayStruct.Id && e.FuncStruct.Id == null).ToList();

                    if (AppAsslist1.Count() != 0)
                    {
                        foreach (var AppAsslist in AppAsslist1)
                        {

                            var EmpAppEval = emplk.EmpAppEvaluation.Where(e => e.AppraisalPeriodCalendar.Id == AppAsslist.AppraisalCalendar.Id).FirstOrDefault();
                            if (EmpAppEval != null)
                            {
                                var EARC = EmpAppEval.EmpAppRatingConclusion.Select(c => new
                                {
                                    EmpAppRatingConcId = c.Id,
                                    AppraisalAssistanceLk = c.AppraisalAssistance.LookupVal,
                                    EmpAppRateValue = c.EmpAppRating.Select(r => new
                                    {
                                        EARAppAssignmentId = r.AppAssignment.Id,
                                        EARComments = r.Comments,
                                        EARatingPNT = r.RatingPoints,
                                    }).Where(e => e.EARAppAssignmentId == AppAsslist.Id).ToList(),

                                }).Where(e => LKVAL.Contains(e.AppraisalAssistanceLk)).ToList();


                                var getEAR = EARC.Where(e => e.EmpAppRateValue.Count() > 0).Select(d => d.EmpAppRateValue).ToList();
                                if (EARC.Count() > 0 && getEAR.Count() > 0)
                                {
                                    foreach (var itemEARC in EARC)
                                    {
                                        foreach (var itemEAR in itemEARC.EmpAppRateValue)
                                        {
                                            EmpAppRatingConclusionDetailsList.Add(new EmpAppRatingConclusionDetails
                                            {
                                                ID = AppAsslist.Id,
                                                EmpEvaluationId = EmpAppEval.Id,
                                                CatName = AppAsslist == null ? "" : AppAsslist.AppCategory == null ? "" : AppAsslist.AppCategory.Name == null ? "" : AppAsslist.AppCategory.Name.ToString(),
                                                SubCatName = AppAsslist == null ? "" : AppAsslist.AppSubCategory == null ? "" : AppAsslist.AppSubCategory.Name == null ? "" : AppAsslist.AppSubCategory.Name.ToString(),

                                                MaxRatingPoints = AppAsslist == null ? 0 : AppAsslist.MaxRatingPoints == null ? 0 : AppAsslist.MaxRatingPoints,
                                                RatingPoints = itemEAR.EARatingPNT,
                                                Comments = itemEAR.EARComments,
                                                Editable = "true"
                                            });
                                        }
                                    }

                                }
                                else
                                {
                                    EmpAppRatingConclusionDetailsList.Add(new EmpAppRatingConclusionDetails
                                    {
                                        ID = AppAsslist.Id,
                                        EmpEvaluationId = EmpAppEval.Id,
                                        CatName = AppAsslist == null ? "" : AppAsslist.AppCategory == null ? "" : AppAsslist.AppCategory.Name == null ? "" : AppAsslist.AppCategory.Name.ToString(),
                                        SubCatName = AppAsslist == null ? "" : AppAsslist.AppSubCategory == null ? "" : AppAsslist.AppSubCategory.Name == null ? "" : AppAsslist.AppSubCategory.Name.ToString(),

                                        MaxRatingPoints = AppAsslist == null ? 0 : AppAsslist.MaxRatingPoints == null ? 0 : AppAsslist.MaxRatingPoints,
                                        RatingPoints = 0,
                                        Comments = "",
                                        Editable = "true"
                                    });
                                }

                            }
                        }
                    }
                }

                #endregion

                #region For AppAssignment GeoStruct and FuncStruct True

                if (emplk != null)
                {
                    var AppAsslist1 = db.AppAssignment.Include(e => e.AppCategory).Include(e => e.AppraisalCalendar).Include(e => e.AppSubCategory).Include(e => e.FuncStruct).Include(e => e.GeoStruct).Include(e => e.PayStruct)
                                          .Where(e => e.GeoStruct.Id == OEmployee.GeoStruct.Id && e.PayStruct.Id == null && e.FuncStruct.Id == OEmployee.FuncStruct.Id).ToList();

                    if (AppAsslist1.Count() != 0)
                    {

                        foreach (var AppAsslist in AppAsslist1)
                        {

                            var EmpAppEval = emplk.EmpAppEvaluation.Where(e => e.AppraisalPeriodCalendar.Id == AppAsslist.AppraisalCalendar.Id).FirstOrDefault();
                            if (EmpAppEval != null)
                            {
                                var EARC = EmpAppEval.EmpAppRatingConclusion.Select(c => new
                                {
                                    EmpAppRatingConcId = c.Id,
                                    AppraisalAssistanceLk = c.AppraisalAssistance.LookupVal,
                                    EmpAppRateValue = c.EmpAppRating.Select(r => new
                                    {
                                        EARAppAssignmentId = r.AppAssignment.Id,
                                        EARComments = r.Comments,
                                        EARatingPNT = r.RatingPoints,
                                    }).Where(e => e.EARAppAssignmentId == AppAsslist.Id).ToList(),

                                }).Where(e => LKVAL.Contains(e.AppraisalAssistanceLk)).ToList();


                                var getEAR = EARC.Where(e => e.EmpAppRateValue.Count() > 0).Select(d => d.EmpAppRateValue).ToList();
                                if (EARC.Count() > 0 && getEAR.Count() > 0)
                                {
                                    foreach (var itemEARC in EARC)
                                    {
                                        foreach (var itemEAR in itemEARC.EmpAppRateValue)
                                        {
                                            EmpAppRatingConclusionDetailsList.Add(new EmpAppRatingConclusionDetails
                                            {
                                                ID = AppAsslist.Id,
                                                EmpEvaluationId = EmpAppEval.Id,
                                                CatName = AppAsslist == null ? "" : AppAsslist.AppCategory == null ? "" : AppAsslist.AppCategory.Name == null ? "" : AppAsslist.AppCategory.Name.ToString(),
                                                SubCatName = AppAsslist == null ? "" : AppAsslist.AppSubCategory == null ? "" : AppAsslist.AppSubCategory.Name == null ? "" : AppAsslist.AppSubCategory.Name.ToString(),

                                                MaxRatingPoints = AppAsslist == null ? 0 : AppAsslist.MaxRatingPoints == null ? 0 : AppAsslist.MaxRatingPoints,
                                                RatingPoints = itemEAR.EARatingPNT,
                                                Comments = itemEAR.EARComments,
                                                Editable = "true"
                                            });
                                        }
                                    }
                                }
                                else
                                {
                                    EmpAppRatingConclusionDetailsList.Add(new EmpAppRatingConclusionDetails
                                    {
                                        ID = AppAsslist.Id,
                                        EmpEvaluationId = EmpAppEval.Id,
                                        CatName = AppAsslist == null ? "" : AppAsslist.AppCategory == null ? "" : AppAsslist.AppCategory.Name == null ? "" : AppAsslist.AppCategory.Name.ToString(),
                                        SubCatName = AppAsslist == null ? "" : AppAsslist.AppSubCategory == null ? "" : AppAsslist.AppSubCategory.Name == null ? "" : AppAsslist.AppSubCategory.Name.ToString(),

                                        MaxRatingPoints = AppAsslist == null ? 0 : AppAsslist.MaxRatingPoints == null ? 0 : AppAsslist.MaxRatingPoints,
                                        RatingPoints = 0,
                                        Comments = "",
                                        Editable = "true"
                                    });
                                }

                            }
                        }
                    }
                }

                #endregion

                #region For AppAssignment PayStruct and FuncStruct True

                if (emplk != null)
                {

                    var AppAsslist1 = db.AppAssignment.Include(e => e.AppCategory).Include(e => e.AppraisalCalendar).Include(e => e.AppSubCategory).Include(e => e.FuncStruct).Include(e => e.GeoStruct).Include(e => e.PayStruct)
                                          .Where(e => e.GeoStruct.Id == null && e.PayStruct.Id == OEmployee.PayStruct.Id && e.FuncStruct.Id == OEmployee.FuncStruct.Id).ToList();

                    if (AppAsslist1.Count() != 0)
                    {
                        foreach (var AppAsslist in AppAsslist1)
                        {

                            var EmpAppEval = emplk.EmpAppEvaluation.Where(e => e.AppraisalPeriodCalendar.Id == AppAsslist.AppraisalCalendar.Id).FirstOrDefault();
                            if (EmpAppEval != null)
                            {
                                var EARC = EmpAppEval.EmpAppRatingConclusion.Select(c => new
                                {
                                    EmpAppRatingConcId = c.Id,
                                    AppraisalAssistanceLk = c.AppraisalAssistance.LookupVal,
                                    EmpAppRateValue = c.EmpAppRating.Select(r => new
                                    {
                                        EARAppAssignmentId = r.AppAssignment.Id,
                                        EARComments = r.Comments,
                                        EARatingPNT = r.RatingPoints,
                                    }).Where(e => e.EARAppAssignmentId == AppAsslist.Id).ToList(),

                                }).Where(e => LKVAL.Contains(e.AppraisalAssistanceLk)).ToList();


                                var getEAR = EARC.Where(e => e.EmpAppRateValue.Count() > 0).Select(d => d.EmpAppRateValue).ToList();
                                if (EARC.Count() > 0 && getEAR.Count() > 0)
                                {
                                    foreach (var itemEARC in EARC)
                                    {
                                        foreach (var itemEAR in itemEARC.EmpAppRateValue)
                                        {
                                            EmpAppRatingConclusionDetailsList.Add(new EmpAppRatingConclusionDetails
                                            {
                                                ID = AppAsslist.Id,
                                                EmpEvaluationId = EmpAppEval.Id,
                                                CatName = AppAsslist == null ? "" : AppAsslist.AppCategory == null ? "" : AppAsslist.AppCategory.Name == null ? "" : AppAsslist.AppCategory.Name.ToString(),
                                                SubCatName = AppAsslist == null ? "" : AppAsslist.AppSubCategory == null ? "" : AppAsslist.AppSubCategory.Name == null ? "" : AppAsslist.AppSubCategory.Name.ToString(),

                                                MaxRatingPoints = AppAsslist == null ? 0 : AppAsslist.MaxRatingPoints == null ? 0 : AppAsslist.MaxRatingPoints,
                                                RatingPoints = itemEAR.EARatingPNT,
                                                Comments = itemEAR.EARComments,
                                                Editable = "true"
                                            });
                                        }
                                    }
                                }
                                else
                                {
                                    EmpAppRatingConclusionDetailsList.Add(new EmpAppRatingConclusionDetails
                                    {
                                        ID = AppAsslist.Id,
                                        EmpEvaluationId = EmpAppEval.Id,
                                        CatName = AppAsslist == null ? "" : AppAsslist.AppCategory == null ? "" : AppAsslist.AppCategory.Name == null ? "" : AppAsslist.AppCategory.Name.ToString(),
                                        SubCatName = AppAsslist == null ? "" : AppAsslist.AppSubCategory == null ? "" : AppAsslist.AppSubCategory.Name == null ? "" : AppAsslist.AppSubCategory.Name.ToString(),

                                        MaxRatingPoints = AppAsslist == null ? 0 : AppAsslist.MaxRatingPoints == null ? 0 : AppAsslist.MaxRatingPoints,
                                        RatingPoints = 0,
                                        Comments = "",
                                        Editable = "true"
                                    });
                                }

                            }
                        }
                    }
                }

                #endregion

                #region For AppAssignment GeoStruct True

                if (emplk != null)
                {

                    var AppAsslist1 = db.AppAssignment.Include(e => e.AppCategory).Include(e => e.AppraisalCalendar).Include(e => e.AppSubCategory).Include(e => e.FuncStruct).Include(e => e.GeoStruct).Include(e => e.PayStruct)
                                          .Where(e => e.GeoStruct.Id == OEmployee.GeoStruct.Id && e.PayStruct.Id == null && e.FuncStruct.Id == null).ToList();

                    if (AppAsslist1.Count() != 0)
                    {

                        foreach (var AppAsslist in AppAsslist1)
                        {

                            var EmpAppEval = emplk.EmpAppEvaluation.Where(e => e.AppraisalPeriodCalendar.Id == AppAsslist.AppraisalCalendar.Id).FirstOrDefault();
                            if (EmpAppEval != null)
                            {
                                var EARC = EmpAppEval.EmpAppRatingConclusion.Select(c => new
                                {
                                    EmpAppRatingConcId = c.Id,
                                    AppraisalAssistanceLk = c.AppraisalAssistance.LookupVal,
                                    EmpAppRateValue = c.EmpAppRating.Select(r => new
                                    {
                                        EARAppAssignmentId = r.AppAssignment.Id,
                                        EARComments = r.Comments,
                                        EARatingPNT = r.RatingPoints,
                                    }).Where(e => e.EARAppAssignmentId == AppAsslist.Id).ToList(),

                                }).Where(e => LKVAL.Contains(e.AppraisalAssistanceLk)).ToList();


                                var getEAR = EARC.Where(e => e.EmpAppRateValue.Count() > 0).Select(d => d.EmpAppRateValue).ToList();
                                if (EARC.Count() > 0 && getEAR.Count() > 0)
                                {
                                    foreach (var itemEARC in EARC)
                                    {
                                        foreach (var itemEAR in itemEARC.EmpAppRateValue)
                                        {
                                            EmpAppRatingConclusionDetailsList.Add(new EmpAppRatingConclusionDetails
                                            {
                                                ID = AppAsslist.Id,
                                                EmpEvaluationId = EmpAppEval.Id,
                                                CatName = AppAsslist == null ? "" : AppAsslist.AppCategory == null ? "" : AppAsslist.AppCategory.Name == null ? "" : AppAsslist.AppCategory.Name.ToString(),
                                                SubCatName = AppAsslist == null ? "" : AppAsslist.AppSubCategory == null ? "" : AppAsslist.AppSubCategory.Name == null ? "" : AppAsslist.AppSubCategory.Name.ToString(),

                                                MaxRatingPoints = AppAsslist == null ? 0 : AppAsslist.MaxRatingPoints == null ? 0 : AppAsslist.MaxRatingPoints,
                                                RatingPoints = itemEAR.EARatingPNT,
                                                Comments = itemEAR.EARComments,
                                                Editable = "true"
                                            });
                                        }
                                    }
                                }
                                else
                                {
                                    EmpAppRatingConclusionDetailsList.Add(new EmpAppRatingConclusionDetails
                                    {
                                        ID = AppAsslist.Id,
                                        EmpEvaluationId = EmpAppEval.Id,
                                        CatName = AppAsslist == null ? "" : AppAsslist.AppCategory == null ? "" : AppAsslist.AppCategory.Name == null ? "" : AppAsslist.AppCategory.Name.ToString(),
                                        SubCatName = AppAsslist == null ? "" : AppAsslist.AppSubCategory == null ? "" : AppAsslist.AppSubCategory.Name == null ? "" : AppAsslist.AppSubCategory.Name.ToString(),

                                        MaxRatingPoints = AppAsslist == null ? 0 : AppAsslist.MaxRatingPoints == null ? 0 : AppAsslist.MaxRatingPoints,
                                        RatingPoints = 0,
                                        Comments = "",
                                        Editable = "true"
                                    });
                                }

                            }
                        }
                    }
                }

                #endregion

                #region For AppAssignment PayStruct True

                if (emplk != null)
                {

                    var AppAsslist1 = db.AppAssignment.Include(e => e.AppCategory).Include(e => e.AppraisalCalendar).Include(e => e.AppSubCategory).Include(e => e.FuncStruct).Include(e => e.GeoStruct).Include(e => e.PayStruct)
                                          .Where(e => e.PayStruct.Id == OEmployee.PayStruct.Id && e.GeoStruct.Id == null && e.FuncStruct.Id == null).ToList();

                    if (AppAsslist1.Count() != 0)
                    {

                        foreach (var AppAsslist in AppAsslist1)
                        {
                            var EmpAppEval = emplk.EmpAppEvaluation.Where(e => e.AppraisalPeriodCalendar.Id == AppAsslist.AppraisalCalendar.Id).FirstOrDefault();
                            if (EmpAppEval != null)
                            {
                                var EARC = EmpAppEval.EmpAppRatingConclusion.Select(c => new
                                {
                                    EmpAppRatingConcId = c.Id,
                                    AppraisalAssistanceLk = c.AppraisalAssistance.LookupVal,
                                    EmpAppRateValue = c.EmpAppRating.Select(r => new
                                    {
                                        EARAppAssignmentId = r.AppAssignment.Id,
                                        EARComments = r.Comments,
                                        EARatingPNT = r.RatingPoints,
                                    }).Where(e => e.EARAppAssignmentId == AppAsslist.Id).ToList(),

                                }).Where(e => LKVAL.Contains(e.AppraisalAssistanceLk)).ToList();


                                var getEAR = EARC.Where(e => e.EmpAppRateValue.Count() > 0).Select(d => d.EmpAppRateValue).ToList();
                                if (EARC.Count() > 0 && getEAR.Count() > 0)
                                {
                                    foreach (var itemEARC in EARC)
                                    {
                                        foreach (var itemEAR in itemEARC.EmpAppRateValue)
                                        {
                                            EmpAppRatingConclusionDetailsList.Add(new EmpAppRatingConclusionDetails
                                            {
                                                ID = AppAsslist.Id,
                                                EmpEvaluationId = EmpAppEval.Id,
                                                CatName = AppAsslist == null ? "" : AppAsslist.AppCategory == null ? "" : AppAsslist.AppCategory.Name == null ? "" : AppAsslist.AppCategory.Name.ToString(),
                                                SubCatName = AppAsslist == null ? "" : AppAsslist.AppSubCategory == null ? "" : AppAsslist.AppSubCategory.Name == null ? "" : AppAsslist.AppSubCategory.Name.ToString(),

                                                MaxRatingPoints = AppAsslist == null ? 0 : AppAsslist.MaxRatingPoints == null ? 0 : AppAsslist.MaxRatingPoints,
                                                RatingPoints = itemEAR.EARatingPNT,
                                                Comments = itemEAR.EARComments,
                                                Editable = "true"
                                            });
                                        }
                                    }
                                }
                                else
                                {
                                    EmpAppRatingConclusionDetailsList.Add(new EmpAppRatingConclusionDetails
                                    {
                                        ID = AppAsslist.Id,
                                        EmpEvaluationId = EmpAppEval.Id,
                                        CatName = AppAsslist == null ? "" : AppAsslist.AppCategory == null ? "" : AppAsslist.AppCategory.Name == null ? "" : AppAsslist.AppCategory.Name.ToString(),
                                        SubCatName = AppAsslist == null ? "" : AppAsslist.AppSubCategory == null ? "" : AppAsslist.AppSubCategory.Name == null ? "" : AppAsslist.AppSubCategory.Name.ToString(),

                                        MaxRatingPoints = AppAsslist == null ? 0 : AppAsslist.MaxRatingPoints == null ? 0 : AppAsslist.MaxRatingPoints,
                                        RatingPoints = 0,
                                        Comments = "",
                                        Editable = "true"
                                    });
                                }

                            }
                        }
                    }
                }
                #endregion

                #region For AppAssignment FuncStruct True

                if (emplk != null)
                {

                    var AppAsslist1 = db.AppAssignment.Include(e => e.AppraisalCalendar).Include(e => e.AppCategory).Include(e => e.AppSubCategory).Include(e => e.FuncStruct).Include(e => e.GeoStruct).Include(e => e.PayStruct)
                                          .Where(e => e.FuncStruct.Id == OEmployee.FuncStruct.Id && e.GeoStruct.Id == null && e.PayStruct.Id == null).ToList();


                    if (AppAsslist1.Count() != 0)
                    {

                        foreach (var AppAsslist in AppAsslist1)
                        {
                            var EmpAppEval = emplk.EmpAppEvaluation.Where(e => e.AppraisalPeriodCalendar.Id == AppAsslist.AppraisalCalendar.Id).FirstOrDefault();
                            if (EmpAppEval != null)
                            {
                                var EARC = EmpAppEval.EmpAppRatingConclusion.Select(c => new
                                {
                                    EmpAppRatingConcId = c.Id,
                                    AppraisalAssistanceLk = c.AppraisalAssistance.LookupVal,
                                    EmpAppRateValue = c.EmpAppRating.Select(r => new
                                    {
                                        EARAppAssignmentId = r.AppAssignment.Id,
                                        EARComments = r.Comments,
                                        EARatingPNT = r.RatingPoints,
                                    }).Where(e => e.EARAppAssignmentId == AppAsslist.Id).ToList(),

                                }).Where(e => LKVAL.Contains(e.AppraisalAssistanceLk)).ToList();


                                var getEAR = EARC.Where(e => e.EmpAppRateValue.Count() > 0).Select(d => d.EmpAppRateValue).ToList();
                                if (EARC.Count() > 0 && getEAR.Count() > 0)
                                {
                                    foreach (var itemEARC in EARC)
                                    {
                                        foreach (var itemEAR in itemEARC.EmpAppRateValue)
                                        {
                                            EmpAppRatingConclusionDetailsList.Add(new EmpAppRatingConclusionDetails
                                            {
                                                ID = AppAsslist.Id,
                                                EmpEvaluationId = EmpAppEval.Id,
                                                CatName = AppAsslist == null ? "" : AppAsslist.AppCategory == null ? "" : AppAsslist.AppCategory.Name == null ? "" : AppAsslist.AppCategory.Name.ToString(),
                                                SubCatName = AppAsslist == null ? "" : AppAsslist.AppSubCategory == null ? "" : AppAsslist.AppSubCategory.Name == null ? "" : AppAsslist.AppSubCategory.Name.ToString(),

                                                MaxRatingPoints = AppAsslist == null ? 0 : AppAsslist.MaxRatingPoints == null ? 0 : AppAsslist.MaxRatingPoints,
                                                RatingPoints = itemEAR.EARatingPNT,
                                                Comments = itemEAR.EARComments,
                                                Editable = "true"
                                            });
                                        }
                                    }
                                }
                                else
                                {
                                    EmpAppRatingConclusionDetailsList.Add(new EmpAppRatingConclusionDetails
                                    {
                                        ID = AppAsslist.Id,
                                        EmpEvaluationId = EmpAppEval.Id,
                                        CatName = AppAsslist == null ? "" : AppAsslist.AppCategory == null ? "" : AppAsslist.AppCategory.Name == null ? "" : AppAsslist.AppCategory.Name.ToString(),
                                        SubCatName = AppAsslist == null ? "" : AppAsslist.AppSubCategory == null ? "" : AppAsslist.AppSubCategory.Name == null ? "" : AppAsslist.AppSubCategory.Name.ToString(),

                                        MaxRatingPoints = AppAsslist == null ? 0 : AppAsslist.MaxRatingPoints == null ? 0 : AppAsslist.MaxRatingPoints,
                                        RatingPoints = 0,
                                        Comments = "",
                                        Editable = "true"
                                    });
                                }

                            }
                        }
                    }
                }
                #endregion

                //        break;
                //    }
                //}
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
                                || (e.EmpEvaluationId.ToString().Contains(gp.searchString.ToUpper()))
                               || (e.CatName.ToUpper().Contains(gp.searchString.ToUpper()))
                               || (e.SubCatName.ToUpper().Contains(gp.searchString.ToUpper()))
                               || (e.MaxRatingPoints.ToString().Contains(gp.searchString))
                               || (e.RatingPoints.ToString().Contains(gp.searchString))
                               ).Select(a => new { a.ID, a.EmpEvaluationId, a.CatName, a.SubCatName, a.MaxRatingPoints, a.RatingPoints, a.Comments, a.Editable }).ToList();
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
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.ID, a.EmpEvaluationId, a.CatName, a.SubCatName, a.MaxRatingPoints, a.RatingPoints, a.Comments, a.Editable }).ToList();
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
                        jsonData = IE.Select(a => new Object[] { a.ID, a.EmpEvaluationId, a.CatName, a.SubCatName, a.MaxRatingPoints, a.RatingPoints, a.Comments, a.Editable }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.ID, a.EmpEvaluationId, a.CatName, a.SubCatName, a.MaxRatingPoints, a.RatingPoints, a.Comments, a.Editable }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.ID, a.EmpEvaluationId, a.CatName, a.SubCatName, a.MaxRatingPoints, a.RatingPoints, a.Comments, a.Editable }).ToList();
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

        public ActionResult LoadAssData(P2BGrid_Parameters gp, string extraeditdata)
        {
            List<string> Msg = new List<string>();


            try
            {
                DataBaseContext db = new DataBaseContext();
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                var jsonData = (Object)null;
                int EmpId = Convert.ToInt32(extraeditdata.Split(',')[0]);
                int AssId = Convert.ToInt32(extraeditdata.Split(',')[2]);

                List<EmpAppRatingConclusionDetails> EmpAppRatingConclusionDetailsList = new List<EmpAppRatingConclusionDetails>();

                var db_data = db.EmployeeAppraisal
             .Include(e => e.EmpAppEvaluation)
             .Include(e => e.EmpAppEvaluation.Select(a => a.EmpAppRatingConclusion))
             .Include(e => e.EmpAppEvaluation.Select(a => a.EmpAppRatingConclusion.Select(s => s.EmpAppRating)))
             .Include(e => e.EmpAppEvaluation.Select(a => a.EmpAppRatingConclusion.Select(s => s.EmpAppRating.Select(r => r.AppAssignment))))
             .Include(e => e.EmpAppEvaluation.Select(a => a.EmpAppRatingConclusion.Select(s => s.EmpAppRating.Select(r => r.AppAssignment.AppCategory))))
             .Include(e => e.EmpAppEvaluation.Select(a => a.EmpAppRatingConclusion.Select(s => s.EmpAppRating.Select(r => r.AppAssignment.AppSubCategory))))
             .Include(e => e.EmpAppEvaluation.Select(a => a.EmpAppRatingConclusion.Select(s => s.EmpAppRating.Select(r => r.AppAssignment.AppRatingObjective))))
             .Include(e => e.EmpAppEvaluation.Select(a => a.EmpAppRatingConclusion.Select(s => s.EmpAppRating.Select(r => r.AppAssignment.AppRatingObjective.Select(q => q.ObjectiveWordings)))))
             .Where(e => e.Id == EmpId).SingleOrDefault();
                if (db_data != null)
                {
                    List<DeserializeClass> returndata = new List<DeserializeClass>();

                    foreach (var item in db_data.EmpAppEvaluation)
                    {
                        foreach (var item1 in item.EmpAppRatingConclusion)
                        {
                            foreach (var item2 in item1.EmpAppRating)
                            {
                                var obj = item2.AppAssignment.AppRatingObjective.Select(e => e.ObjectiveWordings.LookupVal.ToString()).SingleOrDefault();
                                returndata.Add(new DeserializeClass
                                {
                                    Id = item1.Id.ToString(),
                                    Category = item2.AppAssignment.AppCategory.Name,
                                    AppriasalAssistance = item1.AppraisalAssistance == null ? "" : item1.AppraisalAssistance.LookupVal.ToString(),
                                    Comments = item2.Comments.ToString(),
                                    MaxPoints = item2.AppAssignment.MaxRatingPoints.ToString(),
                                    ObjectiveWordings = obj,
                                    RatingPoints = item2.RatingPoints.ToString(),
                                    SubCategory = item2.AppAssignment.AppSubCategory.Name,
                                    AppraiseePoints = "0",
                                    AppraiseeComments = "",
                                    AppraiserPoints = "0",
                                    AppraiserComments = "",
                                    HRPoints = "0",
                                    HRComments = ""
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
                            //  gp.sidx == "Appriasalassistance" ? c.Appriasalassistance.ToString() :
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
        public class DeserializedClass
        {
            public string Autho_Action { get; set; }
            public string Autho_Allow { get; set; }
            public string employee_table { get; set; }
            public string AppraisalAssistancelist { get; set; }
            public string AssistanceOverallComments { get; set; }


        }
        public class returndatagridclass //Parentgrid
        {
            public string Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
            public string JoiningDate { get; set; }
            public string Job { get; set; }
            public string Grade { get; set; }
            public string Location { get; set; }
        }

        //[HttpPost]
        //public ActionResult Create(string forwarddata, FormCollection form) //Create submit
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        string Emp = form["Employee-Table"] == null ? null : form["Employee-Table"];
        //        string AppraisalCalendar = form["AppCalendardrop"] == null ? null : form["AppCalendardrop"];
        //        //   string AppAssistance = form["AppraisalAssistancelist"] == null ? null : form["AppraisalAssistancelist"];
        //        string AppAssistance = form["AppraisalAssistancelist"] == "0" ? "" : form["AppraisalAssistancelist"];
        //        string AssistanceOverallCo = form["AssistanceOverallComments"] == "" ? "" : form["AssistanceOverallComments"];




        //        EmpAppRatingConclusion p = new EmpAppRatingConclusion();

        //        List<String> Msg = new List<String>();
        //        try
        //        {
        //            List<int> ids1 = null;
        //            if (Emp != null && Emp != "0" && Emp != "false")
        //            {
        //                ids1 = Utility.StringIdsToListIds(Emp);
        //            }
        //            else
        //            {
        //                List<string> Msgu = new List<string>();
        //                Msgu.Add("  Kindly select employee  ");
        //                return Json(new Utility.JsonReturnClass { success = false, responseText = Msgu }, JsonRequestBehavior.AllowGet);
        //            }
        //            if (AssistanceOverallCo != null)
        //            {
        //                p.AssistanceOverallComments = AssistanceOverallCo.ToString();
        //            }

        //            var serialize = new JavaScriptSerializer();

        //            var obj = serialize.Deserialize<List<DeserializeClass>>(forwarddata);

        //            if (obj == null || obj.Count < 0)
        //            {
        //                return Json(new { sucess = true, responseText = "You have to change record to update." }, JsonRequestBehavior.AllowGet);
        //            }
        //            List<int> ids = obj.Select(e => int.Parse(e.Id)).ToList();
        //            List<int> idss = obj.Select(e => int.Parse(e.Id)).ToList();
        //            List<int> idss2 = obj.Select(e => int.Parse(e.EmpEvaluationId)).ToList();
        //            var evalids = Convert.ToInt32(idss2[0]);
        //            var iidd = Convert.ToInt32(idss[0]);

        //            var iid = Convert.ToInt32(ids1[0]);
        //            var q1 = db.Employee.Where(q => q.Id == (iid)).SingleOrDefault();

        //            if (AppAssistance != null && AppAssistance != "")
        //            {
        //                //LookupValue Lkval = db.LookupValue.Find(Convert.ToInt32(AppAssistance));
        //                //p.AppraisalAssistance = Lkval;

        //                LookupValue val = db.LookupValue.Find(int.Parse(AppAssistance));
        //                p.AppraisalAssistance = val;
        //            }

        //            //if (AppraisalCalendar != null && AppraisalCalendar != "")
        //            //{
        //            //    int AddId = Convert.ToInt32(AppraisalCalendar);
        //            //    var val = db.Calendar.Include(e => e.Name).Where(e => e.Id == AddId).SingleOrDefault();
        //            //    p.AppraisalPeriodCalendar = val;
        //            //}
        //            if (ModelState.IsValid)
        //            {
        //                using (TransactionScope ts = new TransactionScope())
        //                {
        //                    p.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
        //                    List<EmpAppRating> AppRatingList = new List<EmpAppRating>();
        //                    int RatingPoints = 0;
        //                    int maxpoint = 0;
        //                    foreach (int ca in ids)
        //                    {
        //                        AppAssignment AppA = db.AppAssignment.Include(e => e.AppRatingObjective).Include(a => a.AppRatingObjective.Select(b => b.ObjectiveWordings)).Where(e => e.Id == ca).SingleOrDefault();
        //                        // var ep = db.EmpAppEvaluation.Include(a => a.EmpAppRatingConclusion.Select(b => b.EmpAppRating.Select(c => c.AppAssignment).Where(f => f.Id == AppA.Id))).SingleOrDefault();
        //                        RatingPoints = Convert.ToInt32(obj.Where(e => e.Id == ca.ToString()).Select(e => e.RatingPoints).Single());
        //                        maxpoint = Convert.ToInt32(obj.Where(e => e.Id == ca.ToString()).Select(e => e.MaxPoints).Single());
        //                        var AppRatingObj = AppA.AppRatingObjective.OrderBy(e => e.RatingPoints).ToList();

        //                        LookupValue wording = null;
        //                        foreach (var a in AppRatingObj)
        //                        {
        //                            if (RatingPoints <= a.RatingPoints)
        //                            {
        //                                wording = a.ObjectiveWordings;
        //                            }
        //                        }

        //                        EmpAppRating EmpAppR = new EmpAppRating()
        //                        {
        //                            AppAssignment = AppA,
        //                            Comments = obj.Where(e => e.Id == ca.ToString()).Select(e => e.Comments).Single(),
        //                            ObjectiveWordings = wording,
        //                            RatingPoints = RatingPoints,
        //                            DBTrack = p.DBTrack,
        //                        };
        //                        AppRatingList.Add(EmpAppR);
        //                    }

        //                    EmpAppRatingConclusion Appcategory = new EmpAppRatingConclusion()
        //                    {
        //                        //AppraisalPeriodCalendar = p.AppraisalPeriodCalendar,
        //                        AppraisalAssistance = p.AppraisalAssistance,
        //                        DBTrack = p.DBTrack,
        //                        AssistanceOverallComments = p.AssistanceOverallComments,
        //                        EmpAppRating = AppRatingList
        //                    };

        //                    db.EmpAppRatingConclusion.Add(Appcategory);
        //                    //var rtn_Obj = DBTrackFile.DBTrackSave("Appraisal/Appraisal", null, db.ChangeTracker, p.DBTrack);
        //                    //     db.SaveChanges();

        //                    List<EmpAppRatingConclusion> EmpAppRatingConcl = new List<EmpAppRatingConclusion>();
        //                    EmpAppRatingConcl.Add(Appcategory);
        //                    //   db.SaveChanges();


        //               //   var eavn = db.EmpAppEvaluation.Include(a => a.AppraisalPeriodCalendar).Include(a => a.EmpAppRatingConclusion).Where(a => a.Id == evalids).SingleOrDefault();
        //                  //  var AppAsslist1 = db.AppAssignment.Include(e => e.AppCategory).Include(e => e.AppSubCategory)
        //                   //              .Where(e => e.Id == evalids).SingleOrDefault();
        //                    //EmpAppEvaluation empappeval = new EmpAppEvaluation()
        //                    //{

        //                    //};
        //                    //if (eavn != null)
        //                    //{
        //                    //    eavn.SecurePoints = RatingPoints;
        //                    //    eavn.DBTrack = p.DBTrack;
        //                    //    eavn.EmpAppRatingConclusion = EmpAppRatingConcl,
        //                    //    db.EmpAppEvaluation.Attach(eavn);
        //                    //    db.Entry(eavn).State = System.Data.Entity.EntityState.Modified;
        //                    //    db.SaveChanges();
        //                    //}


        //                    EmpAppEvaluation evp = new EmpAppEvaluation()
        //                    {
        //                        EmpAppRatingConclusion = EmpAppRatingConcl,
        //                        SecurePoints = RatingPoints,
        //                      //AppraisalPeriodCalendar = ,
        //                        MaxPoints = maxpoint,
        //                       DBTrack = p.DBTrack,
        //                    };
        //                    //db.EmpAppEvaluation.Add(evp);


        //                    //db.SaveChanges();

        //                    //      var EmployeeAppraisal = new EmployeeAppraisal();


        //                    List<EmpAppEvaluation> AppcategoryLost = new List<EmpAppEvaluation>();
        //                    AppcategoryLost.Add(evp);



        //                    var empappr = db.EmployeeAppraisal.Where(a => a.Employee.Id == q1.Id).SingleOrDefault();
        //                    if (db.EmployeeAppraisal.Any(o => o.Employee.Id == q1.Id))
        //                    {

        //                        empappr.EmpAppEvaluation = AppcategoryLost;
        //                        empappr.DBTrack = p.DBTrack;
        //                        db.EmployeeAppraisal.Attach(empappr);
        //                        db.Entry(empappr).State = System.Data.Entity.EntityState.Modified;
        //                        //    db.SaveChanges();

        //                        //Msg.Add("Code Already Exists.");
        //                        //return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                    }
        //                    else
        //                    {
        //                        empappr.DBTrack = p.DBTrack;
        //                        empappr.Employee = q1;
        //                        empappr.EmpAppEvaluation = AppcategoryLost;
        //                        db.EmployeeAppraisal.Add(empappr);
        //                    }

        //                    db.SaveChanges();

        //                    //if (db.EmployeeAppraisal.Any(o => o.Employee.Id == q1.Id))
        //                    //{
        //                    //    EmployeeAppraisal.EmpAppRatingConclusion = EmpAppRatingConcl;
        //                    //    db.EmployeeAppraisal.Attach(EmployeeAppraisal);
        //                    //    db.Entry(EmployeeAppraisal).State = System.Data.Entity.EntityState.Modified;

        //                    //}
        //                    //else
        //                    //{
        //                    //    EmployeeAppraisal.DBTrack = p.DBTrack;
        //                    //    EmployeeAppraisal.Employee = q1;
        //                    //    EmployeeAppraisal.EmpAppRatingConclusion = EmpAppRatingConcl;
        //                    //    db.EmployeeAppraisal.Add(EmployeeAppraisal);

        //                    //}

        //                    db.SaveChanges();
        //                    ts.Complete();
        //                    Msg.Add("Data Saved Successfully.");
        //                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            LogFile Logfile = new LogFile();
        //            ErrorLog Err = new ErrorLog()
        //            {
        //                ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
        //                ExceptionMessage = e.Message,
        //                ExceptionStackTrace = e.StackTrace,
        //                LineNo = Convert.ToString(new System.Diagnostics.StackTrace(e, true).GetFrame(0).GetFileLineNumber()),
        //                LogTime = DateTime.Now
        //            };
        //            Logfile.CreateLogFile(Err);
        //            Msg.Add(e.Message);
        //            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //}

        [HttpPost]
        public ActionResult Create(FormCollection form, string forwarddata) //Create submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                string Emp = form["Employee-Table"] == null ? null : form["Employee-Table"];
                string AppraisalCalendar = form["AppCalendardrop"] == null ? null : form["AppCalendardrop"];

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
                        //List<string> Msgu = new List<string>();
                        //Msgu.Add("  Kindly select employee  ");
                        //return Json(new Utility.JsonReturnClass { success = false, responseText = Msgu }, JsonRequestBehavior.AllowGet);
                        return Json(new Object[] { "", "", "Kindly select employee " }, JsonRequestBehavior.AllowGet);
                        // return Json(new { sucess = true, responseText = "Kindly select employee ." }, JsonRequestBehavior.AllowGet);
                    }
                    if (AssistanceOverallCo != null)
                    {
                        p.AssistanceOverallComments = AssistanceOverallCo.ToString();
                    }

                    var serialize = new JavaScriptSerializer();

                    var obj = serialize.Deserialize<List<DeserializeClass>>(forwarddata);

                    if (obj == null || obj.Count < 0)
                    {
                        //   return Json(new { sucess = true, responseText = "You have to change record to update." }, JsonRequestBehavior.AllowGet);
                        return Json(new Object[] { "", "", "You have to change record to update" }, JsonRequestBehavior.AllowGet);

                    }
                    List<int> ids = obj.Select(e => int.Parse(e.Id)).ToList();
                    List<int> idss = obj.Select(e => int.Parse(e.Id)).ToList();
                    List<int> idss2 = obj.Select(e => int.Parse(e.EmpEvaluationId)).ToList();
                    var evalids = Convert.ToInt32(idss2[0]);
                    var iidd = Convert.ToInt32(idss[0]);

                    var iid = Convert.ToInt32(ids1[0]);
                    var q1 = db.Employee.Where(q => q.Id == (iid)).SingleOrDefault();

                    if (AppAssistance != null && AppAssistance != "")
                    {
                        //LookupValue Lkval = db.LookupValue.Find(Convert.ToInt32(AppAssistance));
                        //p.AppraisalAssistance = Lkval;

                        LookupValue val = db.LookupValue.Find(int.Parse(AppAssistance));
                        p.AppraisalAssistance = val;
                    }

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
                                // var ep = db.EmpAppEvaluation.Include(a => a.EmpAppRatingConclusion.Select(b => b.EmpAppRating.Select(c => c.AppAssignment).Where(f => f.Id == AppA.Id))).SingleOrDefault();
                                RatingPoints = Convert.ToInt32(obj.Where(e => e.Id == ca.ToString()).Select(e => e.RatingPoints).Single());
                                var AppRatingObj = AppA.AppRatingObjective.OrderBy(e => e.RatingPoints).ToList();

                                LookupValue wording = null;
                                foreach (var a in AppRatingObj)
                                {
                                    if (RatingPoints == a.RatingPoints)
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
                            db.SaveChanges();

                            List<EmpAppRatingConclusion> EmpAppRatingConcl = new List<EmpAppRatingConclusion>();
                            //   db.SaveChanges();
                            EmpAppRatingConcl.Add(db.EmpAppRatingConclusion.Find(Appcategory.Id));

                            var eavn = db.EmpAppEvaluation.Include(a => a.AppraisalPeriodCalendar).Include(a => a.EmpAppRatingConclusion).Where(a => a.Id == evalids).SingleOrDefault();


                            //EmpAppEvaluation empappeval = new EmpAppEvaluation()
                            //{

                            //};
                            if (eavn != null)
                            {

                                if (eavn.EmpAppRatingConclusion != null)
                                {
                                    if (eavn.EmpAppRatingConclusion.Any(r => r.AppraisalAssistance.Id == p.AppraisalAssistance.Id))
                                    {
                                        Msg.Add("Record already exist.");
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    }
                                    EmpAppRatingConcl.AddRange(eavn.EmpAppRatingConclusion);
                                }

                                eavn.SecurePoints = RatingPoints;
                                eavn.DBTrack = p.DBTrack;
                                eavn.EmpAppRatingConclusion = EmpAppRatingConcl;
                                db.EmpAppEvaluation.Attach(eavn);
                                db.Entry(eavn).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                            }


                            //EmpAppEvaluation evp = new EmpAppEvaluation()
                            //{
                            //    EmpAppRatingConclusion = EmpAppRatingConcl,
                            //    SecurePoints = RatingPoints,
                            //    AppraisalPeriodCalendar = eavn.AppraisalPeriodCalendar,
                            //    MaxPoints = eavn.MaxPoints,
                            //    DBTrack = eavn.DBTrack
                            //};
                            //db.EmpAppEvaluation.Add(evp);


                            //db.SaveChanges();

                            //      var EmployeeAppraisal = new EmployeeAppraisal();


                            //List<EmpAppEvaluation> AppcategoryLost = new List<EmpAppEvaluation>();
                            //AppcategoryLost.Add(evp);



                            //var empappr = db.EmployeeAppraisal.Where(a => a.Employee.Id == q1.Id).SingleOrDefault();
                            //if (db.EmployeeAppraisal.Any(o => o.Employee.Id == q1.Id))
                            //{

                            //    empappr.EmpAppEvaluation = AppcategoryLost;
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
                            //    empappr.EmpAppEvaluation = AppcategoryLost;
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
                            //   Msg.Add("Data Saved Successfully.");
                            //return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new { success = true, responseText = "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { "", "", "Data Saved Successfully" }, JsonRequestBehavior.AllowGet);
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

        [HttpPost]
        public ActionResult EditSave(FormCollection form, string forwarddata) //Create submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                string Emp = form["Employee-Table"] == null ? null : form["Employee-Table"];
                string AppraisalCalendar = form["AppCalendardrop"] == null ? null : form["AppCalendardrop"];

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
                        //List<string> Msgu = new List<string>();
                        //Msgu.Add("  Kindly select employee  ");
                        //return Json(new Utility.JsonReturnClass { success = false, responseText = Msgu }, JsonRequestBehavior.AllowGet);
                        return Json(new Object[] { "", "", "Kindly select employee " }, JsonRequestBehavior.AllowGet);
                        // return Json(new { sucess = true, responseText = "Kindly select employee ." }, JsonRequestBehavior.AllowGet);
                    }
                    if (AssistanceOverallCo != null)
                    {
                        p.AssistanceOverallComments = AssistanceOverallCo.ToString();
                    }

                    var serialize = new JavaScriptSerializer();

                    var obj = serialize.Deserialize<List<DeserializeClass>>(forwarddata);

                    if (obj == null || obj.Count < 0)
                    {
                        //   return Json(new { sucess = true, responseText = "You have to change record to update." }, JsonRequestBehavior.AllowGet);
                        return Json(new Object[] { "", "", "You have to change record to update" }, JsonRequestBehavior.AllowGet);

                    }
                    List<int> ids = obj.Select(e => int.Parse(e.Id)).ToList();
                    List<int> idss = obj.Select(e => int.Parse(e.Id)).ToList();
                    List<int> idss2 = obj.Select(e => int.Parse(e.EmpEvaluationId)).ToList();
                    List<int> maxPNT = obj.Select(e => int.Parse(e.MaxPoints)).ToList();
                    var maxpoint = Convert.ToInt32(maxPNT[0]);
                    var evalids = Convert.ToInt32(idss2[0]);
                    var iidd = Convert.ToInt32(idss[0]);

                    var iid = Convert.ToInt32(ids1[0]);
                    var q1 = db.Employee.Where(q => q.Id == (iid)).SingleOrDefault();

                    if (AppAssistance != null && AppAssistance != "")
                    {
                        //LookupValue Lkval = db.LookupValue.Find(Convert.ToInt32(AppAssistance));
                        //p.AppraisalAssistance = Lkval;

                        LookupValue val = db.LookupValue.Find(int.Parse(AppAssistance));
                        p.AppraisalAssistance = val;
                    }
                    else
                    {
                        return Json(new Object[] { "", "", "You have to select Appraisal Assistance" }, JsonRequestBehavior.AllowGet);
                    }

                    //var eavn = db.EmpAppEvaluation.Include(a => a.AppraisalPeriodCalendar).Include(a => a.EmpAppRatingConclusion).Include(e => e.EmpAppRatingConclusion.Select(r => r.AppraisalAssistance))
                    //           .Where(a => a.Id == evalids).SingleOrDefault();
                    //if (eavn != null)
                    //{
                    //    if (eavn.EmpAppRatingConclusion != null)
                    //    {
                    //        if (eavn.EmpAppRatingConclusion.Any(r => r.AppraisalAssistance.Id == p.AppraisalAssistance.Id))
                    //        {
                    //            Msg.Add("Record already exist.");
                    //            return Json(new Object[] { "false", "", Msg }, JsonRequestBehavior.AllowGet);
                    //        }
                    //        // EmpAppRatingConcl.AddRange(eavn.EmpAppRatingConclusion);
                    //    }

                    //    //eavn.SecurePoints = RatingPoints;
                    //    //eavn.DBTrack = p.DBTrack;
                    //    //eavn.EmpAppRatingConclusion = EmpAppRatingConcl;
                    //    //db.EmpAppEvaluation.Attach(eavn);
                    //    //db.Entry(eavn).State = System.Data.Entity.EntityState.Modified;
                    //    //db.SaveChanges();
                    //}
                    var AppraisalCalendarr = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "AppraisalCalendar".ToUpper() && e.Default == true).SingleOrDefault();

                    var dataExistchk = db.EmpAppEvaluation.Include(e => e.AppraisalPeriodCalendar).Include(e => e.AppraisalSchedule)
                                         .Include(e => e.EmployeeAppraisal).Include(e => e.AppraisalSchedule)
                                         .Include(e => e.EmpAppRatingConclusion)
                                         .Include(e => e.EmpAppRatingConclusion.Select(r => r.AppraisalAssistance))
                                         .Include(e => e.EmpAppRatingConclusion.Select(a => a.EmpAppRating))

                                         .Include(e => e.EmpAppRatingConclusion.Select(a => a.EmpAppRating.Select(s => s.AppAssignment)))
                                         .Where(e => e.AppraisalPeriodCalendar.Id == AppraisalCalendarr.Id).ToList();
                    AppraisalSchedule AppSchedule = null;
                    foreach (var itemchk in dataExistchk)
                    {
                        if (itemchk.EmployeeAppraisal != null && itemchk.EmployeeAppraisal.Count() > 0)
                        {
                            foreach (var item in itemchk.EmployeeAppraisal)
                            {
                                var empAppIds = ids1.Contains(item.Id);


                                foreach (var itemCon in itemchk.EmpAppRatingConclusion)
                                {
                                    //var AppAssist = itemCon.AppraisalAssistance.Id == val.Id;

                                    if ((empAppIds == true) && (itemCon.AppraisalAssistance.Id == p.AppraisalAssistance.Id))
                                    {
                                        //return Json(new Utility.JsonClass { status = false, responseText = " AppraiserSanction for this employee already done. " }, JsonRequestBehavior.AllowGet);

                                        Msg.Add("Record already exist.");
                                        return Json(new Object[] { "false", "", Msg }, JsonRequestBehavior.AllowGet);
                                    }
                                }
                            }
                        }

                    }


                    var empAPP = db.EmployeeAppraisal.Include(e => e.EmpAppEvaluation)
                                .Include(e => e.EmpAppEvaluation.Select(a => a.AppraisalSchedule))
                                .Where(e => e.Id == iid).FirstOrDefault().EmpAppEvaluation;
                    if (empAPP != null)
                    {
                        foreach (var itema in empAPP)
                        {
                            AppSchedule = itema.AppraisalSchedule;
                        }
                    }


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
                                // var ep = db.EmpAppEvaluation.Include(a => a.EmpAppRatingConclusion.Select(b => b.EmpAppRating.Select(c => c.AppAssignment).Where(f => f.Id == AppA.Id))).SingleOrDefault();
                                RatingPoints = Convert.ToInt32(obj.Where(e => e.Id == ca.ToString()).Select(e => e.RatingPoints).Single());
                                var AppRatingObj = AppA.AppRatingObjective.OrderBy(e => e.RatingPoints).ToList();

                                LookupValue wording = null;
                                foreach (var a in AppRatingObj)
                                {
                                    if (RatingPoints == a.RatingPoints)
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
                                AppraisalAssistance = p.AppraisalAssistance,
                                DBTrack = p.DBTrack,
                                AssistanceOverallComments = p.AssistanceOverallComments,
                                EmpAppRating = AppRatingList,
                                IsTrClose = true
                            };

                            db.EmpAppRatingConclusion.Add(Appcategory);
                            db.SaveChanges();

                            List<EmpAppRatingConclusion> EmpAppRatingConcl = new List<EmpAppRatingConclusion>();
                            EmpAppRatingConcl.Add(db.EmpAppRatingConclusion.Find(Appcategory.Id));

                            List<EmpAppEvaluation> EmpappEvalList = new List<EmpAppEvaluation>();

                            var EmpAppraisaldata = db.EmployeeAppraisal.Include(e => e.EmpAppEvaluation)
                                .Include(e => e.EmpAppEvaluation.Select(c => c.EmpAppRatingConclusion))
                                .Where(e => e.Id == iid).FirstOrDefault();

                            List<EmpAppEvaluation> GetempappEVAL = null;

                            #region Code For many-many ICollection of EmpAppEvaluation w.r.t. EmployeeAppraisal By Anandrao
                            //if (EmpAppraisaldata != null)  //// Code For many-many ICollection of EmpAppEvaluation w.r.t. EmployeeAppraisal By Anandrao
                            //{
                            //    GetempappEVAL = EmpAppraisaldata.EmpAppEvaluation.ToList();
                            //    if (GetempappEVAL.Count() > 0 && GetempappEVAL != null)
                            //    {
                            //        EmpappEvalList.AddRange(GetempappEVAL);

                            //        var empcon = GetempappEVAL.Where(e => e.EmpAppRatingConclusion.Count() > 0).Select(c => c.EmpAppRatingConclusion).ToList();
                            //        if (empcon.Count() > 0 && empcon != null)
                            //        {
                            //            EmpAppEvaluation empAppEvalObj = new EmpAppEvaluation()
                            //            {
                            //                EmpAppRatingConclusion = EmpAppRatingConcl,
                            //                SecurePoints = RatingPoints,
                            //                AppraisalPeriodCalendar = AppraisalCalendarr,
                            //                AppraisalSchedule = AppSchedule,
                            //                MaxPoints = maxpoint,
                            //                DBTrack = p.DBTrack,
                            //                IsTrClose = true
                            //            };
                            //            db.EmpAppEvaluation.Add(empAppEvalObj);
                            //            db.SaveChanges();
                            //            EmpappEvalList.Add(empAppEvalObj);
                            //        }
                            //        else
                            //        {
                            //            foreach (var itemT in EmpappEvalList)
                            //            {
                            //                itemT.EmpAppRatingConclusion = EmpAppRatingConcl;
                            //                itemT.SecurePoints = RatingPoints;
                            //                itemT.IsTrClose = true;
                            //            }

                            //        }

                            //    }

                            //}
                            #endregion

                            if (EmpAppraisaldata != null)
                            {
                                GetempappEVAL = EmpAppraisaldata.EmpAppEvaluation.ToList();
                                if (GetempappEVAL.Count() > 0 && GetempappEVAL != null)
                                {
                                    // EmpappEvalList.AddRange(GetempappEVAL);

                                    List<EmpAppRatingConclusion> EmpAppRatingConclListRange = new List<EmpAppRatingConclusion>();

                                    foreach (var itemEAE in GetempappEVAL)
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
                                            EmpappEvalList.Add(itemEAE);
                                        }

                                    }


                                }

                            }


                            var empappr = db.EmployeeAppraisal.Where(a => a.Employee.Id == q1.Id).SingleOrDefault();
                            if (db.EmployeeAppraisal.Any(o => o.Employee.Id == q1.Id))
                            {
                                empappr.EmpAppEvaluation = EmpappEvalList;
                                empappr.DBTrack = p.DBTrack;
                                db.EmployeeAppraisal.Attach(empappr);
                                db.Entry(empappr).State = System.Data.Entity.EntityState.Modified;

                            }
                            else
                            {
                                empappr.DBTrack = p.DBTrack;
                                empappr.Employee = q1;
                                empappr.EmpAppEvaluation = EmpappEvalList;
                                db.EmployeeAppraisal.Add(empappr);
                            }

                            db.SaveChanges();
                            ts.Complete();
                            //   Msg.Add("Data Saved Successfully.");
                            //return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new { success = true, responseText = "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
                            return Json(new Object[] { "true", "", "Data Saved Successfully" }, JsonRequestBehavior.AllowGet);

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


        public class returnDataClass
        {

            public double RatingPoints { get; set; }
            public string Appriasalassistance { get; set; }
            public string Comments { get; set; }

        }

        public ActionResult GridEditData(int data)
        {
            var returnlist = new List<returnDataClass>();
            using (DataBaseContext db = new DataBaseContext())
            {

                if (data != 0)
                {
                    var retrundataList = db.EmpAppRatingConclusion.Include(e => e.EmpAppRating).Where(e => e.Id == data).SingleOrDefault();
                    //  var returnl = retrundataList.Select(a => a.EmpAppRating).ToList();
                    var rp = retrundataList.EmpAppRating.Select(b => new { b.RatingPoints, b.Comments }).SingleOrDefault();
                    returnlist.Add(new returnDataClass()
                    {
                        // Appriasalassistance = a.AppraisalAssistance.LookupVal.ToString(),
                        RatingPoints = rp.RatingPoints,
                        Comments = rp.Comments
                    });

                    return Json(new { returndata = returnlist }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return View();
                }
            }
        }

        public ActionResult GridEditSave1(EmpAppRatingConclusion ypay, FormCollection form, string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                var ids = Utility.StringIdsToListIds(data);
                var rat = form["RatingPoints"] == null ? "" : form["RatingPoints"];
                var com = form["Comments"] == null ? "" : form["Comments"];
                Employee OEmployee = null;
                EmployeePayroll OEmployeePayroll = null;

                var blog = db.EmpAppRatingConclusion.Include(a => a.EmpAppRating).Where(a => a.Id.ToString() == data).SingleOrDefault();

                //OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct).Include(e => e.PayStruct.JobStatus)
                //                .Include(e => e.EmpOffInfo)
                //                .Where(r => r.Id == Emp).SingleOrDefault();
                int empidintrnal = Convert.ToInt32(ids[0]);
                int empidMain = Convert.ToInt32(ids[1]);
                OEmployeePayroll = db.EmployeePayroll.Include(q => q.SalaryT).Where(e => e.Id == empidMain).SingleOrDefault();
                var mnth = db.PerkTransT.Where(e => e.Id == empidintrnal).Select(q => q.PayMonth).SingleOrDefault();
                var calf = OEmployeePayroll.SalaryT.Any(e => e.PayMonth == mnth);
                if (calf == true)
                {
                    Msg.Add("  Salary Already generated for tHis Employee. Now you cannot make any changes. ");
                    return Json(new { status = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }

                if (data != null)
                {
                    var id = Convert.ToInt32(ids[0]);
                    var parent = Convert.ToInt32(ids[1]);
                    var db_data = db.PerkTransT.Include(e => e.SalaryHead).Where(e => e.Id == id).SingleOrDefault();

                    var OEmployeePayroll1 = db.EmployeePayroll.Include(e => e.PerkTransT).ToList();


                    foreach (var a in OEmployeePayroll1)
                    {
                        foreach (var b in a.PerkTransT)
                        {

                            if (b.Id == id)
                            {

                                var SalT = db.EmployeePayroll.Where(e => e.Id == a.Id).Select(e => e.SalaryT.Where(r => r.ReleaseDate != null).FirstOrDefault()).SingleOrDefault();
                                if (SalT != null)
                                {
                                    return Json(new { status = false, data = db_data, responseText = "Salary is released for this month. You can't change amount now..!" }, JsonRequestBehavior.AllowGet);
                                    // return Json(new { status = false, responseText = "Salary is released for this month. You can't change amount now." }, JsonRequestBehavior.AllowGet);

                                }
                            }

                        }
                    }

                    //  db_data.ActualAmount = ypay.ActualAmount;
                    //   db_data.ProjectedAmount = ypay.ProjectedAmount;

                    try
                    {
                        db.PerkTransT.Attach(db_data);
                        db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;
                        return Json(new { status = true, data = db_data, responseText = "Record Updated..!" }, JsonRequestBehavior.AllowGet);
                    }
                    catch (Exception e)
                    {

                        return Json(new { status = false, responseText = e.InnerException.ToString() }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(new { status = false, responseText = "Data Is Null" }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        public ActionResult GridDelete(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                int empidintrnal = 0;
                int empidMain = 0;
                if (data != null)
                {
                    var ids = Utility.StringIdsToListIds(data);

                    empidintrnal = Convert.ToInt32(ids[0]);
                    empidMain = Convert.ToInt32(ids[1]);
                }
                var LvEP = db.EmpAppRating.Find(empidintrnal);
                db.EmpAppRating.Remove(LvEP);
                db.SaveChanges();
                //return Json(new Object[] { "", "", " Record Deleted Successfully " }, JsonRequestBehavior.AllowGet);
                List<string> Msgr = new List<string>();
                Msgr.Add("Record Deleted Successfully  ");
                return Json(new Utility.JsonReturnClass { success = true, responseText = Msgr }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GridEditSave(EmpAppRating ITP, FormCollection form, string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                int empidMain = 0;
                if (data != null)
                {
                    empidMain = Convert.ToInt32(data);
                }
                var rat = form["RatingPoints"] == null ? "" : form["RatingPoints"];
                var com = form["Comments"] == null ? "" : form["Comments"];
                // var Appriasalassi = form["Appriasalassistance"] == null ? "" : form["Appriasalassistance"];
                //    var dat = form["data"] == null ? "" : form["data"];
                //  var blog = db.EmpAppRating.Include(a => a.AppAssignment).Where(a => a.Id == empidintrnal).SingleOrDefault();

                var blog = db.EmpAppRatingConclusion.Include(a => a.EmpAppRating)
                    .Include(e => e.EmpAppRating.Select(r => r.AppAssignment)).Where(a => a.Id == empidMain).SingleOrDefault();
                var OEmpAppRating = blog.EmpAppRating.FirstOrDefault();

                ITP.RatingPoints = int.Parse(rat);
                if (ITP.RatingPoints > OEmpAppRating.AppAssignment.MaxRatingPoints)
                {
                    return Json(new { status = false, responseText = "Rating Points Should Be Less Than Max Points..!" }, JsonRequestBehavior.AllowGet);
                }
                ITP.Comments = com;
                if (blog != null)
                {
                    //var id = Convert.ToInt32(dat);
                    var db_data = db.EmpAppRating.Where(e => e.Id == OEmpAppRating.Id).SingleOrDefault();
                    db_data.RatingPoints = ITP.RatingPoints;
                    db_data.Comments = ITP.Comments;

                    try
                    {
                        db.EmpAppRating.Attach(db_data);
                        db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;
                        return Json(new { status = true, data = db_data, responseText = "Record Updated..!" }, JsonRequestBehavior.AllowGet);
                        //  return Json(new { data = db_data, responseText = "Record Updated..!" }, JsonRequestBehavior.AllowGet);

                    }
                    catch (Exception e)
                    {

                        throw e;
                    }
                }

                else
                {
                    return Json(new { responseText = "Data Is Null" }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        public ActionResult PopulateDropDownListCalendar(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var qurey = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "APPRAISALCALENDAR" && e.Default == true).ToList();
                //  var qurey = db.Calendar.Include(e=>e.Name).ToList();
                var selected = "";
                if (!string.IsNullOrEmpty(data2))
                {
                    selected = data2;
                }
                var returndata = new SelectList(qurey, "Id", "FullDetails", selected);
                return Json(returndata, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Emp_Grid(ParamModel param, string y)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    //var all = db.EmployeeAppraisal.Include(e => e.Employee).Include(e => e.Employee.EmpName).Include(e => e.Employee.ServiceBookDates)
                    //    .Include(e => e.Employee.GeoStruct).Include(e => e.Employee.GeoStruct.Location.LocationObj).Include(e => e.Employee.FuncStruct)
                    //    .Include(e => e.Employee.FuncStruct.Job).Include(e => e.Employee.PayStruct).Include(e => e.Employee.PayStruct.Grade)
                    //    .ToList();


                    var all = db.EmployeeAppraisal.Select(r => new
                    {
                        Id = r.Id.ToString(),
                        EmpCode = r.Employee.EmpCode,
                        EmpName = r.Employee.EmpName.FullNameFML,
                        JoiningDate = r.Employee.ServiceBookDates.JoiningDate,
                        JobName = r.Employee.FuncStruct.Job.Name,
                        GradeName = r.Employee.PayStruct.Grade.Name,
                        LocDesc = r.Employee.GeoStruct.Location.LocationObj.LocDesc
                    }).ToList();
                    //IEnumerable<EmployeeAppraisal> fall;
                    if (param.sSearch == null)
                    {
                        all = all;
                    }
                    else
                    {

                        all = all.Where(e => (e.Id.Contains(param.sSearch))
                                  || (e.JoiningDate.Value.ToString("dd/MM/yyyy").Contains(param.sSearch))
                                  || (e.EmpCode.ToUpper().Contains(param.sSearch.ToUpper()))
                                  || (e.EmpName.ToUpper().Contains(param.sSearch.ToUpper()))
                                  || (e.JobName.ToUpper().Contains(param.sSearch.ToUpper()))
                                  || (e.GradeName.ToString().ToUpper().Contains(param.sSearch.ToUpper()))
                                  || (e.LocDesc.ToUpper().Contains(param.sSearch.ToUpper()))).ToList();                           
                    }

                    //for column sorting
                    //var sortindex = Convert.ToInt32(Request["iSortCol_0"]);

                    //Func<EmployeeAppraisal, string> orderfunc = (c =>
                    //                                            Convert.ToUInt32(sortindex) == 0 ? c.Id.ToString() :
                    //                                            sortindex == 1 ? c.Employee.EmpCode : "");
                    //var sortcolumn = Request["sSortDir_0"];
                    //if (sortcolumn == "asc")
                    //{
                    //    fall = fall.OrderBy(orderfunc);
                    //}
                    //else
                    //{
                    //    fall = fall.OrderByDescending(orderfunc);
                    //}
                    // Paging 
                    //var dcompanies = fall
                    //        .Skip(param.iDisplayStart).Take(param.iDisplayLength).ToList();
                    //if (dcompanies.Count == 0)
                    //{
                    List<returndatagridclass> result = new List<returndatagridclass>();
                    foreach (var item in all)
                    {
                        result.Add(new returndatagridclass
                        {
                            Id = item.Id,
                            Code = item.EmpCode == null ? "" : item.EmpCode,
                            Name = item.EmpName == null ? "" : item.EmpName,
                            JoiningDate = item.JoiningDate == null ? "" : item.JoiningDate.Value.ToString("dd/MM/yyyy"),
                            Job = item.JobName == null ? "" : item.JobName,
                            Grade = item.GradeName == null ? "" : item.GradeName,
                            Location = item.LocDesc == null ? "" : item.LocDesc
                        });
                    }
                    return Json(new
                    {
                        sEcho = param.sEcho,
                        iTotalRecords = all.Count(),
                        iTotalDisplayRecords = all.Count(),
                        data = result
                    }, JsonRequestBehavior.AllowGet);
                    //}
                    //else
                    //{
                    //    var result = from c in dcompanies

                    //                 select new[] { null, Convert.ToString(c.Id), c.Employee.EmpCode, };
                    //    return Json(new
                    //    {
                    //        sEcho = param.sEcho,
                    //        iTotalRecords = all.Count(),
                    //        iTotalDisplayRecords = fall.Count(),
                    //        data = result
                    //    }, JsonRequestBehavior.AllowGet);
                    //}
                }
                catch (Exception e)
                {
                    List<string> Msg = new List<string>();
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

        public ActionResult Get_AppAssignData(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var db_data = db.EmployeeAppraisal
                        .Include(e => e.EmpAppEvaluation)
                        .Include(e => e.EmpAppEvaluation.Select(a => a.EmpAppRatingConclusion))
                        .Include(e => e.EmpAppEvaluation.Select(a => a.EmpAppRatingConclusion.Select(s => s.AppraisalAssistance)))
                        .Include(e => e.EmpAppEvaluation.Select(a => a.EmpAppRatingConclusion.Select(s => s.EmpAppRating)))
                        .Include(e => e.EmpAppEvaluation.Select(a => a.EmpAppRatingConclusion.Select(s => s.EmpAppRating.Select(r => r.AppAssignment))))
                        .Include(e => e.EmpAppEvaluation.Select(a => a.EmpAppRatingConclusion.Select(s => s.EmpAppRating.Select(r => r.AppAssignment.AppCategory))))
                        .Include(e => e.EmpAppEvaluation.Select(a => a.EmpAppRatingConclusion.Select(s => s.EmpAppRating.Select(r => r.AppAssignment.AppSubCategory))))
                        .Include(e => e.EmpAppEvaluation.Select(a => a.EmpAppRatingConclusion.Select(s => s.EmpAppRating.Select(r => r.AppAssignment.AppRatingObjective))))
                        .Include(e => e.EmpAppEvaluation.Select(a => a.EmpAppRatingConclusion.Select(s => s.EmpAppRating.Select(r => r.AppAssignment.AppRatingObjective.Select(q => q.ObjectiveWordings)))))
                        .Where(e => e.Id == data).SingleOrDefault();
                    if (db_data.EmpAppEvaluation != null)
                    {
                        List<DeserializeClass> returndata = new List<DeserializeClass>();

                        foreach (var item in db_data.EmpAppEvaluation)
                        {
                            foreach (var item1 in item.EmpAppRatingConclusion)
                            {
                                foreach (var item2 in item1.EmpAppRating)
                                {
                                    // var obj = item2.AppAssignment.AppRatingObjective.Select(e => e.ObjectiveWordings.LookupVal.ToString()).SingleOrDefault();

                                    var ab = item2.RatingPoints;
                                    var ad = item2.AppAssignment.AppRatingObjective.ToList();
                                    foreach (var item12 in ad)
                                    {
                                        if (ab >= item12.RatingPointsFrom && ab <= item12.RatingPointsTo)
                                        {
                                            var objj = item12.ObjectiveWordings.LookupVal.ToString();

                                            returndata.Add(new DeserializeClass
                                            {
                                                Id = item1.Id.ToString(),
                                                EmpAppRatingId = item2.Id.ToString(),
                                                Category = item2.AppAssignment == null ? "" : item2.AppAssignment.AppCategory == null ? "" : item2.AppAssignment.AppCategory.Name,
                                                SubCategory = item2.AppAssignment == null ? "" : item2.AppAssignment.AppSubCategory == null ? "" : item2.AppAssignment.AppSubCategory.Name,
                                                AppriasalAssistance = item1.AppraisalAssistance == null ? "" : item1.AppraisalAssistance.LookupVal.ToString(),
                                                Comments = item2.Comments == null ? "" : item2.Comments.ToString(),
                                                MaxPoints = item2.AppAssignment.MaxRatingPoints == null ? "" : item2.AppAssignment.MaxRatingPoints.ToString(),
                                                ObjectiveWordings = objj,
                                                RatingPoints = item2.RatingPoints == null ? "" : item2.RatingPoints.ToString(),
                                                AppraiseePoints = "0",
                                                AppraiseeComments = "",
                                                AppraiserPoints = "0",
                                                AppraiserComments = "",
                                                HRPoints = "0",
                                                HRComments = ""
                                            });
                                            break;

                                        }
                                    }
                                }
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


    }
}