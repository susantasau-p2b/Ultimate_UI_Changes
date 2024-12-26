///
/// Created by Kapil
///

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
using Training;
using P2BUltimate.Security;

namespace P2BUltimate.Controllers.Training.MainController
{
    public class TrainingEvaluationController : Controller
    {
        //
        // GET: /TrainingEvaluation/
        public ActionResult Index()
        {
            return View("~/Views/Training/MainViews/TrainingEvaluation/Index.cshtml");
        }

        public ActionResult partial()
        {
            return View("~/Views/Shared/Training/_TrainingEvaluationPartial.cshtml");
        }
        public ActionResult GridPartial()
        {
            return View("~/Views/Shared/Training/_TrainingEvaluationPartial.cshtml");
        }
        // private DataBaseContext db = new DataBaseContext();




        public class DeserializeClass
        {
            public string Id { get; set; }
            public int GDScore { get; set; }
            public int InterviewScore { get; set; }
            public int WrittenScore { get; set; }
            public string EvaluationDetails { get; set; }


        }

        public class returnDataClass
        {

            public int GDScore { get; set; }
            public int InterviewScore { get; set; }
            public int WrittenScore { get; set; }
            public string EvaluationDetails { get; set; }
            public string Batchname { get; set; }

        }

        public class returndatagridclass //Parentgrid
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
            public string StartDate { get; set; }
            public string EndDate { get; set; }
            public string ProgramList { get; set; }
        }

        public ActionResult Emp_Grid(ParamModel param, string y)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    List<EmployeeTraining> all1 = new List<EmployeeTraining>();
                    List<EmployeeTraining> FilterEmployee = new List<EmployeeTraining>();
                    var compid = Convert.ToInt32(Session["CompId"].ToString());
                    var ab = db.CompanyTraining.Where(e => e.Company.Id == compid)
                      .Include(e => e.EmployeeTraining)
                      .Include(e => e.EmployeeTraining.Select(a => a.Employee))
                      .Include(e => e.EmployeeTraining.Select(a => a.TrainingDetails))
                      .Include(e => e.EmployeeTraining.Select(a => a.TrainingDetails.Select(t => t.TrainingSchedule)))
                      .Include(e => e.EmployeeTraining.Select(a => a.TrainingDetails.Select(t => t.TrainigDetailSessionInfo)))
                      .Include(e => e.EmployeeTraining.Select(a => a.TrainingDetails.Select(t => t.TrainigDetailSessionInfo.Select(x => x.TrainingSession))))
                      .Include(e => e.EmployeeTraining.Select(a => a.TrainingDetails.Select(t => t.TrainigDetailSessionInfo.Select(x => x.TrainingEvaluation))))
                      .Include(e => e.EmployeeTraining.Select(a => a.TrainingDetails.Select(t => t.TrainigDetailSessionInfo.Select(x => x.TrainingSession.TrainingProgramCalendar.ProgramList))))
                      .Include(e => e.EmployeeTraining.Select(a => a.Employee.EmpName)).AsNoTracking().AsParallel()
                      .SingleOrDefault();
                    if (y != "")
                    {
                        int scheduleids = Convert.ToInt32(y);
                        all1 = ab.EmployeeTraining.ToList();

                        foreach (var item in all1)
                        {
                            int count = 0;
                            if (item.TrainingDetails.Count() > 0)
                            {
                                var filterschedulewiseemp = item.TrainingDetails.Where(t => t.TrainingSchedule.Id == scheduleids).ToList();
                                if (filterschedulewiseemp.Count() > 0)
                                {
                                    foreach (var item1 in filterschedulewiseemp)
                                    {
                                        var OnlyPresentEmployee = item1.TrainigDetailSessionInfo.Where(e => e.IsPresent == true && e.IsCancelled == false).ToList();
                                        if (OnlyPresentEmployee.Count() > 0)
                                        {
                                            count = +1;

                                        }
                                    }
                                }
                            }
                            if (count > 0)
                            {
                                FilterEmployee.Add(item);
                            }
                        }
                    }
                    else
                    {
                        all1 = ab.EmployeeTraining.ToList();

                        foreach (var item in all1)
                        {
                            int count = 0;
                            if (item.TrainingDetails.Count() > 0)
                            {
                                var filterschedulewiseemp = item.TrainingDetails.ToList();
                                if (filterschedulewiseemp.Count() > 0)
                                {
                                    foreach (var item1 in filterschedulewiseemp)
                                    {
                                        var OnlyPresentEmployee = item1.TrainigDetailSessionInfo.Where(e => e.IsPresent == true && e.IsCancelled == false).ToList();
                                        if (OnlyPresentEmployee.Count() > 0)
                                        {
                                            count = +1;

                                        }
                                    }
                                }
                            }
                            if (count > 0)
                            {
                                FilterEmployee.Add(item);
                            }
                        }
                    }

                    var all = FilterEmployee;
                    //for searchs
                    IEnumerable<EmployeeTraining> fall;
                    if (param.sSearch == null)
                    {
                        fall = all;
                    }
                    else
                    {
                        fall = all.Where(e => e.Employee.EmpCode.ToUpper() == param.sSearch.ToUpper()).ToList();
                        fall = all.Where(e => (e.Id.ToString().Contains(param.sSearch))
                                  || (e.Employee.EmpCode.ToString().Contains(param.sSearch))
                                  || (e.Employee.EmpName.FullNameFML.ToString().ToUpper().Contains(param.sSearch.ToUpper()))
                                  ).ToList();
                    }

                    //for column sorting
                    var sortindex = Convert.ToInt32(Request["iSortCol_0"]);

                    Func<EmployeeTraining, string> orderfunc = (c =>
                                                                Convert.ToUInt32(sortindex) == 0 ? c.Id.ToString() :
                                                                sortindex == 1 ? c.Employee.EmpCode : "");
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
                            //foreach (var item1 in item.TrainingDetails)
                            //{
                            //if (y == null)
                            //{
                            //var OnlyPresentEmployee = item1.TrainigDetailSessionInfo.Where(e => e.IsPresent == true && e.IsCancelled == false).ToList();
                            //foreach (var item2 in OnlyPresentEmployee)
                            //{
                            //if (item2.TrainingEvaluation != null)
                            //{
                            result.Add(new returndatagridclass
                            {
                                Id = item.Id,
                                Code = item.Employee != null ? item.Employee.EmpCode : null,
                                Name = item.Employee != null && item.Employee.EmpName != null ? item.Employee.EmpName.FullNameFML : null,
                                //StartDate = item2.TrainingSession != null && item2.TrainingSession.TrainingProgramCalendar != null ? item2.TrainingSession.TrainingProgramCalendar.StartDate.Value.ToShortDateString() : "",
                                //EndDate = item2.TrainingSession != null && item2.TrainingSession.TrainingProgramCalendar != null ? item2.TrainingSession.TrainingProgramCalendar.EndDate.Value.ToShortDateString() : "",
                                //ProgramList = item2.TrainingSession != null && item2.TrainingSession.TrainingProgramCalendar != null && item2.TrainingSession.TrainingProgramCalendar.ProgramList != null ? item2.TrainingSession.TrainingProgramCalendar.ProgramList.FullDetails : "",
                            });
                            // }
                            //  }
                            //}
                            //else
                            //{
                            //    if (item1.TrainingSchedule.TrainingBatchName == y)
                            //    {
                            //        foreach (var item2 in item1.TrainingSchedule.TrainingSession)
                            //        {

                            //            result.Add(new returndatagridclass
                            //            {
                            //                Id = item1.Id.ToString(),
                            //                Code = item.Employee != null ? item.Employee.EmpCode : null,
                            //                Name = item.Employee != null && item.Employee.EmpName != null ? item.Employee.EmpName.FullNameFML : null,
                            //                StartDate = item2.TrainingProgramCalendar != null ? item2.TrainingProgramCalendar.StartDate.Value.ToShortDateString() : "",
                            //                EndDate = item2.TrainingProgramCalendar != null ? item2.TrainingProgramCalendar.EndDate.Value.ToShortDateString() : "",
                            //                ProgramList = item2.TrainingProgramCalendar.ProgramList != null ? item2.TrainingProgramCalendar.ProgramList.FullDetails : "",
                            //            });
                            //        }
                            //    }

                            //}

                            // }
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

        public ActionResult Get_Employelist(string databatch, string session, string geo_id)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                DateTime? dt = null;
                string monthyr = "";
                DateTime? dtChk = null;
                int lastDayOfMonth;


                List<Employee> data = new List<Employee>();
                var compid = Convert.ToInt32(Session["CompId"].ToString());
                db.Database.CommandTimeout = 3600;

                var empdata = db.CompanyTraining.Where(e => e.Company.Id == compid)
                    .Include(e => e.EmployeeTraining)
                    .Include(e => e.EmployeeTraining.Select(a => a.Employee))
                    .Include(e => e.EmployeeTraining.Select(a => a.TrainingDetails))
                    .Include(e => e.EmployeeTraining.Select(a => a.TrainingDetails.Select(r => r.TrainingSchedule)))
                    .Include(e => e.EmployeeTraining.Select(a => a.TrainingDetails.Select(r => r.TrainigDetailSessionInfo)))
                    .Include(e => e.EmployeeTraining.Select(a => a.TrainingDetails.Select(r => r.TrainigDetailSessionInfo.Select(t => t.TrainingEvaluation))))
                    .Include(e => e.EmployeeTraining.Select(a => a.TrainingDetails.Select(r => r.TrainigDetailSessionInfo.Select(t => t.TrainingSession))))
                    .Include(e => e.EmployeeTraining.Select(a => a.Employee.EmpName))
                    .Include(e => e.EmployeeTraining.Select(a => a.Employee.ServiceBookDates)).AsNoTracking().AsParallel()
                    .SingleOrDefault();



                List<Employee> Emp = new List<Employee>();
                if (databatch != null)
                {

                    var id = Convert.ToInt32(databatch);
                    int Sessionid = Convert.ToInt32(session);
                    foreach (var item in empdata.EmployeeTraining)
                    {
                        //var EmpLst = item.TrainingDetails.Where(r => r.TrainingSchedule.Id == id).SingleOrDefault();
                        int count = 0;
                        foreach (var item1 in item.TrainingDetails)
                        {

                            if (item1.TrainingSchedule.Id == id)
                            {
                                if (item1.TrainigDetailSessionInfo.Where(t => t.TrainingSession.Id == Sessionid).Count() > 0)
                                {
                                    if (item1.TrainigDetailSessionInfo.Where(t => t.IsPresent == true && t.IsCancelled == false && t.TrainingEvaluation == null).ToList().Count() > 0)
                                    {
                                        count = +1;
                                    }
                                }
                            }
                        }
                        if (count > 0)
                        {
                            Emp.Add(item.Employee);
                        }
                        //if (EmpLst != null)
                        //{
                        //    if (EmpLst.TrainigDetailSessionInfo.Where(t => t.TrainingSession.Id == Sessionid).Count() > 0)
                        //    {
                        //        Emp.Add(item.Employee);
                        //    }

                        //}
                    }
                }


                List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                if (Emp != null && Emp.Count != 0)
                {
                    foreach (var item in Emp)
                    {
                        if (geo_id == "")
                        {
                            returndata.Add(new Utility.returndataclass
                            {
                                code = item.Id.ToString(),
                                value = item.FullDetails,
                            });
                        }
                        else
                        {
                            var Serialize = new JavaScriptSerializer();
                            var deserialize = Serialize.Deserialize<Utility.GridParaStructIdClass>(geo_id);

                            if (deserialize.Filter != "" && deserialize.Filter != null)
                            {
                                dt = Convert.ToDateTime("01/" + deserialize.Filter).AddMonths(-1);
                                monthyr = dt.Value.ToString("MM/yyyy");
                                dtChk = Convert.ToDateTime(DateTime.DaysInMonth(dt.Value.Year, dt.Value.Month) + "/" + monthyr);
                            }
                            else
                            {
                                dt = Convert.ToDateTime("01/" + DateTime.Now.ToString("MM/yyyy")).AddMonths(-1);
                                monthyr = dt.Value.ToString("MM/yyyy");
                                dtChk = Convert.ToDateTime(DateTime.DaysInMonth(dt.Value.Year, dt.Value.Month) + "/" + monthyr);
                            }

                            //var empdata = db.Employee
                            //    .Include(a => a.GeoStruct)
                            //    .Include(a => a.FuncStruct)
                            //    .Include(a => a.PayStruct)
                            //    .Include(a => a.EmpName)
                            //    .Include(a => a.ServiceBookDates)
                            //    .Where(e =>e.Id==item.Id).SingleOrDefault();
                            List<Employee> List_all = new List<Employee>();

                            if (deserialize.GeoStruct != null)
                            {
                                var one_id = Utility.StringIdsToListIds(deserialize.GeoStruct);
                                foreach (var ca in one_id)
                                {
                                    var id = Convert.ToInt32(ca);
                                    if (item.GeoStruct != null && item.GeoStruct.Id == id)
                                    {

                                        returndata.Add(new Utility.returndataclass
                                        {
                                            code = item.Id.ToString(),
                                            value = item.FullDetails,
                                        });
                                    }
                                }
                            }
                            if (deserialize.PayStruct != null)
                            {
                                var one_id = Utility.StringIdsToListIds(deserialize.PayStruct);
                                foreach (var ca in one_id)
                                {
                                    var id = Convert.ToInt32(ca);
                                    if (item.PayStruct != null && item.PayStruct.Id == id)
                                    {

                                        returndata.Add(new Utility.returndataclass
                                        {
                                            code = item.Id.ToString(),
                                            value = item.FullDetails,
                                        });
                                    }
                                }

                            }
                            if (deserialize.FunStruct != null)
                            {
                                var one_id = Utility.StringIdsToListIds(deserialize.FunStruct);
                                foreach (var ca in one_id)
                                {
                                    var id = Convert.ToInt32(ca);
                                    if (item.FuncStruct != null && item.FuncStruct.Id == id)
                                    {

                                        returndata.Add(new Utility.returndataclass
                                        {
                                            code = item.Id.ToString(),
                                            value = item.FullDetails,
                                        });
                                    }
                                }

                            }

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
                    return Json(new Utility.JsonReturnClass { success = false, responseText = "No Record Found OR Data is alredy assigned for all candidates in this batch!", data = "Employee-Table" }, JsonRequestBehavior.AllowGet);
                    //return Json("No Record Found", JsonRequestBehavior.AllowGet);
                }
            }
        }

        public ActionResult GetBatchNameDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.TrainingSchedule.ToList();
                IEnumerable<TrainingSchedule> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.TrainingSchedule.ToList();

                }
                else
                {

                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.TrainingBatchName }).OrderByDescending(s => s.srno).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.TrainingBatchName }).OrderByDescending(i => i.Id).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetBatchNameDetails1(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.TrainingSchedule.Where(e => e.IsBatchClose == false).ToList();
                IEnumerable<TrainingSchedule> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.TrainingSchedule.Where(e => e.IsBatchClose == false).ToList();

                }
                else
                {

                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.TrainingBatchName }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.TrainingBatchName }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        //public ActionResult GetLookupDetailsSession(string data, int ts)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {

        //        var fall1 = db.TrainingSchedule.Include(e => e.TrainingSession)
        //            .Include(e => e.TrainingSession.Select(r => r.SessionType))
        //            .Where(e => e.Id == ts).FirstOrDefault();

        //        var fall = fall1.TrainingSession;
        //        IEnumerable<TrainingSession> all;
        //        if (!string.IsNullOrEmpty(data))
        //        {
        //            all = db.TrainingSession.ToList();

        //        }
        //        else
        //        {

        //            var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
        //            return Json(r, JsonRequestBehavior.AllowGet);
        //        }
        //        var result = (from c in all
        //                      select new { c.Id, c.FullDetails }).Distinct();
        //        return Json(result, JsonRequestBehavior.AllowGet);
        //    }
        //}


        public ActionResult GetLookupDetailsSession(string data, int ts)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                var fall1 = db.TrainingSchedule.Include(e => e.TrainingSession)
                    .Include(e => e.TrainingSession.Select(r => r.SessionType))
                    .Include(e => e.TrainingSession.Select(t => t.TrainingProgramCalendar))
                    .Include(e => e.TrainingSession.Select(t => t.TrainingProgramCalendar.ProgramList))

                    .Where(e => e.Id == ts).FirstOrDefault();
                var fall = fall1.TrainingSession;
                IEnumerable<TrainingSession> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.TrainingSession
                        .Include(e => e.TrainingProgramCalendar)
                        .Include(e => e.TrainingProgramCalendar.ProgramList).ToList();

                }
                else
                {

                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails + ", Program : " + ca.TrainingProgramCalendar.ProgramList.FullDetails }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.FullDetails }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }






        [HttpPost]
        public ActionResult Create1(TrainingEvaluation c) //Create submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (ModelState.IsValid)
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                        TrainingEvaluation TrainingEvaluation = new TrainingEvaluation()
                        {
                            InterviewScore = c.InterviewScore,
                            WrittenScore = c.WrittenScore,
                            GDScore = c.GDScore,
                            EvaluationDetails = c.EvaluationDetails,
                            DBTrack = c.DBTrack
                        };
                        try
                        {
                            db.TrainingEvaluation.Add(TrainingEvaluation);
                            var rtn_Obj = DBTrackFile.DBTrackSave("Training/Training", null, db.ChangeTracker, c.DBTrack);
                            DT_TrainingEvaluation DT_Corp = (DT_TrainingEvaluation)rtn_Obj;
                            db.Create(DT_Corp);
                            db.SaveChanges();
                            ts.Complete();
                            return this.Json(new Object[] { TrainingEvaluation.Id, TrainingEvaluation.FullDetails, "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            return RedirectToAction("Create", new { concurrencyError = true, id = c.Id });
                        }
                        catch (DataException /* dex */)
                        {
                            return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
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
                }
            }
        }


        [HttpPost]
        public async Task<ActionResult> Create(TrainingEvaluation c, FormCollection form) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                var BatchName = form["Batch_Name"] == null ? "" : form["Batch_Name"];
                var Emplist = form["Employee-Table"] == null ? "" : form["Employee-Table"];
                var SessionList = form["TrainingScheduleList1"] == "0" ? "" : form["TrainingScheduleList1"];
                bool Auth = form["Autho_Allow"] == "true" ? true : false;

                var compid = Convert.ToInt32(Session["CompId"].ToString());
                List<int> EmpId = null;
                if (Emplist != null && Emplist != "")
                {

                    EmpId = Utility.StringIdsToListIds(Emplist);

                }
                if (EmpId == null)
                {
                    Msg.Add("Please select employee ");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
                if (Auth == false)
                {
                    if (ModelState.IsValid)
                    {
                        try
                        {
                            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                            {
                                int sid = 0;
                                string sfull = null;

                                foreach (var a in EmpId)
                                {


                                    var OEmployeeTraining = db.EmployeeTraining
                                        .Include(e => e.TrainingDetails)
                                        .Include(e => e.TrainingDetails.Select(r => r.TrainingSchedule))
                                        .Include(e => e.TrainingDetails.Select(r => r.TrainigDetailSessionInfo))
                                        .Include(e => e.TrainingDetails.Select(r => r.TrainigDetailSessionInfo.Select(t => t.TrainingSession)))

                                        .Where(e => e.Employee.Id == a).SingleOrDefault();

                                    List<Employee> Emp = new List<Employee>();
                                    if (BatchName != null)
                                    {

                                        var id = Convert.ToInt32(BatchName);
                                        int Sessionid = Convert.ToInt32(SessionList);

                                        var EmpLst = OEmployeeTraining.TrainingDetails.Where(r => r.TrainingSchedule.Id == id).ToList();
                                        if (EmpLst.Count() > 0)
                                        {
                                            foreach (var item in EmpLst)
                                            {

                                                var SessionLstInfo = item.TrainigDetailSessionInfo.Where(t => t.TrainingSession.Id == Sessionid).FirstOrDefault();
                                                if (SessionLstInfo != null)
                                                {
                                                    c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };


                                                    TrainingEvaluation te = new TrainingEvaluation
                                                    {
                                                        EvaluationDate = c.EvaluationDate,
                                                        GDScore = c.GDScore,
                                                        WrittenScore = c.WrittenScore,
                                                        InterviewScore = c.InterviewScore,
                                                        DBTrack = c.DBTrack,
                                                        EvaluationDetails = c.EvaluationDetails
                                                    };
                                                    db.TrainingEvaluation.Add(te);
                                                    db.SaveChanges();

                                                    SessionLstInfo.TrainingEvaluation = te;
                                                    db.TrainigDetailSessionInfo.Attach(SessionLstInfo);
                                                    db.Entry(SessionLstInfo).State = System.Data.Entity.EntityState.Modified;
                                                    db.SaveChanges();
                                                    db.Entry(SessionLstInfo).State = System.Data.Entity.EntityState.Detached;
                                                }

                                            }
                                        }

                                    }
                                }



                                ts.Complete();
                                Msg.Add("  Record Updated");
                                return Json(new Utility.JsonReturnClass { Id = sid, Val = sfull, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                        }
                        catch (DbUpdateConcurrencyException ex)
                        {
                            var entry = ex.Entries.Single();
                            var clientValues = (TrainingDetails)entry.Entity;
                            var databaseEntry = entry.GetDatabaseValues();
                            if (databaseEntry == null)
                            {
                                Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                var databaseValues = (TrainingDetails)databaseEntry.ToObject();
                                c.RowVersion = databaseValues.RowVersion;
                            }
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                        Msg.Add("Record modified by another user.So refresh it and try to save again.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                }
                return View();
            }
        }








        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.TrainingEvaluation
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        InterviewScore = e.InterviewScore,
                        WrittenScore = e.WrittenScore,
                        GDScore = e.GDScore,
                        EvaluationDetails = e.EvaluationDetails,
                        Action = e.DBTrack.Action
                    }).ToList();


                var W = db.DT_TrainingEvaluation
                     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                     (e => new
                     {
                         DT_Id = e.Id,
                         InterviewScore = e.InterviewScore,
                         WrittenScore = e.WrittenScore,
                         GDScore = e.GDScore,
                         EvaluationDetails = e.EvaluationDetails,
                     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var Corp = db.TrainingEvaluation.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, W, Auth, JsonRequestBehavior.AllowGet });
            }
        }

        [HttpPost]
        public async Task<ActionResult> EditSave(TrainingEvaluation c, int data, FormCollection form) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                bool Auth = form["Autho_Allow"] == "true" ? true : false;

                if (Auth == false)
                {
                    if (ModelState.IsValid)
                    {
                        try
                        {
                            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                            {
                                TrainingEvaluation blog = null; // to retrieve old data
                                DbPropertyValues originalBlogValues = null;

                                using (var context = new DataBaseContext())
                                {
                                    blog = context.TrainingEvaluation.Where(e => e.Id == data).SingleOrDefault();
                                    originalBlogValues = context.Entry(blog).OriginalValues;
                                }

                                c.DBTrack = new DBTrack
                                {
                                    CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                    CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                    Action = "M",
                                    ModifiedBy = SessionManager.UserName,
                                    ModifiedOn = DateTime.Now
                                };

                                int a = EditS(data, c, c.DBTrack);
                                using (var context = new DataBaseContext())
                                {
                                    var obj = DBTrackFile.DBTrackSave("Training/Training", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                    DT_TrainingEvaluation DT_Corp = (DT_TrainingEvaluation)obj;
                                    db.Create(DT_Corp);
                                    db.SaveChanges();
                                }
                                await db.SaveChangesAsync();
                                ts.Complete();


                                return Json(new Object[] { c.Id, c.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });

                            }
                        }
                        catch (DbUpdateConcurrencyException ex)
                        {
                            var entry = ex.Entries.Single();
                            var clientValues = (TrainingEvaluation)entry.Entity;
                            var databaseEntry = entry.GetDatabaseValues();
                            if (databaseEntry == null)
                            {
                                return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                            }
                            else
                            {
                                var databaseValues = (TrainingEvaluation)databaseEntry.ToObject();
                                c.RowVersion = databaseValues.RowVersion;

                            }
                        }

                        return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                    }
                }
                else
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {

                        TrainingEvaluation blog = null; // to retrieve old data
                        DbPropertyValues originalBlogValues = null;
                        TrainingEvaluation Old_Corp = null;

                        using (var context = new DataBaseContext())
                        {
                            blog = context.TrainingEvaluation.Where(e => e.Id == data).SingleOrDefault();
                            originalBlogValues = context.Entry(blog).OriginalValues;
                        }
                        c.DBTrack = new DBTrack
                        {
                            CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                            CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                            Action = "M",
                            IsModified = blog.DBTrack.IsModified == true ? true : false,
                            ModifiedBy = SessionManager.UserName,
                            ModifiedOn = DateTime.Now
                        };
                        TrainingEvaluation corp = new TrainingEvaluation()
                        {
                            InterviewScore = c.InterviewScore,
                            WrittenScore = c.WrittenScore,
                            GDScore = c.GDScore,
                            EvaluationDetails = c.EvaluationDetails,
                            Id = data,
                            DBTrack = c.DBTrack,
                            RowVersion = (Byte[])TempData["RowVersion"]
                        };

                        using (var context = new DataBaseContext())
                        {
                            var obj = DBTrackFile.ModifiedDataHistory("Training/Training", "M", blog, corp, "TrainingEvaluation", c.DBTrack);
                            DT_TrainingEvaluation DT_Corp = (DT_TrainingEvaluation)obj;
                            db.Create(DT_Corp);
                        }
                        blog.DBTrack = c.DBTrack;
                        db.TrainingEvaluation.Attach(blog);
                        db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                        db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                        db.SaveChanges();
                        ts.Complete();
                        return Json(new Object[] { blog.Id, null, "Record Updated", JsonRequestBehavior.AllowGet });
                    }

                }
                return View();
            }
        }

        [HttpPost]
        public async Task<ActionResult> AuthSave(int auth_id, Boolean isauth, String auth_action) //to save authorised data
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (auth_action == "C")
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {

                        TrainingEvaluation corp = db.TrainingEvaluation.FirstOrDefault(e => e.Id == auth_id);

                        corp.DBTrack = new DBTrack
                        {
                            Action = "C",
                            ModifiedBy = corp.DBTrack.ModifiedBy != null ? corp.DBTrack.ModifiedBy : null,
                            CreatedBy = corp.DBTrack.CreatedBy != null ? corp.DBTrack.CreatedBy : null,
                            CreatedOn = corp.DBTrack.CreatedOn != null ? corp.DBTrack.CreatedOn : null,
                            IsModified = corp.DBTrack.IsModified == true ? false : false,
                            AuthorizedBy = SessionManager.UserName,
                            AuthorizedOn = DateTime.Now
                        };

                        db.TrainingEvaluation.Attach(corp);
                        db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                        db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                        var rtn_Obj = DBTrackFile.DBTrackSave("Training/Training", null, db.ChangeTracker, corp.DBTrack);
                        DT_TrainingEvaluation DT_Corp = (DT_TrainingEvaluation)rtn_Obj;
                        db.Create(DT_Corp);
                        await db.SaveChangesAsync();
                        ts.Complete();
                        return Json(new Object[] { corp.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                    }
                }
                else if (auth_action == "M")
                {

                    TrainingEvaluation Old_Corp = db.TrainingEvaluation.Where(e => e.Id == auth_id).SingleOrDefault();

                    DT_TrainingEvaluation Curr_Corp = db.DT_TrainingEvaluation
                                                .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                                                .OrderByDescending(e => e.Id)
                                                .FirstOrDefault();

                    if (Curr_Corp != null)
                    {
                        TrainingEvaluation corp = new TrainingEvaluation();
                        corp.InterviewScore = Curr_Corp.InterviewScore == null ? Old_Corp.InterviewScore : Curr_Corp.InterviewScore;
                        corp.WrittenScore = Curr_Corp.WrittenScore == null ? Old_Corp.WrittenScore : Curr_Corp.WrittenScore;
                        corp.GDScore = Curr_Corp.GDScore == null ? Old_Corp.GDScore : Curr_Corp.GDScore;
                        corp.EvaluationDetails = Curr_Corp.EvaluationDetails == null ? Old_Corp.EvaluationDetails : Curr_Corp.EvaluationDetails;

                        if (ModelState.IsValid)
                        {
                            try
                            {
                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    corp.DBTrack = new DBTrack
                                    {
                                        CreatedBy = Old_Corp.DBTrack.CreatedBy == null ? null : Old_Corp.DBTrack.CreatedBy,
                                        CreatedOn = Old_Corp.DBTrack.CreatedOn == null ? null : Old_Corp.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = Old_Corp.DBTrack.ModifiedBy == null ? null : Old_Corp.DBTrack.ModifiedBy,
                                        ModifiedOn = Old_Corp.DBTrack.ModifiedOn == null ? null : Old_Corp.DBTrack.ModifiedOn,
                                        AuthorizedBy = SessionManager.UserName,
                                        AuthorizedOn = DateTime.Now,
                                        IsModified = false
                                    };

                                    int a = EditS(auth_id, corp, corp.DBTrack);
                                    await db.SaveChangesAsync();
                                    ts.Complete();
                                    return Json(new Object[] { corp.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (TrainingEvaluation)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (TrainingEvaluation)databaseEntry.ToObject();
                                    corp.RowVersion = databaseValues.RowVersion;
                                }
                            }

                            return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else
                        return Json(new Object[] { "", "Data removed from history", JsonRequestBehavior.AllowGet });
                }
                else if (auth_action == "D")
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {

                        TrainingEvaluation corp = db.TrainingEvaluation.AsNoTracking().FirstOrDefault(e => e.Id == auth_id);
                        corp.DBTrack = new DBTrack
                        {
                            Action = "D",
                            ModifiedBy = corp.DBTrack.ModifiedBy != null ? corp.DBTrack.ModifiedBy : null,
                            CreatedBy = corp.DBTrack.CreatedBy != null ? corp.DBTrack.CreatedBy : null,
                            CreatedOn = corp.DBTrack.CreatedOn != null ? corp.DBTrack.CreatedOn : null,
                            IsModified = false,
                            AuthorizedBy = SessionManager.UserName,
                            AuthorizedOn = DateTime.Now
                        };

                        db.TrainingEvaluation.Attach(corp);
                        db.Entry(corp).State = System.Data.Entity.EntityState.Deleted;


                        var rtn_Obj = DBTrackFile.DBTrackSave("Training/Training", null, db.ChangeTracker, corp.DBTrack);
                        DT_TrainingEvaluation DT_Corp = (DT_TrainingEvaluation)rtn_Obj;
                        db.Create(DT_Corp);
                        await db.SaveChangesAsync();
                        db.Entry(corp).State = System.Data.Entity.EntityState.Detached;
                        ts.Complete();
                        return Json(new Object[] { "", "Record Authorised", JsonRequestBehavior.AllowGet });
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
                List<string> Msg = new List<string>();

                TrainingEvaluation TrainingEvaluations = db.TrainingEvaluation.Where(e => e.Id == data).SingleOrDefault();

                if (TrainingEvaluations.DBTrack.IsModified == true)
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        DBTrack dbT = new DBTrack
                        {
                            Action = "D",
                            CreatedBy = TrainingEvaluations.DBTrack.CreatedBy != null ? TrainingEvaluations.DBTrack.CreatedBy : null,
                            CreatedOn = TrainingEvaluations.DBTrack.CreatedOn != null ? TrainingEvaluations.DBTrack.CreatedOn : null,
                            IsModified = TrainingEvaluations.DBTrack.IsModified == true ? true : false
                        };
                        TrainingEvaluations.DBTrack = dbT;
                        db.Entry(TrainingEvaluations).State = System.Data.Entity.EntityState.Modified;
                        var rtn_Obj = DBTrackFile.DBTrackSave("Training/Training", null, db.ChangeTracker, TrainingEvaluations.DBTrack);
                        DT_TrainingEvaluation DT_Corp = (DT_TrainingEvaluation)rtn_Obj;
                        db.Create(DT_Corp);
                        await db.SaveChangesAsync();
                        ts.Complete();
                        Msg.Add("  Data removed successfully.  ");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now,
                                CreatedBy = TrainingEvaluations.DBTrack.CreatedBy != null ? TrainingEvaluations.DBTrack.CreatedBy : null,
                                CreatedOn = TrainingEvaluations.DBTrack.CreatedOn != null ? TrainingEvaluations.DBTrack.CreatedOn : null,
                                IsModified = TrainingEvaluations.DBTrack.IsModified == true ? false : false//,
                            };

                            db.Entry(TrainingEvaluations).State = System.Data.Entity.EntityState.Deleted;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Training/Training", null, db.ChangeTracker, dbT);
                            DT_TrainingEvaluation DT_Corp = (DT_TrainingEvaluation)rtn_Obj;
                            db.Create(DT_Corp);
                            await db.SaveChangesAsync();
                            ts.Complete();
                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });

                        }
                        catch (RetryLimitExceededException /* dex */)
                        {
                            return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
                        }
                    }
                }
            }
        }

        public int EditS(int data, TrainingEvaluation c, DBTrack dbT)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var CurCorp = db.TrainingEvaluation.Find(data);
                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    c.DBTrack = dbT;
                    TrainingEvaluation corp = new TrainingEvaluation()
                    {
                        InterviewScore = c.InterviewScore,
                        WrittenScore = c.WrittenScore,
                        GDScore = c.GDScore,
                        EvaluationDetails = c.EvaluationDetails,
                        Id = data,
                        DBTrack = c.DBTrack
                    };


                    db.TrainingEvaluation.Attach(corp);
                    db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    return 1;
                }
                return 0;
            }
        }

        public void RollBack()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
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
                IEnumerable<TrainingEvaluation> TrainingEvaluation = null;
                if (gp.IsAutho == true)
                {
                    TrainingEvaluation = db.TrainingEvaluation.Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {
                    TrainingEvaluation = db.TrainingEvaluation.AsNoTracking().ToList();
                }

                IEnumerable<TrainingEvaluation> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = TrainingEvaluation;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Select(a => new { a.Id, a.GDScore, a.InterviewScore, a.WrittenScore }).Where((e => (e.Id.ToString() == gp.searchString) || (e.GDScore.ToString() == gp.searchString.ToLower()) || (e.InterviewScore.ToString() == gp.searchString.ToLower()))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.GDScore, a.InterviewScore, a.WrittenScore }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = TrainingEvaluation;
                    Func<TrainingEvaluation, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "GDScore" ? Convert.ToString(c.GDScore) :
                                         gp.sidx == "InterviewScore" ? Convert.ToString(c.InterviewScore) :
                                         gp.sidx == "InterviewScore" ? Convert.ToString(c.InterviewScore) :
                                         "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.GDScore), Convert.ToString(a.InterviewScore), a.WrittenScore }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.GDScore), Convert.ToString(a.InterviewScore), a.WrittenScore }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.GDScore, a.InterviewScore, a.WrittenScore }).ToList();
                    }
                    totalRecords = TrainingEvaluation.Count();
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


        public ActionResult GetLookup(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.TrainingEvaluation.ToList();


                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.TrainingEvaluation.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }
                //var list1 = db.FacultySpecialization.ToList();
                //var list2 = fall.Except(list1);
                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }


        public class DeserializeClass1
        {
            public int Id { get; set; }
            public string BatchName { get; set; }
            public string FullDetails { get; set; }
            public string ProgramList { get; set; }
            public int GDScore { get; set; }
            public int WrittenScore { get; set; }
            public int InterviewScore { get; set; }
            public string EvaluationDetails { get; set; }

        }

        public ActionResult Get_AppAssignData(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var db_data = db.EmployeeTraining
                      .Include(e => e.TrainingDetails)
                      .Include(e => e.TrainingDetails.Select(a => a.TrainingSchedule))
                      .Include(e => e.TrainingDetails.Select(t => t.TrainigDetailSessionInfo))
                      .Include(e => e.TrainingDetails.Select(t => t.TrainigDetailSessionInfo.Select(a => a.TrainingEvaluation)))
                      .Include(e => e.TrainingDetails.Select(t => t.TrainigDetailSessionInfo.Select(z => z.TrainingSession)))
                      .Include(e => e.TrainingDetails.Select(t => t.TrainigDetailSessionInfo.Select(z => z.TrainingSession.SessionType)))
                      .Include(e => e.TrainingDetails.Select(t => t.TrainigDetailSessionInfo.Select(z => z.TrainingSession.TrainingProgramCalendar.ProgramList)))
                        .Where(e => e.Id == data).SingleOrDefault();
                    List<DeserializeClass1> returndata = new List<DeserializeClass1>();
                    if (db_data != null)
                    {
                        foreach (var item in db_data.TrainingDetails)
                        {
                            foreach (var item2 in item.TrainigDetailSessionInfo)
                            {
                                if (item2.TrainingSession != null && item2.TrainingSession.TrainingProgramCalendar != null && item2.TrainingSession.TrainingProgramCalendar.ProgramList != null && item2.TrainingEvaluation != null)
                                {
                                    string ProgrameFulldetails = "Start Date:" + item2.TrainingSession.TrainingProgramCalendar.StartDate.Value.ToShortDateString() + ", End Date:"
                                                           + item2.TrainingSession.TrainingProgramCalendar.EndDate.Value.ToShortDateString() + ", Subject Details:" + item2.TrainingSession.TrainingProgramCalendar.ProgramList.SubjectDetails;

                                    returndata.Add(new DeserializeClass1
                                    {
                                        Id = item2.TrainingEvaluation.Id,
                                        BatchName = item.TrainingSchedule != null ? item.TrainingSchedule.TrainingBatchName : "",
                                        FullDetails = item2.TrainingSession.FullDetails,
                                        ProgramList = ProgrameFulldetails,
                                        GDScore = item2.TrainingEvaluation.GDScore,
                                        WrittenScore = item2.TrainingEvaluation.WrittenScore,
                                        InterviewScore = item2.TrainingEvaluation.InterviewScore,
                                        EvaluationDetails = item2.TrainingEvaluation.EvaluationDetails
                                    });
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


        public ActionResult GridEditData(int data, string batch)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var returnlist = new List<returnDataClass>();
                if (data != null && data != 0)
                {
                    var retrundataList = db.TrainingEvaluation.Where(e => e.Id == data).ToList();
                    //  var returnl = retrundataList.Select(a => a.EmpAppRating).ToList();
                    foreach (var a in retrundataList)
                    {

                        //var rp = a.Select(b => new { b.RatingPoints, b.Comments }).SingleOrDefault();
                        returnlist.Add(new returnDataClass()
                        {

                            GDScore = a.GDScore,
                            WrittenScore = a.WrittenScore,
                            InterviewScore = a.InterviewScore,
                            EvaluationDetails = a.EvaluationDetails,
                            Batchname = batch
                        });

                    }
                    return Json(new { returndata = returnlist }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return View();
                }
            }
        }


        [HttpPost]
        public ActionResult GridEditSave(TrainingEvaluation ITP, FormCollection form, string data) // Edit submit
        {


            List<String> Msg = new List<String>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var WrittenScore1 = form["WrittenScore1"] == "0" ? "" : form["WrittenScore1"];
                    var InterviewScore1 = form["InterviewScore1"] == "0" ? "" : form["InterviewScore1"];
                    var GDScore1 = form["GDScore1"] == null ? "" : form["GDScore1"];
                    var EvaluationDetails1 = form["EvaluationDetails1"] == "0" ? "" : form["EvaluationDetails1"];
                    var BatchName = form["batchname"] == "" ? null : form["batchname"];


                    int empidintrnal = 0;
                    int empidMain = 0;
                    if (data != null)
                    {
                        var ids = Utility.StringIdsToListIds(data);

                        empidintrnal = Convert.ToInt32(ids[0]);
                        empidMain = Convert.ToInt32(ids[1]);

                    }


                    bool Auth = form["Autho_Allow"] == "true" ? true : false;
                    int TrSchId = Convert.ToInt32(BatchName);
                    TrainingSchedule trSch = db.TrainingSchedule.Find(TrSchId);
                    if (trSch != null)
                    {
                        if (trSch.IsBatchClose == true)
                        {
                            return Json(new { status = false, responseText = "Batch is closed. You can't edit this record now..!" }, JsonRequestBehavior.AllowGet);
                        }
                    } 

                    var db_data = db.TrainingEvaluation.Where(e => e.Id == empidintrnal).SingleOrDefault();



                    db.TrainingEvaluation.Attach(db_data);
                    db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    TempData["RowVersion"] = db_data.RowVersion;
                    db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;


                    if (ModelState.IsValid)
                    {
                        //DbContextTransaction transaction = db.Database.BeginTransaction();

                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            TrainingEvaluation blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.TrainingEvaluation.Where(e => e.Id == empidintrnal)
                                                        .SingleOrDefault();
                                originalBlogValues = context.Entry(blog).OriginalValues;
                            }

                            ITP.DBTrack = new DBTrack
                            {
                                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                Action = "M",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };


                            var CurOBJ = db.TrainingEvaluation.Find(empidintrnal);
                            TempData["CurrRowVersion"] = CurOBJ.RowVersion;
                            db.Entry(CurOBJ).State = System.Data.Entity.EntityState.Detached;
                            if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                            {
                                TrainingEvaluation ESIOBJ = new TrainingEvaluation()
                                {
                                    Id = empidintrnal,
                                    GDScore = Convert.ToInt32(GDScore1),
                                    WrittenScore = Convert.ToInt32(WrittenScore1),
                                    InterviewScore = Convert.ToInt32(InterviewScore1),
                                    EvaluationDetails = EvaluationDetails1,
                                    DBTrack = ITP.DBTrack

                                };
                                db.TrainingEvaluation.Attach(ESIOBJ);
                                db.Entry(ESIOBJ).State = System.Data.Entity.EntityState.Modified;
                                db.Entry(ESIOBJ).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                //// DBTrackFile.DBTrackSave("Payroll", originalBlogValues, db.ChangeTracker, c.DBTrack);
                            }


                            using (var context = new DataBaseContext())
                            {

                                db.SaveChanges();
                            }
                            ts.Complete();
                            //Msg.Add("  Record Updated");
                            //return Json(new Utility.JsonReturnClass { Id = ITP.Id, Val = ITP.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            return Json(new { status = true, data = db_data, responseText = "Record Updated..!" }, JsonRequestBehavior.AllowGet);
                        }

                        Msg.Add("Record modified by another user.So refresh it and try to save again.");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        //return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                    }

                }
                catch (DbUpdateConcurrencyException ex)
                {
                    var entry = ex.Entries.Single();
                    var clientValues = (TrainingEvaluation)entry.Entity;
                    var databaseEntry = entry.GetDatabaseValues();
                    if (databaseEntry == null)
                    {
                        Msg.Add(ex.Message);
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //  return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                    }
                    else
                    {
                        var databaseValues = (TrainingEvaluation)databaseEntry.ToObject();
                        ITP.RowVersion = databaseValues.RowVersion;

                    }
                }
                catch (Exception e)
                {
                    Msg.Add(e.Message);
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
                return View();
            }
        }

        public ActionResult GridDelete(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();

                int empidintrnal = 0;
                int empidMain = 0;
                if (data != null)
                {
                    var ids = Utility.StringIdsToListIds(data);

                    empidintrnal = Convert.ToInt32(ids[0]);
                    empidMain = Convert.ToInt32(ids[1]);

                }
                var LvEP = db.TrainingEvaluation.Find(empidintrnal);
                try
                {
                    db.TrainingEvaluation.Remove(LvEP);
                    db.SaveChanges();
                    List<string> Msgs = new List<string>();
                    Msgs.Add("Record Deleted Successfully ");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msgs }, JsonRequestBehavior.AllowGet);
                }
                catch (DataException /* dex */)
                {
                    Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                }
            }
        }


    }
}