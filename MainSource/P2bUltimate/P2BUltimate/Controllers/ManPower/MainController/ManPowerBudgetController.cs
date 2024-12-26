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
using P2BUltimate.Security;
using Payroll;
using Recruitment;

namespace P2BUltimate.Controllers.ManPower.MainController
{
    public class ManPowerBudgetController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();
        //
        // GET: /ManPowerBudget/
        public ActionResult Index()
        {
            return View("~/Views/ManPower/MainViews/ManPowerBudget/Index.cshtml");
        }
        public ActionResult partial()
        {
            return View("~/Views/Shared/Manpower/_YearlyManpowerBudget.cshtml");
        }
        //public ActionResult GetFullDetals(Int32 Data)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var getData = db.GeoStruct.Include(e => e.Location)
        //            .Include(e => e.Region)
        //            .Include(e => e.Unit)
        //            .Include(e => e.Company)
        //            .Include(e => e.Corporate)
        //            .Include(e => e.Department)
        //            //.Include(e => e.Location)
        //            .Where(e => e.Id == Data).SingleOrDefault();

        //        if (getData != null)
        //        {
        //            var jsonresult = new
        //            {
        //                id = getData.Id,
        //                fulldetails = getData.FullDetailsLD
        //            };
        //            return Json(new { success = true, responseText = "", data = jsonresult }, JsonRequestBehavior.AllowGet);
        //        }
        //        else
        //        {
        //            return Json(new { success = false, }, JsonRequestBehavior.AllowGet);

        //        }
        //    }


        //}
        public ActionResult GetFullDetals1(FormCollection form)
        {


            using (DataBaseContext db = new DataBaseContext())
            {
                //var Division = form["division-table"];
                //var Location = form["location-table"];
                //var Department = form["department-table"];
                //var group = form["group-table"];
                //var unit = form["unit-table"];

                //var getData = db.GeoStruct.Include(e => e.Location)
                //    .Include(e => e.Region)
                //    .Include(e => e.Unit)
                //    .Include(e => e.Company)
                //    .Include(e => e.Corporate)
                //    .Include(e => e.Department)
                //    //.Include(e => e.Location)
                //    .Where(e => e.Id != null).ToList();

                //if (Division != null)
                //{
                //    getData = getData.Where(a => a.Division.Id.ToString() == Division).ToList();
                //}
                //if (Location != null)
                // {
                //     getData = getData.Where(a => a.Location.Id.ToString() == Location).ToList();
                // }
                //if (Department != null)
                //{
                //    getData = getData.Where(a => a.Department.Id.ToString() == Department).ToList();
                // }
                // if (group != null)
                // {
                //      getData = getData.Where(a => a.Group.Id.ToString() == group).ToList();
                // }
                // if (unit != null)
                // {
                //    getData= getData.Where(a => a.Unit.Id.ToString() == unit).ToList();
                // }

                ////var div = db.GeoStruct.Where(a => a.Division.Id.ToString() == Division).FirstOrDefault();
                ////var Loc = db.GeoStruct.Where(a => a.Location.Id.ToString() == Location).FirstOrDefault();
                ////var Dep = db.GeoStruct.Where(a => a.Department.Id.ToString() == Department).FirstOrDefault();
                //if (getData != null)
                //{

                //    var b = db.GeoStruct.Include(e => e.Region)
                //    .Include(e => e.Unit)
                //    .Include(e => e.Company)
                //    .Include(e => e.Corporate)
                //    .Include(e => e.Department).Where(e => e.Id != null).FirstOrDefault();
                //    return Json(new { success = true, responseText = "", data = b }, JsonRequestBehavior.AllowGet);

                //}
                //else
                //{
                //    return Json(new { success = false, }, JsonRequestBehavior.AllowGet);

                //}


                var job = form["job-table"];
                var jobposition = form["jobposition-table"];
                var funid = form["fun_id"];
                var fundata = db.FuncStruct.Include(e => e.Job).Include(e => e.JobPosition).Where(e => e.Id != null).ToList();
                if (fundata != null)
                {
                    fundata = fundata.Where(a => a.Job.Id.ToString() == funid.ToString()).ToList();

                    return Json(new { success = true, responseText = "", data = fundata }, JsonRequestBehavior.AllowGet);


                }
                else
                {
                    return Json(new { success = false }, JsonRequestBehavior.AllowGet);
                }

            }


        }


        public ActionResult getCalendar()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var qurey = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "RECRUITMENTCALENDAR" && e.Default == true).AsEnumerable()
                    .Select(e => new
                    {
                        Id = e.Id,
                        Lvcalendardesc = "From Date :" + e.FromDate.Value.ToShortDateString() + " To Date :" + e.ToDate.Value.ToShortDateString(),

                    }).SingleOrDefault();
                return Json(qurey, JsonRequestBehavior.AllowGet);
            }
        }


        public class man
        {
            public string id { get; set; }
            public string fulldetails { get; set; }
            public string Jobposition { get; set; }

        }
        public ActionResult GetFullDetals(FormCollection form)
        {

            var funid = form["fun_id"];
            var geoid = form["geo_id"];
            List<int> ids = null;
            if (geoid != null)
            {

            }
            if (funid != "")
            {
                ids = Utility.StringIdsToListIds(funid);
            }
            else
            {
                return Json(new { success = false, responseText = "Please Select Funcstruct Filter" }, JsonRequestBehavior.AllowGet);
            }
            List<man> a = new List<man>();
            using (DataBaseContext db = new DataBaseContext())
            {
                // var funstructdata = db.FuncStruct.Include(e => e.Job).Select(e => e.Job).Distinct().ToList();
                foreach (var item in ids)
                {
                    var getData = db.FuncStruct.Include(e => e.Job)
                        .Include(e => e.Job.JobPosition)
                        //.Include(e => e.Location)
                        .Where(e => e.Id.ToString() == item.ToString()).SingleOrDefault();
                    Int32 count = 2;
                    char[] spearator = { ',' };
                    String[] strlist = getData.FullDetails.Split(spearator, count);
                    string spiltstring2 = strlist[0];
                    string jobpositionname = "";
                    if (strlist.Count() > 1)
                    {
                        string spiltstring3 = strlist[1];
                        jobpositionname = spiltstring3.Remove(0, 13);
                    }
                    string Jobname = spiltstring2.Remove(0, 5);

                    if (item != null)
                    {
                        a.Add(new man
                       {
                           id = getData.Id.ToString(),
                           fulldetails = Jobname + "," + jobpositionname
                       });
                    }
                    //   TempData["b"] = a; 

                }
                // return RedirectToAction("P2BInlineGrid");
                return Json(new { success = true, responseText = "", data = a }, JsonRequestBehavior.AllowGet);

                return null;

            }


        }
        public class EmpAppRatingConclusionDetails
        {
            public int Id { get; set; }
            public string Post { get; set; }
            public int SanctionedPosts { get; set; }
            public double BudgetAmount { get; set; }
            public string Editable { get; set; }
            // public string Appriasalassistance { get; set; }
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
                List<EmpAppRatingConclusionDetails> EmpAppRatingConclusionDetailsList = new List<EmpAppRatingConclusionDetails>();
                List<int> ids = null;
                if (extraeditdata != "")
                {
                    ids = Utility.StringIdsToListIds(extraeditdata);
                }
                else
                {
                    return Json(new { success = false, responseText = "Please Select Funcstruct Filter" }, JsonRequestBehavior.AllowGet);
                }
                if (ids.Count() > 0)
                {

                    foreach (var item in ids)
                    {
                        var getData = db.FuncStruct.Include(e => e.Job)
                            .Include(e => e.Job.JobPosition)
                            //.Include(e => e.Location)
                            .Where(e => e.Id.ToString() == item.ToString()).SingleOrDefault();
                        Int32 count = 2;
                        char[] spearator = { ',' };
                        String[] strlist = getData.FullDetails.Split(spearator, count);
                        string spiltstring2 = strlist[0];
                        string jobpositionname = "";
                        if (strlist.Count() > 1)
                        {
                            string spiltstring3 = strlist[1];
                            jobpositionname = spiltstring3.Remove(0, 13);
                        }
                        string Jobname = spiltstring2.Remove(0, 5);

                        if (item != null)
                        {
                            EmpAppRatingConclusionDetailsList.Add(new EmpAppRatingConclusionDetails
                            {
                                Id = getData.Id,
                                // EmpEvaluationId = empd.Id,
                                // CatName =empd.app  AppAsslist.AppCategory.Name != null ? AppAsslist.AppCategory.Name : null,
                                Post = Jobname + "," + jobpositionname,
                                SanctionedPosts = 0,
                                BudgetAmount = 0,
                                Editable = "true"
                            });
                        }
                        //   TempData["b"] = a; 

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
                            jsonData = IE.Where(e => (e.Id.ToString().Contains(gp.searchString))
                                || (e.Post.ToString().Contains(gp.searchString.ToUpper()))
                               || (e.SanctionedPosts.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                               || (e.BudgetAmount.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                               ).Select(a => new { a.Id, a.Post, a.SanctionedPosts, a.BudgetAmount, a.Editable }).ToList();
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
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Post, a.SanctionedPosts, a.BudgetAmount, a.Editable }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = EmpAppRatingConclusionDetailsList;
                    Func<EmpAppRatingConclusionDetails, dynamic> orderfuc;

                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "Post" ? c.Post.ToString() :
                                         gp.sidx == "SanctionedPosts" ? c.SanctionedPosts.ToString() :
                                         gp.sidx == "BudgetAmount" ? c.BudgetAmount.ToString() :
                                         gp.sidx == "Editable" ? c.Editable.ToString() : "");

                    }
                    if (gp.sord == "asc")
                    {                                             //Convert.ToString(a.AppSubCategory.FullDetails),a.AppRatingObjective.Select(r=>r.ObjectiveWordings.LookupVal) !=null? Convert.ToString(a.AppRatingObjective.Select(r=>r.ObjectiveWordings.LookupVal.ToString())): null                                                                                                                                     
                        jsonData = IE.Select(a => new Object[] { a.Id, a.Post, a.SanctionedPosts, a.BudgetAmount, a.Editable }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.Post, a.SanctionedPosts, a.BudgetAmount, a.Editable }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Post, a.SanctionedPosts, a.BudgetAmount, a.Editable }).ToList();
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


        public ActionResult editdata(FormCollection form)
        {
            return null;
        }


        public ActionResult PopulateDropDownListCalendar(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var qurey = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "RECRUITMENTCALENDAR" && e.Default == true).ToList();
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

        public class DeserializeClass
        {
            public String Id { get; set; }
            public String Post { get; set; }
            public String SanctionedPosts { get; set; }
            public String BudgetAmount { get; set; }


        }
        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult Create(ManPowerBudget c, FormCollection form) //Create submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string func = form["funstruct_sanction"] == "0" ? null : form["funstruct_sanction"];
                    string funcid = form["funstruct_id"] == "0" ? null : form["funstruct_id"];
                    string funid = form["fun_id"] == "0" ? null : form["fun_id"];
                    string geoid = form["geo_id"] == "0" ? null : form["geo_id"];
                    string payid = form["pay_id"] == "0" ? null : form["pay_id"];

                    var v = func;
                    var v1 = funcid;
                    var v2 = funid;
                    var v3 = geoid;
                    var v4 = payid;

                    if (funid != null)
                    {
                        var val1 = db.FuncStruct.Find(int.Parse(funid));
                        var val2 = db.FuncStruct.Include(a => a.Job).Where(a => a.Job.Id.ToString() == funid).FirstOrDefault();
                        c.FuncStruct = val2;
                    }

                    if (geoid != null)
                    {

                        var val2 = db.GeoStruct.Include(a => a.Company).Include(a => a.Department).Include(a => a.Location).Where(a => a.Id.ToString() == geoid).FirstOrDefault();
                        c.GeoStruct = val2;
                    }

                    if (payid != null)
                    {

                        var val2 = db.PayStruct.Include(a => a.Grade).Include(a => a.JobStatus).Include(a => a.Level).Where(a => a.Id.ToString() == payid).FirstOrDefault();
                        c.PayStruct = val2;
                    }



                    string RecruitCalendardrop = form["RecruitCalendardrop"] == "0" ? null : form["RecruitCalendardrop"];
                    string salhd = form["SalaryComponentList"] == "0" ? "" : form["SalaryComponentList"];
                    //if (RecruitCalendardrop != null)
                    //{
                    //    var val = db.Calendar.Find(int.Parse(RecruitCalendardrop));
                    //    c.RecruitmentCalendar = val;
                    //}


                    //if (salhd != null)
                    //{
                    //    var ids = Utility.StringIdsToListIds(salhd);
                    //    var HolidayList = new List<ManPowerDetailsBatch>();
                    //    foreach (var item in ids)
                    //    {

                    //        int HolidayListid = Convert.ToInt32(item);
                    //        var val = db.ManPowerDetailsBatch.Include(e => e.ManPowerPostData)
                    //                            .Where(e => e.Id == HolidayListid).SingleOrDefault();
                    //        if (val != null)
                    //        {
                    //            HolidayList.Add(val);
                    //        }
                    //    }
                    //    c.ManPowerDetailsBatch = HolidayList;
                    //}

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {

                            //  c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            ManPowerBudget ctc = new ManPowerBudget()
                            {
                                //  RecruitmentCalendar = c.RecruitmentCalendar,
                                BudgetAmount = c.BudgetAmount,
                                SanctionedPosts = int.Parse(func),
                                // ManPowerDetailsBatch = c.ManPowerDetailsBatch,
                                FuncStruct = c.FuncStruct
                                //   DBTrack = c.DBTrack
                            };
                            try
                            {
                                db.ManPowerBudget.Add(ctc);
                                //   var rtn_Obj = DBTrackFile.DBTrackSave("ManPower", null, db.ChangeTracker,"");
                                //  DT_CtcDefinition DT_Corp = (DT_Corporate)rtn_Obj;

                                //   db.Create(DT_Corp);
                                db.SaveChanges();
                                //DBTrackFile.DBTrackSave("Core/P2b.Global", "C", corporate, null, "Corporate", null);


                                ts.Complete();
                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                //  return this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                return RedirectToAction("Create", new { concurrencyError = true, id = c.Id });
                            }
                            catch (DataException /* dex */)
                            {
                                //  return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });

                                Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

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

        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult Process(ManPowerBudget c, FormCollection form, String forwarddata, String selected) //Create submit
        {
            List<string> Msg = new List<string>();
            List<string> jobm = new List<string>();
            try
            {
                //var serialize = new JavaScriptSerializer();
                //var obj = serialize.Deserialize<List<DeserializeClass>>(forwarddata);

                using (DataBaseContext db = new DataBaseContext())
                {


                    //if (obj == null || obj.Count < 0)
                    //{
                    //    return Json(new { success = false, responseText = "Cant update" }, JsonRequestBehavior.AllowGet);
                    //}

                    //List<int> b = obj.Select(e => int.Parse(e.Id)).ToList();
                    var serialize = new JavaScriptSerializer();
                    var obj = serialize.Deserialize<List<DeserializeClass>>(forwarddata);
                    if (obj == null || obj.Count < 0)
                    {
                        return Json(new { success = false, responseText = "You have to change amount to update salary structure." }, JsonRequestBehavior.AllowGet);
                    }

                    //  List<int> b = 


                    string forword = form["forwarddata"] == "0" ? null : form["forwarddata"];
                    string sel = form["selected"] == "0" ? null : form["selected"];
                    string funcid = form["funstruct_id"] == "0" ? null : form["funstruct_id"];
                    string sanpost = form["funstruct_sanction"] == "0" ? null : form["funstruct_sanction"];
                    string funid = form["fun_id"] == "0" ? null : form["fun_id"];
                    string geoid = form["geo_id"] == "0" ? null : form["geo_id"];
                    string payid = form["pay_id"] == "0" ? null : form["pay_id"];
                    string BudgetAmount = form["Budget_amount"] == "0" ? null : form["Budget_amount"];

                    if (BudgetAmount == "")
                    {
                        Msg.Add(" Enter Budget Amount  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }
                    // var v = func;
                    var v1 = funcid;
                    var v2 = funid;
                    var v3 = geoid;
                    var v4 = payid;

                    FuncStruct funct = null;

                    List<int> ids1 = obj.Select(e => int.Parse(e.Id)).ToList();
                    List<int> BudgetamountList = obj.Select(e => int.Parse(e.BudgetAmount)).ToList();
                    List<int> ids2 = obj.Select(e => int.Parse(e.SanctionedPosts)).ToList();

                    //if (funid != "")
                    //{
                    //    ids1 = Utility.StringIdsToListIds(funid);
                    //}

                    //if (BudgetAmount != "")
                    //{
                    //    BudgetamountList = Utility.StringIdsToListIds(BudgetAmount);
                    //}

                    //if (funcid != "")
                    //{
                    //    ids1 = Utility.StringIdsToListIds(funcid);
                    //}

                    //if (sanpost != null || sanpost != "0" || sanpost != "")
                    //{
                    //    ids2 = Utility.StringIdsToListIds(sanpost);
                    //}

                    List<int> ids3 = null;
                    if (geoid != "")
                    {
                        ids3 = Utility.StringIdsToListIds(geoid);
                    }

                    //List<int> ids4 = null;
                    //if (payid != "")
                    //{
                    //    ids4 = Utility.StringIdsToListIds(payid);
                    //}

                    //   jobm.Add(val2);

                    //if (funid != null)
                    //{
                    //    var val1 = db.FuncStruct.Find(int.Parse(funid));
                    //    var val2 = db.FuncStruct.Include(a => a.Job).Where(a => a.Job.Id.ToString() == funid).FirstOrDefault();
                    //    c.FuncStruct = val2;
                    //}
                    //if (ids3 != null)
                    //{
                    //    foreach (var item in ids3)
                    //    {
                    //        var val2 = db.GeoStruct.Include(a => a.Company).Include(a => a.Department).Include(a => a.Location).Where(a => a.Id == item).FirstOrDefault();
                    //        c.GeoStruct = val2;

                    //    }
                    //}
                    //if (ids4 != null)
                    //{
                    //    foreach (var item in ids4)
                    //    {

                    //        var val2 = db.PayStruct.Include(a => a.Grade).Include(a => a.JobStatus).Include(a => a.Level).Where(a => a.Id == item).FirstOrDefault();
                    //        c.PayStruct = val2;
                    //    }
                    //}

                    //string RecruitCalendardrop = form["dispcalender_id"] == "0" ? null : form["dispcalender_id"];
                    //   string RecruitCalendardrop1 = form["dispcalender"] == "0" ? null : form["dispcalender"];
                    string RecruitCalendardrop2 = form["dispcalender_id"] == "0" ? null : form["dispcalender_id"];
                    //  string RecruitCalendardrop3 = form["RecruitCalendardrop"] == "0" ? null : form["RecruitCalendardrop"];
                    string salhd = form["SalaryComponentList"] == "0" ? "" : form["SalaryComponentList"];
                    RecruitYearlyCalendar yearlyrecruitmentCalendar = null;
                    if (RecruitCalendardrop2 != null)
                    {
                        var val = db.Calendar.Find(int.Parse(RecruitCalendardrop2));
                        yearlyrecruitmentCalendar = db.RecruitYearlyCalendar
                            .Include(e => e.RecruitmentCalendar)
                            .Include(e => e.ManPowerBudget).Where(e => e.RecruitmentCalendar.Id == val.Id).SingleOrDefault();
                    }
                    if (ids3.Count() > 0)
                    {
                        foreach (var item in ids3)
                        {
                            var val2 = db.GeoStruct
                                .Include(e => e.Location)
                                .Include(e => e.Location.LocationObj)
                                .Where(a => a.Id == item).FirstOrDefault();
                            c.GeoStruct = val2;

                            var dataa = db.ManPowerBudget
                                //.Include(a => a.RecruitmentCalendar)
                                .Include(a => a.FuncStruct)
                                   .Include(a => a.GeoStruct)
                                   .Where(a => a.GeoStruct.Id == item).ToList();
                            if (dataa.Count > 0)
                            {
                                for (int i = 0; i < ids1.Count; i++)
                                {
                                    int j = Convert.ToInt32(ids1[i]);
                                    var funcdata = db.FuncStruct.Include(e=>e.Job).Where(a =>a.Id == j).FirstOrDefault();
                                    var funcobject = dataa.Where(e => e.FuncStruct.Id == j).ToList();
                                    if (funcobject.Count > 0)
                                    {
                                        Msg.Add(" Data has already been added for this Geostruct and Funcstruct " + val2.Location.LocationObj.LocDesc + " " + funcdata.Job.Name);

                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet); 
                                    }
                                }
                               
                            }
                        }
                    }

                    //if (salhd != null)
                    //{
                    //    var ids = Utility.StringIdsToListIds(salhd);
                    //    var HolidayList = new List<ManPowerDetailsBatch>();
                    //    foreach (var item in ids)
                    //    {

                    //        int HolidayListid = Convert.ToInt32(item);
                    //        var val = db.ManPowerDetailsBatch.Include(e => e.ManPowerPostData)
                    //                            .Where(e => e.Id == HolidayListid).SingleOrDefault();
                    //        if (val != null)
                    //        {
                    //            HolidayList.Add(val);
                    //        }
                    //    }
                    //    // c.ManPowerDetailsBatch = HolidayList;
                    //}

                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                         new System.TimeSpan(0, 30, 0)))
                        {
                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                            try
                            {
                                List<ManPowerBudget> OFAT = new List<ManPowerBudget>();
                                if (ids3.Count() > 0)
                                {

                                    foreach (var item in ids3)
                                    {
                                        //var val2 = db.GeoStruct
                                        //    .Include(e => e.Location)
                                        //    .Include(e => e.Location.LocationObj)
                                        //    .Where(a => a.Id == item).FirstOrDefault();
                                        //c.GeoStruct = val2;

                                        //var dataa = db.ManPowerBudget
                                        //    //.Include(a => a.RecruitmentCalendar)
                                        //    .Include(a => a.FuncStruct)
                                        //       .Include(a => a.GeoStruct)
                                        //       .Where(a => a.GeoStruct.Id == item).ToList();
                                        //if (dataa.Count > 0)
                                        //{
                                          
                                        //    Msg.Add(" Data Already Added For this Geostruct " + val2.Location.LocationObj.LocDesc);
                                        //    continue;
                                        //    // return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        //}

                                        for (int i = 0; i < ids1.Count; i++)
                                        {
                                            int j = Convert.ToInt32(ids1[i]);

                                            funct = (db.FuncStruct.Include(a => a.Job).Include(a => a.JobPosition).Where(a => a.Id == j).FirstOrDefault());

                                            ManPowerBudget man = new ManPowerBudget()
                                            {
                                                SanctionedPosts = ids2[i],
                                                BudgetAmount = BudgetamountList[i],
                                                FuncStruct = funct,
                                                GeoStruct = c.GeoStruct,
                                                PayStruct = c.PayStruct,
                                                DBTrack = c.DBTrack
                                            };
                                            db.ManPowerBudget.Add(man);
                                            db.SaveChanges();

                                            //     man.SanctionedPosts = Convert.ToInt32(obj.Where(e => e.Id == ca.ToString()).Select(e => e.Posts).Single());
                                            //man.SanctionedPosts = ids2[i];
                                            //man.BudgetAmount = BudgetamountList[i];
                                            //// man.ManPowerDetailsBatch = c.ManPowerDetailsBatch;
                                            ////  man.RecruitmentCalendar = c.RecruitmentCalendar;
                                            //man.FuncStruct = funct;
                                            //man.GeoStruct = c.GeoStruct;
                                            //man.PayStruct = c.PayStruct;
                                            //man.DBTrack = c.DBTrack;
                                            //db.ManPowerBudget.Attach(man);

                                            //db.Entry(man).State = System.Data.Entity.EntityState.Added;
                                            OFAT.Add(db.ManPowerBudget.Find(man.Id));

                                        }
                                    }
                                    if (yearlyrecruitmentCalendar.ManPowerBudget != null)
                                    {
                                        OFAT.AddRange(yearlyrecruitmentCalendar.ManPowerBudget);
                                    }
                                    yearlyrecruitmentCalendar.ManPowerBudget = OFAT;
                                    //OEmployeePayroll.DBTrack = dbt;
                                    db.RecruitYearlyCalendar.Attach(yearlyrecruitmentCalendar);
                                    db.Entry(yearlyrecruitmentCalendar).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    db.Entry(yearlyrecruitmentCalendar).State = System.Data.Entity.EntityState.Detached;
                                    ts.Complete();
                                    Msg.Add("  Data Saved successfully  ");
                                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            catch (Exception e)
                            {
                                throw;
                            }

                            //  c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            //ManPowerBudget ctc = new ManPowerBudget()

                            //{
                            //    RecruitmentCalendar = c.RecruitmentCalendar,
                            //    BudgetAmount = c.BudgetAmount,
                            //    SanctionedPosts = int.Parse(func),
                            //    ManPowerDetailsBatch = c.ManPowerDetailsBatch,
                            //    FuncStruct = c.FuncStruct
                            //    //   DBTrack = c.DBTrack
                            //};
                            //try
                            //{
                            //    // db.ManPowerBudget.Add(ctc);
                            //    //   var rtn_Obj = DBTrackFile.DBTrackSave("ManPower", null, db.ChangeTracker,"");
                            //    //  DT_CtcDefinition DT_Corp = (DT_Corporate)rtn_Obj;

                            //    //   db.Create(DT_Corp);
                            //    //    db.SaveChanges();
                            //    //DBTrackFile.DBTrackSave("Core/P2b.Global", "C", corporate, null, "Corporate", null);


                            //    ts.Complete();
                            //    Msg.Add("  Data Saved successfully  ");
                            //    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            //    //  return this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
                            //}
                            //catch (DbUpdateConcurrencyException)
                            //{
                            //    return RedirectToAction("Create", new { concurrencyError = true, id = c.Id });
                            //}
                            //catch (DataException /* dex */)
                            //{
                            //    //  return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });

                            //    Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
                            //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            //}
                            return null;
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

        public class returndatagridclass //Parentgrid
        {
            public string Id { get; set; }
            public string Division { get; set; }
            public string Location { get; set; }
            public string Department { get; set; }
        }
        public ActionResult Formula_Grid(ParamModel param, string Filterdata)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                RecruitYearlyCalendar yearlydata = null;
                List<ManPowerBudget> Sal = new List<ManPowerBudget>();
                if (Filterdata != "")
                {
                    int calendarid = Convert.ToInt32(Filterdata);
                    yearlydata = db.RecruitYearlyCalendar
                            .Include(e => e.ManPowerBudget)
                            .Include(e => e.ManPowerBudget.Select(t => t.GeoStruct))
                            .Include(e => e.ManPowerBudget.Select(t => t.GeoStruct.Division))
                            .Include(e => e.ManPowerBudget.Select(t => t.GeoStruct.Location))
                            .Include(e => e.ManPowerBudget.Select(t => t.GeoStruct.Location.LocationObj))
                            .Include(e => e.ManPowerBudget.Select(t => t.GeoStruct.Department))
                            .Include(e => e.ManPowerBudget.Select(t => t.GeoStruct.Department.DepartmentObj))
                            .Include(e => e.RecruitmentCalendar)
                            .Where(e => e.RecruitmentCalendar.Id == calendarid).SingleOrDefault();
                    Sal = yearlydata.ManPowerBudget.GroupBy(e => e.GeoStruct.Id).Select(e => e.FirstOrDefault()).ToList();
                }
                else
                {
                    Sal = db.ManPowerBudget
                         .Include(e => e.GeoStruct)
                         .Include(e => e.GeoStruct.Division)
                          .Include(e => e.GeoStruct.Location)
                         .Include(e => e.GeoStruct.Location.LocationObj)
                         .Include(e => e.GeoStruct.Department)
                         .Include(e => e.GeoStruct.Department.DepartmentObj)
                         .ToList();
                }

                try
                {
                    var id = int.Parse(Session["CompId"].ToString());
                    //var a = db.PayScaleAssignment.Include(e => e.SalHeadFormula).Include(e => e.PayScaleAgreemnt).Where(e => e.Id == data).ToList();

                    var all = Sal.GroupBy(e => e.GeoStruct.Id).Select(e => e.FirstOrDefault()).ToList();
                    // for searchs
                    IEnumerable<ManPowerBudget> fall;
                    string DependRule = "";
                    if (param.sSearch == null)
                    {
                        fall = all;

                    }
                    else
                    {
                        fall = all.Where(e => (e.Id.ToString().Contains(param.sSearch))
                            || (e.GeoStruct == null ? false : e.GeoStruct.Division == null ? false : e.GeoStruct.Division.Name.Contains(param.sSearch.ToUpper()))
                            || (e.GeoStruct == null ? false : e.GeoStruct.Location == null ? false : e.GeoStruct.Location.LocationObj == null ? false : e.GeoStruct.Location.LocationObj.LocDesc.ToString().Contains(param.sSearch))
                            || (e.GeoStruct == null ? false : e.GeoStruct.Department == null ? false : e.GeoStruct.Department.DepartmentObj == null ? false : e.GeoStruct.Department.DepartmentObj.DeptDesc.ToString().Contains(param.sSearch))
                            ).ToList();

                    }

                    //for column sorting
                    var sortindex = Convert.ToInt32(Request["iSortCol_0"]);

                    Func<ManPowerBudget, string> orderfunc = (c =>
                                                                Convert.ToUInt32(sortindex) == 0 ? c.Id.ToString() :
                                                                sortindex == 1 ? c.GeoStruct == null ? "" : c.GeoStruct.Location == null ? "" : c.GeoStruct.Location.LocationObj == null ? "" : c.GeoStruct.Location.LocationObj.LocDesc : "");
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
                                Division = item.GeoStruct == null ? "" : item.GeoStruct.Division == null ? "" : item.GeoStruct.Division.Name,
                                Location = item.GeoStruct == null ? "" : item.GeoStruct.Location == null ? "" : item.GeoStruct.Location.LocationObj == null ? "" : item.GeoStruct.Location.LocationObj.LocDesc,
                                Department = item.GeoStruct == null ? "" : item.GeoStruct.Department == null ? "" : item.GeoStruct.Department.DepartmentObj == null ? "" : item.GeoStruct.Department.DepartmentObj.DeptDesc,
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
                                     select new[] { null, Convert.ToString(c.Id), c.GeoStruct == null ? "" : c.GeoStruct.Division == null ? "" : c.GeoStruct.Division.Name, };
                        return Json(new
                        {
                            sEcho = param.sEcho,
                            iTotalRecords = all.Count(),
                            iTotalDisplayRecords = fall.Count(),
                            data = result
                        }, JsonRequestBehavior.AllowGet);
                    }//for data reterivation
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
        public class FormChildDataClass //childgrid
        {
            public int Id { get; set; }
            public string PostName { get; set; }
            public int SanctionPost { get; set; }
            public double BudgetAmount { get; set; }
        }
        public ActionResult Get_FormulaStructDetails(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    int id = Convert.ToInt32(Session["CompId"]);
                    var SalHdForm = db.ManPowerBudget
                        .Include(e => e.GeoStruct)
                        .Where(e => e.Id == data).SingleOrDefault();

                    var db_data = db.ManPowerBudget
                        .Include(e => e.FuncStruct)
                        .Include(e => e.FuncStruct.Job)
                        .Include(e => e.FuncStruct.Job.JobPosition)
                        .Include(e => e.GeoStruct).Where(e => e.GeoStruct.Id == SalHdForm.GeoStruct.Id)
                        .ToList();


                    if (db_data != null)
                    {
                        List<FormChildDataClass> returndata = new List<FormChildDataClass>();

                        foreach (var a in db_data)
                        {
                            returndata.Add(new FormChildDataClass
                            {
                                Id = a.Id,
                                PostName = a.FuncStruct == null ? "" : a.FuncStruct.FullDetails,
                                SanctionPost = a.SanctionedPosts,
                                BudgetAmount = a.BudgetAmount,

                            });
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



        public class salhddetails
        {
            public Array ManPowerDetailsBatch_id { get; set; }
            public Array ManPowerDetailsBatch_details { get; set; }
        }
        [HttpPost]
        //public ActionResult Edit(int data)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {

        //        var Q = db.ManPowerBudget
        //            //.Include(e => e.ManPowerDetailsBatch)
        //            //.Include(e => e.RecruitmentCalendar)

        //            .Where(e => e.Id == data).Select
        //            (e => new
        //            {
        //                RecruitmentCalendar = e.RecruitmentCalendar,
        //                BudgetAmount = e.BudgetAmount,
        //                SanctionedPosts = e.SanctionedPosts,

        //                //  Action = e.DBTrack.Action
        //            }).ToList();
        //        List<salhddetails> objlist = new List<salhddetails>();
        //        var N = db.ManPowerBudget.Where(e => e.Id == data).Select(e => e.ManPowerDetailsBatch).ToList();
        //        if (N != null && N.Count > 0)
        //        {
        //            foreach (var ca in N)
        //            {
        //                objlist.Add(new salhddetails
        //                {

        //                    ManPowerDetailsBatch_id = ca.Select(e => e.Id).ToArray(),
        //                    ManPowerDetailsBatch_details = ca.Select(e => e.ManPowerPostData).ToArray()


        //                });

        //            }

        //        }

        //        //var W = db.DT_Corporate
        //        //     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
        //        //     (e => new
        //        //     {
        //        //         DT_Id = e.Id,
        //        //         Code = e.Code == null ? "" : e.Code,
        //        //         Name = e.Name == null ? "" : e.Name,
        //        //         BusinessType_Val = e.BusinessType_Id == 0 ? "" : db.LookupValue
        //        //                    .Where(x => x.Id == e.BusinessType_Id)
        //        //                    .Select(x => x.LookupVal).FirstOrDefault(),

        //        //         Address_Val = e.Address_Id == 0 ? "" : db.Address.Where(x => x.Id == e.Address_Id).Select(x => x.FullAddress).FirstOrDefault(),
        //        //         Contact_Val = e.ContactDetails_Id == 0 ? "" : db.ContactDetails.Where(x => x.Id == e.ContactDetails_Id).Select(x => x.FullContactDetails).FirstOrDefault()
        //        //     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

        //        var Corp = db.Corporate.Find(data);
        //        //TempData["RowVersion"] = Corp.RowVersion;
        //        // var Auth = Corp.DBTrack.IsModified;
        //        return Json(new Object[] { Q, objlist, JsonRequestBehavior.AllowGet });
        //    }
        //}

        //[HttpPost]
        //public async Task<ActionResult> EditSave(ManPowerBudget c, int data, FormCollection form) // Edit submit
        //{
        //    List<string> Msg = new List<string>();
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        try
        //        {

        //            string RecruitCalendardrop = form["RecruitCalendardrop"] == "0" ? null : form["RecruitCalendardrop"];
        //            string salhd = form["SalaryComponentList"] == "0" ? "" : form["SalaryComponentList"];
        //            //if (RecruitCalendardrop != null)
        //            //{
        //            //    var val = db.Calendar.Find(int.Parse(RecruitCalendardrop));
        //            //    c.RecruitmentCalendar = val;
        //            //}


        //            if (salhd != null)
        //            {
        //                var ids = Utility.StringIdsToListIds(salhd);
        //                var HolidayList = new List<ManPowerDetailsBatch>();
        //                foreach (var item in ids)
        //                {

        //                    int HolidayListid = Convert.ToInt32(item);
        //                    var val = db.ManPowerDetailsBatch.Include(e => e.ManPowerPostData)
        //                                        .Where(e => e.Id == HolidayListid).SingleOrDefault();
        //                    if (val != null)
        //                    {
        //                        HolidayList.Add(val);
        //                    }
        //                }
        //                c.ManPowerDetailsBatch = HolidayList;
        //            } bool Auth = form["Autho_Allow"] == "true" ? true : false;
        //            if (Auth == false)
        //            {
        //                if (ModelState.IsValid)
        //                {
        //                    try
        //                    {

        //                        //DbContextTransaction transaction = db.Database.BeginTransaction();

        //                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                        {
        //                            ManPowerBudget blog = null; // to retrieve old data
        //                            DbPropertyValues originalBlogValues = null;

        //                            using (var context = new DataBaseContext())
        //                            {
        //                                blog = context.ManPowerBudget.Where(e => e.Id == data).Include(e => e.ManPowerDetailsBatch)
        //                                                        .Include(e => e.RecruitmentCalendar)
        //                                                        .SingleOrDefault();
        //                                originalBlogValues = context.Entry(blog).OriginalValues;
        //                            }

        //                            //L.DBTrack = new DBTrack
        //                            //{
        //                            //    CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                            //    CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                            //    Action = "M",
        //                            //    ModifiedBy = SessionManager.UserName,
        //                            //    ModifiedOn = DateTime.Now
        //                            //};

        //                            if (RecruitCalendardrop != null)
        //                            {
        //                                if (RecruitCalendardrop != "")
        //                                {
        //                                    var val = db.Calendar.Find(int.Parse(RecruitCalendardrop));
        //                                    c.RecruitmentCalendar = val;

        //                                    var type = db.ManPowerBudget.Include(e => e.RecruitmentCalendar).Where(e => e.Id == data).SingleOrDefault();
        //                                    IList<ManPowerBudget> typedetails = null;
        //                                    if (type.RecruitmentCalendar != null)
        //                                    {
        //                                        typedetails = db.ManPowerBudget.Where(x => x.RecruitmentCalendar.Id == type.RecruitmentCalendar
        //                                            .Id && x.Id == data).ToList();
        //                                    }
        //                                    else
        //                                    {
        //                                        typedetails = db.ManPowerBudget.Where(x => x.Id == data).ToList();
        //                                    }
        //                                    //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
        //                                    foreach (var s in typedetails)
        //                                    {
        //                                        s.RecruitmentCalendar = c.RecruitmentCalendar;
        //                                        db.ManPowerBudget.Attach(s);
        //                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                                        //await db.SaveChangesAsync();
        //                                        db.SaveChanges();
        //                                        // TempData["RowVersion"] = s.RowVersion;
        //                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //                                    }
        //                                }
        //                                else
        //                                {
        //                                    var WFTypeDetails = db.ManPowerBudget.Include(e => e.RecruitmentCalendar).Where(x => x.Id == data).ToList();
        //                                    foreach (var s in WFTypeDetails)
        //                                    {
        //                                        s.RecruitmentCalendar = null;
        //                                        db.ManPowerBudget.Attach(s);
        //                                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                                        //await db.SaveChangesAsync();
        //                                        db.SaveChanges();
        //                                        //TempData["RowVersion"] = s.RowVersion;
        //                                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //                                    }
        //                                }
        //                            }
        //                            else
        //                            {
        //                                var HoliCalendarDetails = db.ManPowerBudget.Include(e => e.RecruitmentCalendar).Where(x => x.Id == data).ToList();
        //                                foreach (var s in HoliCalendarDetails)
        //                                {
        //                                    s.RecruitmentCalendar = null;
        //                                    db.ManPowerBudget.Attach(s);
        //                                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                                    //await db.SaveChangesAsync();
        //                                    db.SaveChanges();
        //                                    //   TempData["RowVersion"] = s.RowVersion;
        //                                    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //                                }
        //                            }
        //                            List<ManPowerDetailsBatch> ObjHolidayList = new List<ManPowerDetailsBatch>();
        //                            ManPowerBudget ManPowerBudgetdetails = null;
        //                            ManPowerBudgetdetails = db.ManPowerBudget.Include(e => e.ManPowerDetailsBatch).Where(e => e.Id == data).SingleOrDefault();
        //                            if (salhd != null && salhd != "")
        //                            {
        //                                var ids = Utility.StringIdsToListIds(salhd);
        //                                foreach (var ca in ids)
        //                                {
        //                                    var HolidayListListvalue = db.ManPowerDetailsBatch.Find(ca);
        //                                    ObjHolidayList.Add(HolidayListListvalue);
        //                                    ManPowerBudgetdetails.ManPowerDetailsBatch = ObjHolidayList;
        //                                }
        //                            }
        //                            else
        //                            {
        //                                ManPowerBudgetdetails.ManPowerDetailsBatch = null;
        //                            }

        //                            var CurCorp = db.ManPowerBudget.Find(data);
        //                            // TempData["CurrRowVersion"] = CurCorp.RowVersion;
        //                            db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;


        //                            ManPowerBudget ManPowerBudget = new ManPowerBudget()
        //                                {

        //                                    Id = data,
        //                                    BudgetAmount = c.BudgetAmount,
        //                                    SanctionedPosts = c.SanctionedPosts,

        //                                    // DBTrack = L.DBTrack
        //                                };
        //                            db.ManPowerBudget.Attach(ManPowerBudget);
        //                            db.Entry(ManPowerBudget).State = System.Data.Entity.EntityState.Modified;
        //                            db.Entry(ManPowerBudget).OriginalValues["RowVersion"] = TempData["RowVersion"];

        //                            // int a = EditS(CreditDatelist, ExcludeLeaveHeadslist, ConvertLeaveHeadBallist, ConvertLeaveHeadlist, data, L, L.DBTrack);
        //                            using (var context = new DataBaseContext())
        //                            {
        //                                //var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, L.DBTrack);
        //                                //dt_holiday DT_Corp = (DT_LvCreditPolicy)obj;
        //                                //DT_Corp.CreditDate_Id = blog.CreditDate == null ? 0 : blog.CreditDate.Id;
        //                                //db.Create(DT_Corp);
        //                                db.SaveChanges();
        //                            }
        //                            await db.SaveChangesAsync();
        //                            ts.Complete();
        //                            //var qurey = db.HolidayCalendar.Include(e => e.HoliCalendar).Include(e => e.HolidayList).Where(e => e.Id == data).SingleOrDefault();
        //                            // return Json(new Object[] { c.Id, c.SanctionedPosts, "Record Updated", JsonRequestBehavior.AllowGet });
        //                            Msg.Add("  Record Updated");
        //                            return Json(new Utility.JsonReturnClass { Id = c.Id, Val = c.SanctionedPosts.ToString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                        }

        //                    }
        //                    catch (DbUpdateConcurrencyException ex)
        //                    {
        //                        var entry = ex.Entries.Single();
        //                        var clientValues = (ManPowerBudget)entry.Entity;
        //                        var databaseEntry = entry.GetDatabaseValues();
        //                        if (databaseEntry == null)
        //                        {
        //                            //  return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });

        //                            Msg.Add(" Unable to save changes. The record was deleted by another user.");
        //                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                        }
        //                        else
        //                        {
        //                            var databaseValues = (ManPowerBudget)databaseEntry.ToObject();
        //                            ///   c.RowVersion = databaseValues.RowVersion;

        //                        }
        //                    }

        //                    //   return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
        //                    Msg.Add("Record modified by another user.So refresh it and try to save again.");
        //                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);


        //                }
        //            }
        //            else
        //            {
        //                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                {

        //                    ManPowerBudget blog = null; // to retrieve old data
        //                    DbPropertyValues originalBlogValues = null;
        //                    ManPowerBudget Old_Corp = null;

        //                    using (var context = new DataBaseContext())
        //                    {
        //                        blog = context.ManPowerBudget.Where(e => e.Id == data).SingleOrDefault();
        //                        originalBlogValues = context.Entry(blog).OriginalValues;
        //                    }
        //                    //L.DBTrack = new DBTrack
        //                    //{
        //                    //    CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                    //    CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                    //    Action = "M",
        //                    //    IsModified = blog.DBTrack.IsModified == true ? true : false,
        //                    //    ModifiedBy = SessionManager.UserName,
        //                    //    ModifiedOn = DateTime.Now
        //                    //};

        //                    //if (TempData["RowVersion"] == null)
        //                    //{
        //                    //    TempData["RowVersion"] = blog.RowVersion;
        //                    //}

        //                    //ManPowerBudget corp = new ManPowerBudget()
        //                    //{
        //                    //    ManPowerDetailsBatch = c.ManPowerDetailsBatch,
        //                    //    RecruitmentCalendar = c.RecruitmentCalendar,
        //                    //    Id = data,
        //                    //    //   DBTrack = L.DBTrack,
        //                    //    //  RowVersion = (Byte[])TempData["RowVersion"]
        //                    //};


        //                    //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

        //                    using (var context = new DataBaseContext())
        //                    {
        //                        //var obj = DBTrackFile.ModifiedDataHistory("Leave/Leave", "M", blog, corp, "LvEncashReq", L.DBTrack);
        //                        //// var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

        //                        //Old_Corp = context.LvCreditPolicy.Where(e => e.Id == data).Include(e => e.ConvertLeaveHead)
        //                        //    .Include(e => e.ConvertLeaveHeadBal).Include(e => e.ExcludeLeaveHeads).Include(e => e.CreditDate).SingleOrDefault();
        //                        //DT_LvCreditPolicy DT_Corp = (DT_LvCreditPolicy)obj;
        //                        //DT_Corp.CreditDate_Id = blog.CreditDate == null ? 0 : blog.CreditDate.Id;

        //                        //db.Create(DT_Corp);
        //                        //db.SaveChanges();
        //                    }
        //                    //    blog.DBTrack = L.DBTrack;
        //                    db.ManPowerBudget.Attach(blog);
        //                    db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
        //                    db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //                    db.SaveChanges();
        //                    ts.Complete();
        //                    //   return Json(new Object[] { blog.Id, c.SanctionedPosts, "Record Updated", JsonRequestBehavior.AllowGet });
        //                    Msg.Add("  Record Updated");
        //                    return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = c.SanctionedPosts.ToString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                }

        //            }
        //            return View();
        //        }
        //        catch (Exception ex)
        //        {
        //            LogFile Logfile = new LogFile();
        //            ErrorLog Err = new ErrorLog()
        //            {
        //                ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
        //                ExceptionMessage = ex.Message,
        //                ExceptionStackTrace = ex.StackTrace,
        //                LineNo = Convert.ToString(new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
        //                LogTime = DateTime.Now
        //            };
        //            Logfile.CreateLogFile(Err);
        //            Msg.Add(ex.Message);
        //            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //}
        public ActionResult EditGridDetails(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.ManPowerBudget
                 .Where(e => e.Id == data).AsEnumerable().Select
                 (c => new
                 {
                     SanctionedPosts = c.SanctionedPosts,
                     BudgetAmount = c.BudgetAmount
                 }).SingleOrDefault();
                return Json(new { data = Q }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GridEditSave(ManPowerBudget c, FormCollection form, string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (data != null)
                {
                    var id = Convert.ToInt32(data);

                    var db_data = db.ManPowerBudget.Where(e => e.Id == id).SingleOrDefault();
                    db_data.BudgetAmount = c.BudgetAmount;
                    db_data.SanctionedPosts = c.SanctionedPosts;

                    try
                    {
                        db.ManPowerBudget.Attach(db_data);
                        db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;
                        return Json(new { status = true, data = db_data, responseText = "Record Updated..!" }, JsonRequestBehavior.AllowGet);
                    }
                    catch (Exception e)
                    {

                        throw e;
                    }
                }
                else
                {
                    return Json(new { status = false, responseText = "Data Is Null" }, JsonRequestBehavior.AllowGet);
                }
            }
        }


        public ActionResult P2BGrid(P2BGrid_Parameters gp)
        {
            try
            {
                DataBaseContext db = new DataBaseContext();
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                var jsonData = (Object)null;
                IEnumerable<ManPowerBudget> ManPowerBudget = null;
                if (gp.IsAutho == true)
                {
                    ManPowerBudget = db.ManPowerBudget
                        //.Include(e => e.RecruitmentCalendar)
                        //.Include(e => e.ManPowerDetailsBatch)
                        .AsNoTracking().ToList();
                }
                else
                {
                    ManPowerBudget = db.ManPowerBudget
                        //.Include(e => e.RecruitmentCalendar)
                        // .Include(e => e.ManPowerDetailsBatch)
                        .AsNoTracking().ToList();
                }

                IEnumerable<ManPowerBudget> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = ManPowerBudget;
                    if (gp.searchOper.Equals("eq"))
                    {
                        if (gp.searchField == "Id")
                            jsonData = IE.Select(a => new { a.Id, a.BudgetAmount, a.SanctionedPosts }).Where((e => (e.Id.ToString().Contains(gp.searchString)))).ToList();
                        else if (gp.searchField == "BudgetAmount")
                            jsonData = IE.Select(a => new { a.Id, a.BudgetAmount, a.SanctionedPosts }).Where((e => (e.BudgetAmount.ToString().Contains(gp.searchString)))).ToList();
                        else if (gp.searchField == "SanctionedPosts")
                            jsonData = IE.Select(a => new { a.Id, a.BudgetAmount, a.SanctionedPosts }).Where((e => (e.SanctionedPosts.ToString().Contains(gp.searchString)))).ToList();

                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.BudgetAmount, a.SanctionedPosts }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = ManPowerBudget;
                    Func<ManPowerBudget, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "BudgetAmount" ? c.BudgetAmount.ToString() :
                                         gp.sidx == "SanctionedPosts" ? c.SanctionedPosts.ToString() :
                                           "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.BudgetAmount), Convert.ToString(a.SanctionedPosts) }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.BudgetAmount), Convert.ToString(a.SanctionedPosts) }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.BudgetAmount, a.SanctionedPosts }).ToList();
                    }
                    totalRecords = ManPowerBudget.Count();
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
        public class EditData
        {
            public int Id { get; set; }
            public SalaryHead SalaryHead { get; set; }
            public bool Editable { get; set; }
            public double Amount { get; set; }
            public string job { get; set; }
            public int posts { get; set; }
        }
        public ActionResult P2BInlineGrid(P2BGrid_Parameters gp)
        {
            try
            {
                DataBaseContext db = new DataBaseContext();
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                var jsonData = (Object)null;
                IEnumerable<EditData> EmpSalStruct = null;

                List<EditData> model = new List<EditData>();
                var view = new EditData();
                gp.id = "1";

                int EmpId = int.Parse(gp.id);
                bool EditAppl = true;



                // List<string> d = TempData["b"] as List<string>;
                // foreach (var item in d)
                // {
                //     var v = item.ToString();
                //     var m = v;

                // }
                // List<string> lst = (List<string>)TempData["b"]; // cast tempdata to List of string

                //ViewBag.Collection=lst;

                var v = "";
                if (TempData["b"] != null)
                {
                    if (TempData.Values != null && TempData.Values.Count() > 0)
                    {
                        foreach (var item in TempData["b"] as List<man>)
                        {
                            v = item.fulldetails;
                        }
                    }
                }






                {
                    view = new EditData()
                    {

                        job = v,

                    };

                    model.Add(view);

                }

                EmpSalStruct = model;

                IEnumerable<EditData> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = EmpSalStruct;
                    if (gp.searchOper.Equals("eq"))
                    {
                        //if (gp.searchField == "Id")
                        //    jsonData = IE.Select(a => new Object[] { a.Id, a.SalaryHead.Name, a.Amount, a.SalaryHead.Frequency.LookupVal, a.SalaryHead.Type.LookupVal, a.SalaryHead.SalHeadOperationType, a.Editable }).Where((e => (e.Contains(gp.searchString)))).ToList();
                        //else if (gp.searchField == "SalaryHead")
                        //    jsonData = IE.Select(a => new Object[] { a.Id, a.SalaryHead.Name, a.Amount, a.SalaryHead.Frequency, a.SalaryHead.Type, a.SalaryHead.SalHeadOperationType, a.Editable }).Where((e => (e.Contains(gp.searchString)))).ToList();
                        //else if (gp.searchField == "Amount")
                        //    jsonData = IE.Select(a => new Object[] { a.Id, a.SalaryHead.Name, a.Amount, a.SalaryHead.Frequency, a.SalaryHead.Type, a.SalaryHead.SalHeadOperationType, a.Editable }).Where((e => (e.Contains(gp.searchString)))).ToList();
                        //else if (gp.searchField == "Frequency")
                        //    jsonData = IE.Select(a => new Object[] { a.Id, a.SalaryHead.Name, a.Amount, a.SalaryHead.Frequency, a.SalaryHead.Type, a.SalaryHead.SalHeadOperationType, a.Editable }).Where((e => (e.Contains(gp.searchString)))).ToList();
                        //else if (gp.searchField == "Type")
                        //    jsonData = IE.Select(a => new Object[] { a.Id, a.SalaryHead.Name, a.Amount, a.SalaryHead.Frequency, a.SalaryHead.Type, a.SalaryHead.SalHeadOperationType, a.Editable }).Where((e => (e.Contains(gp.searchString)))).ToList();
                        //else if (gp.searchField == "SalHeadOperationType")
                        //    jsonData = IE.Select(a => new Object[] { a.Id, a.SalaryHead.Name, a.Amount, a.SalaryHead.Frequency, a.SalaryHead.Type, a.SalaryHead.SalHeadOperationType, a.Editable }).Where((e => (e.Contains(gp.searchString)))).ToList();

                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();

                        jsonData = IE.Select(a => new Object[] { a.job }).Where((e => (e.Contains(gp.searchString)))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.job }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = EmpSalStruct;
                    Func<EditData, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "Job" ? c.job :

                                           "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.job }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.job }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.job }).ToList();
                    }
                    totalRecords = EmpSalStruct.Count();
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
    }
}