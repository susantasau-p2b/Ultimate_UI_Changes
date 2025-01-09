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
using Leave;
using P2BUltimate.Controllers;
using System.Linq.Dynamic;

namespace P2BUltimate.Controllers.Training.MainController
{
    public class TrainingAssignmentController : Controller
    {
        //
        List<String> Msg = new List<String>();
        // GET: /TrainingAssignment/
        public ActionResult Index()
        {
            return View("~/Views/Training/MainViews/TrainingAssignment/Index.cshtml");
        }
        //private DataBaseContext db = new DataBaseContext();

        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult Create(TrainingDetails NOBJ, FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string Emp = form["employee-table1"] == "0" ? "" : form["employee-table1"];
                string TrainingScheduleD = form["Training_Schedule"] == null ? "" : form["Training_Schedule"];
                string TrainingSessionDet = form["SessionListSch"] == null ? "" : form["SessionListSch"];
                string TrainingSrc = form["Training_Source_DDL"] == null ? "" : form["Training_Source_DDL"];
                int CompId = 0;
                if (!String.IsNullOrEmpty(Session["CompId"].ToString()))
                {
                    CompId = int.Parse(Session["CompId"].ToString());
                }

                NOBJ.TrainingSchedule = null;
                NOBJ.TrainigDetailSessionInfo = null;


                if (TrainingScheduleD != null && TrainingScheduleD != "")
                {
                    int Id = Convert.ToInt32(TrainingScheduleD);
                    var val = db.TrainingSchedule.Where(e => e.Id == Id).SingleOrDefault();
                    NOBJ.TrainingSchedule = val;
                }




                List<int> ids = null;
                if (Emp != null && Emp != "0" && Emp != "false")
                {
                    ids = Utility.StringIdsToListIds(Emp);
                }
                else
                {
                    //return Json(new Object[] { "", "", "Kindly select employee" }, JsonRequestBehavior.AllowGet);
                    Msg.Add(" Kindly select employee  ");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                }

                //if (db.TrainingDetails.Any(a => a.BatchName == NOBJ.BatchName))
                //{
                //    Msg.Add(" Batch Name Already Exist");
                //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                //}

                using (TransactionScope ts = new TransactionScope())
                {

                    Employee OEmployee = null;
                    EmployeeTraining emptr = null;
                    NOBJ.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                    foreach (var i in ids)
                    {
                        OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.GeoStruct.Company).Include(e => e.FuncStruct).Include(e => e.PayStruct)
                                    .Where(r => r.Id == i).SingleOrDefault();
                        emptr = db.EmployeeTraining.Include(r => r.TrainingDetails).Include(a => a.Employee)
                                .Include(e => e.EmpTrainingNeed).Include(e => e.EmpTrainingNeed.Select(r => r.ProgramList))
                               .Where(e => e.Employee.Id == OEmployee.Id).SingleOrDefault();

                        if (TrainingSessionDet != null && TrainingSessionDet != "")
                        {
                            List<TrainingSession> DetSessionlst = new List<TrainingSession>();
                            List<TrainigDetailSessionInfo> DetSessionInfolst = new List<TrainigDetailSessionInfo>();
                            var idsD = Utility.StringIdsToListIds(TrainingSessionDet);
                            foreach (var ca in idsD)
                            {
                                var DetSessionlst_val = db.TrainingSession.Find(ca);

                                TrainigDetailSessionInfo TrSession = new TrainigDetailSessionInfo()
                                {
                                    DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false },
                                    TrainingSession = DetSessionlst_val
                                };

                                DetSessionInfolst.Add(TrSession);

                            }
                            NOBJ.TrainigDetailSessionInfo = DetSessionInfolst;
                        }
                        else
                        {
                            NOBJ.TrainigDetailSessionInfo = null;
                        }

                        TrainingDetails td = new TrainingDetails()
                        {

                            //BatchName = NOBJ.BatchName,
                            TrainingSchedule = NOBJ.TrainingSchedule,
                            TrainigDetailSessionInfo = NOBJ.TrainigDetailSessionInfo,
                            TrainingCalendar = db.Calendar.Where(e => e.Default == true && e.Name.LookupVal.ToUpper() == "TRAININGCALENDAR").SingleOrDefault(),
                            FuncStruct = OEmployee.FuncStruct,
                            PayStruct = OEmployee.PayStruct,
                            GeoStruct = OEmployee.GeoStruct,
                            DBTrack = NOBJ.DBTrack

                        };
                        db.TrainingDetails.Add(td);
                        db.SaveChanges();
                        int SrcId = Convert.ToInt32(TrainingSrc);

                        //  int TrSessionId = Convert.ToInt32(TrainingSessionDet);
                        var TrSessionId = Utility.StringIdsToListIds(TrainingSessionDet);
                        List<int> PgmLstIdlist = new List<int>();
                        var PgmLstId = db.TrainingSession.Include(e => e.TrainingProgramCalendar)
                       .Include(e => e.TrainingProgramCalendar.ProgramList).Where(e => TrSessionId.Contains(e.Id))
                       .ToList();

                        foreach (var item in PgmLstId)
                        {
                            PgmLstIdlist.Add(item.TrainingProgramCalendar.ProgramList.Id);
                        }


                        string TrSrcLkVal = db.LookupValue.Find(SrcId).LookupVal.ToString().ToUpper();
                       
                        if (TrSrcLkVal == "TRAININGNEED")
                        {
                            foreach (var PgmLst in PgmLstIdlist.Distinct())
                            {


                                EmpTrainingNeed EmpTrNeed = emptr.EmpTrainingNeed.Where(e => e.ProgramList.Id == PgmLst)
                                    .SingleOrDefault();
                                EmpTrNeed.AcceptanceDate = DateTime.Now;
                                EmpTrNeed.IsAccepted = true;
                                EmpTrNeed.TrainingSchedule = NOBJ.TrainingSchedule;
                                db.EmpTrainingNeed.Attach(EmpTrNeed);
                                db.Entry(EmpTrNeed).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                            }
                            //db.Entry(emptr).State = System.Data.Entity.EntityState.Detached;
                        }

                        List<TrainingDetails> trdetails = new List<TrainingDetails>();
                        trdetails.Add(td);


                        if (emptr == null)
                        {
                            EmployeeTraining OTEP = new EmployeeTraining()
                            {
                                Employee = OEmployee,
                                TrainingDetails = trdetails,
                                DBTrack = NOBJ.DBTrack,

                            };
                            db.EmployeeTraining.Add(OTEP);
                            db.SaveChanges();
                        }

                        else
                        {
                            if (emptr.TrainingDetails != null)
                            {
                                trdetails.AddRange(emptr.TrainingDetails);
                            }
                            emptr.TrainingDetails = trdetails;
                            db.EmployeeTraining.Attach(emptr);
                            db.Entry(emptr).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            db.Entry(emptr).State = System.Data.Entity.EntityState.Detached;
                        }



                    }


                    // db.Entry(td).State = System.Data.Entity.EntityState.Added;



                    ts.Complete();
                    //db.TrainingDetails.Add(td);
                }
                Msg.Add("Data Saved Successfully.");
                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

            }
        }

        public ActionResult GetLookupValue(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                int trainingschid = Convert.ToInt32(data);
                List<LookupValue> lookupids = new List<LookupValue>();
                TrainingSchedule emptraininglookupsource = db.TrainingSchedule
                                                          .Include(e => e.TrainingEmployeeSource)
                                                          .Include(e => e.TrainingEmployeeSource.Select(t => t.EmployeeTrainingSource))
                                                          .Where(e => e.Id == trainingschid).SingleOrDefault();
                if (emptraininglookupsource != null)
                {
                    lookupids = emptraininglookupsource.TrainingEmployeeSource.Where(e => e.EmployeeTrainingSource.IsActive == true).Select(e => e.EmployeeTrainingSource).ToList();
                }


                SelectList s = (SelectList)null;
                var selected = "";
                if (data != "" && data != null)
                {
                    // var qurey = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == data).SingleOrDefault();
                    //var qurey = db.Lookup.Where(e => e.Code == data).Select(e => e.LookupValues.Where(r => r.IsActive == true)).SingleOrDefault(); // added by rekha 26-12-16
                    //if (data2 != "" && data2 != "0")
                    //{
                    //    selected = data2;
                    //}
                    if (lookupids.Count() > 0)
                    {
                        s = new SelectList(lookupids, "Id", "LookupVal", selected);
                    }
                }
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }

        public class schedule
        {
            public int srno { get; set; }
            public string lookupvalue { get; set; }
        }

        //public ActionResult Gettrschedule(string data, string parm)
        //{

        //    int yearlyprogassingId = Convert.ToInt16(parm);
        //    using (DataBaseContext db = new DataBaseContext())
        //    {

        //        var fall = db.TrainingSchedule.Include(a => a.City).Include(a => a.Expenses)
        //            //.Include(a => a.TrainingCalendar)
        //            .Include(a => a.TrainingSession)
        //            .Include(a => a.TrainingSession.Select(r => r.SessionType))
        //            .Where(q => q.TrainingCalendar.Id == yearlyprogassingId).ToList();
        //        IEnumerable<TrainingSchedule> all;
        //        if (!string.IsNullOrEmpty(data))
        //        {
        //            //all = db.TrainingSchedule.Where(q => q.TrainingCalendar.Id == yearlyprogassingId).ToList();

        //            var result = (from c in all
        //                          select new { c.Id, c.FullDetails }).Distinct();
        //            return Json(result, JsonRequestBehavior.AllowGet);
        //        }
        //        else
        //        {
        //            List<schedule> schlist = new List<schedule>();


        //            foreach (var item in fall)
        //            {
        //                string asca = item.Session.Select(q => q.FullDetails).SingleOrDefault();
        //                schlist.Add(
        //                 new schedule
        //                 {
        //                     srno = item.Id,
        //                     lookupvalue = "Sesion Type :" + asca + "" + item.FullDetails 
        //                 });
        //            }
        //                return Json(schlist, JsonRequestBehavior.AllowGet);
        //        }
        //        return View();
        //        //var resulta = (from c in schlist
        //        //              select new { c.Id, c.FullDetails }).Distinct();
        //        //return Json(resulta, JsonRequestBehavior.AllowGet);
        //    }

        //}

        //public ActionResult GetTrainingSchDetailLKDetails(string data)
        //{
        //    if (yearlyprogassingId == 0)
        //    {
        //        Msg.Add(" Kindly select Yearly Program Assignment ");
        //        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //    }
        //    else
        //    {
        //        using (DataBaseContext db = new DataBaseContext())
        //        {
        //            var fall = db.TrainingSchedule.Where(e => e.TrainingCalendar.Id == yearlyprogassingId).ToList();
        //            IEnumerable<TrainingSchedule> all;
        //            if (!string.IsNullOrEmpty(data))
        //            {
        //                all = db.TrainingSchedule.Where(e => e.TrainingCalendar.Id == yearlyprogassingId).ToList();

        //            }
        //            else
        //            {
        //                var list1 = db.TrainingSchedule.Include(e => e.TrainingCalendar).Where(e => e.TrainingCalendar.Id == yearlyprogassingId).ToList();
        //                // var list2 = fall.Except(list1);
        //                //var details = "";
        //                //foreach (var a in list2)
        //                //{
        //                //    details="Start Date: "+a.StartDate.Value.ToShortDateString()+",End Date: "+a.EndDate.Value.ToShortDateString()+"Program List: "+ a.ProgramList.FullDetails+"."; 
        //                //}
        //                var r = (from a in list1 select new { srno = a.Id, lookupvalue = a.FullDetails }).Distinct();
        //                return Json(r, JsonRequestBehavior.AllowGet);
        //            }
        //            var result = (from c in all
        //                          select new { c.Id, c.FullDetails }).Distinct();
        //            return Json(result, JsonRequestBehavior.AllowGet);
        //        }
        //    }

        //}

        public class P2BGridData
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
        }

        public ActionResult P2BGrid(P2BGrid_Parameters gp)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    int Id = Convert.ToInt32(gp.id);
                    int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                    int pageSize = gp.rows;
                    int totalPages = 0;
                    int totalRecords = 0;
                    var jsonData = (Object)null;

                    IEnumerable<P2BGridData> ITProjectionList = null;
                    List<P2BGridData> model = new List<P2BGridData>();
                    P2BGridData view = null;
                    string PayMonth = "";

                    if (gp.filter != null)
                        PayMonth = gp.filter;
                    if (PayMonth != null && PayMonth != "")
                    {

                        var BatchName = db.TrainingDetails.Find(int.Parse(PayMonth));

                        //     var BindEmpList = db.TrainingDetails.Include(e => e.EmployeeID).Include(e => e.EmployeeID.EmpName).Where(e => e.BatchName == BatchName).ToList();

                        //   var emp = db.EmployeeTraining.Include(a => a.TrainingDetails).Include(a => a.Employee).ToList();

                        var EmpTr = db.EmployeeTraining.Include(e => e.TrainingDetails).Include(e => e.Employee).Include(e => e.Employee.EmpName).ToList();
                        List<Employee> Emp = new List<Employee>();
                        foreach (var E in EmpTr)
                        {
                            //var TrDet = E.TrainingDetails.Where(e => e.BatchName == BatchName).SingleOrDefault();
                            //if (TrDet != null)
                            //{
                            //    Emp.Add(E.Employee);
                            //}
                        }


                        foreach (var z in Emp)
                        {

                            view = new P2BGridData()
                            {
                                Id = z.Id,
                                Code = z.EmpCode,
                                Name = z.EmpName.FullNameFML
                            };

                            model.Add(view);

                        }

                    }
                    else
                    {
                        List<string> Msgu = new List<string>();
                        Msgu.Add("  BatchName Not Selected ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msgu }, JsonRequestBehavior.AllowGet);
                        //return Json(new Object[] { "", "", "Financial Year Not Selected", JsonRequestBehavior.AllowGet });
                    }
                    ITProjectionList = model;

                    IEnumerable<P2BGridData> IE;
                    if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                    {
                        IE = ITProjectionList;
                        if (gp.searchOper.Equals("eq"))
                        {
                            jsonData = IE.Where(e => (e.Id.ToString().Contains(gp.searchString))
                                    || (e.Code.ToString().Contains(gp.searchString))
                                    || (e.Name.ToString().Contains(gp.searchString))
                                    ).Select(a => new Object[] { a.Id, a.Code, a.Name }).ToList();
                            //jsonData = IE.Select(a => new  { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, Convert.ToString(a.Code), a.Name }).ToList();
                        }
                        totalRecords = IE.Count();
                    }
                    else
                    {
                        IE = ITProjectionList;
                        Func<P2BGridData, dynamic> orderfuc;
                        if (gp.sidx == "Id")
                        {
                            orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                        }
                        else
                        {
                            orderfuc = (c => gp.sidx == "Code" ? c.Code.ToString() :
                                             gp.sidx == "Name" ? c.Name.ToString() :
                                             "");
                        }
                        if (gp.sord == "asc")
                        {
                            IE = IE.OrderBy(orderfuc);
                            jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), a.Name }).ToList();
                        }
                        else if (gp.sord == "desc")
                        {
                            IE = IE.OrderByDescending(orderfuc);
                            jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), a.Name }).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, Convert.ToString(a.Code) }).ToList();
                        }
                        totalRecords = ITProjectionList.Count();
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
                    List<string> Msg = new List<string>();
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
        public ActionResult GetCalendarLKDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //List<string> Ids = SkipIds.ToString();
                var fall = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "Trainingcalendar".ToUpper()).ToList();

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = (db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "Trainingcalendar".ToUpper())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }

                //var list1 = db.TrainingDetails.ToList().Select(e => e.);
                //var list2 = fall.Except(list1);

                //  var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.LookupVal }).Distinct();
                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetTrainingSource(string data)
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

                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.TrainingBatchName }).OrderByDescending(d => d.srno).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.TrainingBatchName }).OrderByDescending(i => i.Id).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetTrainingSource1(string data)
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


        //public ActionResult GetLookupDetailsSessionInfo(List<int> SkipIds)
        //{
        //    List<string> Msg = new List<string>();
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var fall = new List<TrainingSession>();
        //        if (SkipIds != null)
        //        {
        //            foreach (var a in SkipIds)
        //            {
        //                if (fall == null)
        //                    fall = db.TrainingSchedule.Include(e => e.TrainingSession).Include(e => e.TrainingSession.Select(t => t.SessionType)).Where(e => !e.Id.ToString().Contains(a.ToString())).SelectMany(h => h.TrainingSession).ToList();
        //                else
        //                    fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

        //            }
        //        }
        //        var list1 = db.TrainingSession.Include(e => e.SessionType).ToList();
        //        var r = (from ca in list1 select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
        //        return Json(r, JsonRequestBehavior.AllowGet);
        //    }
        //}

        public ActionResult GetLookupDetailsSessionInfo(string data, int ts)
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

        public ActionResult getCalendar()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var qurey = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "Trainingcalendar".ToUpper() && e.Default == true).AsEnumerable()
                    .Select(e => new
                    {
                        Id = e.Id,
                        Lvcalendardesc = "From Date :" + e.FromDate.Value.ToShortDateString() + " To Date :" + e.ToDate.Value.ToShortDateString(),

                    }).SingleOrDefault();
                return Json(qurey, JsonRequestBehavior.AllowGet);
            }
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

        public ActionResult Emp_Grid(ParamModel param, string y)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<EmployeeTraining> all = new List<EmployeeTraining>();
                List<EmployeeTraining> FilterEmployee = new List<EmployeeTraining>();

                try
                {
                    if (y != "")
                    {
                        int scheduleids = Convert.ToInt32(y);
                        all = db.EmployeeTraining
                            .Include(e => e.Employee)
                            .Include(e => e.Employee.EmpName)
                            .Include(e => e.Employee.ServiceBookDates)
                            .Include(e => e.Employee.GeoStruct)
                            .Include(e => e.Employee.GeoStruct.Location.LocationObj)
                            .Include(e => e.Employee.FuncStruct)
                            .Include(e => e.Employee.FuncStruct.Job)
                            .Include(e => e.Employee.PayStruct)
                            .Include(e => e.Employee.PayStruct.Grade)
                            .Include(e => e.TrainingDetails)
                            .Include(e => e.TrainingDetails.Select(t => t.TrainingSchedule))
                            .Include(e => e.TrainingDetails.Select(t => t.TrainingCalendar))
                            .ToList();

                        foreach (var item in all)
                        {
                            var filterschedulewiseemp = item.TrainingDetails.Where(t => t.TrainingSchedule.Id == scheduleids).ToList();
                            if (filterschedulewiseemp.Count() > 0)
                            {
                                FilterEmployee.Add(item);
                            }
                        }
                    }
                    else
                    {
                        FilterEmployee = db.EmployeeTraining
                            .Include(e => e.Employee)
                            .Include(e => e.Employee.EmpName)
                            .Include(e => e.Employee.ServiceBookDates)
                            .Include(e => e.Employee.GeoStruct)
                            .Include(e => e.Employee.GeoStruct.Location.LocationObj)
                            .Include(e => e.Employee.FuncStruct)
                            .Include(e => e.Employee.FuncStruct.Job)
                            .Include(e => e.Employee.PayStruct)
                            .Include(e => e.Employee.PayStruct.Grade)
                            .Include(e => e.TrainingDetails)
                            .Include(e => e.TrainingDetails.Select(t => t.TrainingSchedule))
                            .Include(e => e.TrainingDetails.Select(t => t.TrainingCalendar))
                            .ToList();
                    }
                    // for searchs
                    IEnumerable<EmployeeTraining> fall;
                    if (param.sSearch == null)
                    {
                        fall = FilterEmployee;
                    }
                    else
                    {
                        //fall = all.Where(e => (e.Employee.EmpCode == param.sSearch) || (e.Employee.EmpName.FullNameFML.ToUpper() == param.sSearch.ToUpper())).ToList();
                        fall = FilterEmployee.Where(e => (e.Id.ToString().Contains(param.sSearch))
                               || (e.Employee.EmpCode.Contains(param.sSearch))
                               || (e.Employee.EmpName.FullNameFML.ToUpper().Contains(param.sSearch.ToUpper()))
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
                            //if (y == "" || y == null)
                            //{
                            result.Add(new returndatagridclass
                            {
                                Id = item.Id.ToString(),
                                Code = item.Employee.EmpCode,
                                Name = item.Employee.EmpName != null ? item.Employee.EmpName.FullNameFML : null,
                                JoiningDate = item.Employee.ServiceBookDates != null && item.Employee.ServiceBookDates.JoiningDate != null ? item.Employee.ServiceBookDates.JoiningDate.Value.ToShortDateString() : null,
                                Job = item.Employee.FuncStruct != null && item.Employee.FuncStruct.Job != null ? item.Employee.FuncStruct.Job.Name : null,
                                Grade = item.Employee.PayStruct != null && item.Employee.PayStruct.Grade != null ? item.Employee.PayStruct.Grade.Name : null,
                                Location = item.Employee.GeoStruct != null && item.Employee.GeoStruct.Location != null && item.Employee.GeoStruct.Location.LocationObj != null ? item.Employee.GeoStruct.Location.LocationObj.LocDesc : null
                            });
                            // }
                            //else
                            //{
                            //    foreach (var item1 in item.TrainingDetails)
                            //    {
                            //        if (item1.TrainingSchedule != null && item1.TrainingSchedule.TrainingBatchName == y)
                            //        {
                            //            result.Add(new returndatagridclass
                            //            {
                            //                Id = item.Id.ToString(),
                            //                Code = item.Employee.EmpCode,
                            //                Name = item.Employee.EmpName != null ? item.Employee.EmpName.FullNameFML : null,
                            //                JoiningDate = item.Employee.ServiceBookDates != null && item.Employee.ServiceBookDates.JoiningDate != null ? item.Employee.ServiceBookDates.JoiningDate.Value.ToShortDateString() : null,
                            //                Job = item.Employee.FuncStruct != null && item.Employee.FuncStruct.Job != null ? item.Employee.FuncStruct.Job.Name : null,
                            //                Grade = item.Employee.PayStruct != null && item.Employee.PayStruct.Grade != null ? item.Employee.PayStruct.Grade.Name : null,
                            //                Location = item.Employee.GeoStruct != null && item.Employee.GeoStruct.Location != null && item.Employee.GeoStruct.Location.LocationObj != null ? item.Employee.GeoStruct.Location.LocationObj.LocDesc : null
                            //            });
                            //        }
                            //    }

                            //}

                        }
                        return Json(new
                        {
                            sEcho = param.sEcho,
                            iTotalRecords = FilterEmployee.Count(),
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
                            iTotalRecords = FilterEmployee.Count(),
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

        [HttpPost]
        public async Task<ActionResult> Delete(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        var ids = Utility.StringIdsToListIds(data);
                        foreach (var ca in ids)
                        {
                            var emptraining = db.EmployeeTraining.Include(e => e.TrainingDetails)
                                                .Include(e => e.TrainingDetails.Select(q => q.TrainingSchedule))
                                                .Include(e => e.Employee)
                                                .Where(q => q.Employee.Id == ca).SingleOrDefault();

                            var traischid = emptraining.TrainingDetails.Select(e => e.TrainingSchedule).SingleOrDefault();

                            int traindetailid = emptraining.TrainingDetails.Select(e => e.Id).SingleOrDefault();

                            db.EmployeeTraining.Remove(emptraining);
                            //foreach (var item in traischid)
                            //{
                            //    var aa = db.TrainingSchedule.Where(e => e.Id == item.Id);
                            //    db.TrainingSchedule.RemoveRange(aa);
                            //}
                            //db.SaveChanges();
                            // var sch=db.TrainingDetails.Include(e=>e.TrainingSchedule).ToList();

                            //   db.TrainingDetails.RemoveRange(emptraining.TrainingDetails);
                            var tra = db.TrainingDetails.Where(e => e.Id == traindetailid).SingleOrDefault();
                            db.TrainingDetails.Remove(tra);
                            db.SaveChanges();
                        }
                        ts.Complete();

                        Msg.Add("  Data removed.  ");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    //    if (emptraining.DBTrack.IsModified == true)
                    //    {
                    //        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    //        {
                    //            DBTrack dbT = new DBTrack
                    //            {
                    //                Action = "D",
                    //                CreatedBy = emptraining.DBTrack.CreatedBy != null ? emptraining.DBTrack.CreatedBy : null,
                    //                CreatedOn = emptraining.DBTrack.CreatedOn != null ? emptraining.DBTrack.CreatedOn : null,
                    //                IsModified = emptraining.DBTrack.IsModified == true ? true : false
                    //            };
                    //            emptraining.DBTrack = dbT;
                    //            db.Entry(emptraining).State = System.Data.Entity.EntityState.Modified;
                    //            var rtn_Obj = DBTrackFile.DBTrackSave("Training/Training", null, db.ChangeTracker, emptraining.DBTrack);
                    //            db.SaveChanges();
                    //            ts.Complete();
                    //            Msg.Add("  Data removed.  ");
                    //            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //        }
                    //    }
                    //    else
                    //    {
                    //        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    //        {
                    //            DBTrack dbT = new DBTrack
                    //            {
                    //                Action = "D",
                    //                ModifiedBy = SessionManager.UserName,
                    //                ModifiedOn = DateTime.Now,
                    //                CreatedBy = emptraining.DBTrack.CreatedBy != null ? emptraining.DBTrack.CreatedBy : null,
                    //                CreatedOn = emptraining.DBTrack.CreatedOn != null ? emptraining.DBTrack.CreatedOn : null,
                    //                IsModified = emptraining.DBTrack.IsModified == true ? false : false//,
                    //                //AuthorizedBy = SessionManager.UserName,
                    //                //AuthorizedOn = DateTime.Now
                    //            };


                    //            db.Entry(emptraining).State = System.Data.Entity.EntityState.Deleted;
                    //            var rtn_Obj = DBTrackFile.DBTrackSave("Training/Training", null, db.ChangeTracker, dbT);
                    //            db.SaveChanges();
                    //            ts.Complete();
                    //            //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                    //            Msg.Add("  Data removed.  ");
                    //            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //        }
                    //    }
                    //}
                }
                catch (RetryLimitExceededException /* dex */)
                {
                    Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
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


        [HttpPost]
        public async Task<ActionResult> DeleteAssingnedTrainingDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        var ids = Utility.StringIdsToListIds(data);
                        foreach (var ca in ids)
                        {
                            var emptraining = db.EmployeeTraining.Include(e => e.TrainingDetails)
                                                .Include(e => e.TrainingDetails.Select(q => q.TrainingSchedule))
                                                .Include(e => e.TrainingDetails.Select(ti => ti.TrainigDetailSessionInfo))
                                                .Include(e => e.Employee)

                                                .Where(q => q.Employee.Id == ca).SingleOrDefault();
                            var TrainingDetailsinfo = emptraining.TrainingDetails.FirstOrDefault();
                            if (TrainingDetailsinfo != null)
                            {
                                var TRDSessionInfo = TrainingDetailsinfo.TrainigDetailSessionInfo.FirstOrDefault();

                                var Train_Details = db.TrainingDetails.Where(e => e.Id == TrainingDetailsinfo.Id).SingleOrDefault();
                                if (TRDSessionInfo.IsPresent == true)
                                {
                                    Msg.Add(" For This Training Session, Employees are Already Present. So, You Can not Delete !!!  ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }
                                else if (TRDSessionInfo.IsCancelled == true)
                                {
                                    Msg.Add(" This Training Session, Already Cancelled. So, You Can not Delete !!!  ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }

                                var TrainDetails_Sessioninfo = db.TrainigDetailSessionInfo.Where(e => e.Id == TRDSessionInfo.Id).SingleOrDefault();

                                db.TrainingDetails.Remove(Train_Details);
                                db.TrainigDetailSessionInfo.Remove(TrainDetails_Sessioninfo);
                            }

                            await db.SaveChangesAsync();
                        }
                        ts.Complete();

                        Msg.Add("  Data removed.  ");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                }
                catch (RetryLimitExceededException /* dex */)
                {
                    Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
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


        public class empdetails
        {
            public int Id { get; set; }
            public string EmpCode { get; set; }
            public string EmpName { get; set; }
            public string Training { get; set; }
        }


        public ActionResult Get_Employelist(string geo_id, string trainingsource, string trainingsch, string trainingsession)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                DateTime? dt = null;
                string monthyr = "";
                DateTime? dtChk = null;
                int lastDayOfMonth;

                var Serialize = new JavaScriptSerializer();
                var deserialize = Serialize.Deserialize<Utility.GridParaStructIdClass>(geo_id);

                if (deserialize != null && deserialize.Filter != null)
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

                //List<Employee> data = new List<Employee>();
                List<Employee> dataTemp = new List<Employee>();
                var compid = Convert.ToInt32(Session["CompId"].ToString());
                db.Database.CommandTimeout = 3600;

                var empdata = db.CompanyTraining.Where(e => e.Company.Id == compid)
                    .Include(e => e.EmployeeTraining)
                    .Include(e => e.EmployeeTraining.Select(t => t.TrainingDetails))
                    .Include(e => e.EmployeeTraining.Select(t => t.TrainingDetails.Select(y => y.TrainingSchedule)))
                    .Include(e => e.EmployeeTraining.Select(t => t.TrainingDetails.Select(x => x.TrainigDetailSessionInfo)))
                    .Include(e => e.EmployeeTraining.Select(t => t.TrainingDetails.Select(x => x.TrainigDetailSessionInfo.Select(y => y.TrainingSession))))
                    .Include(e => e.EmployeeTraining.Select(a => a.Employee))
                    .Include(e => e.EmployeeTraining.Select(a => a.Employee.GeoStruct))
                    .Include(e => e.EmployeeTraining.Select(a => a.Employee.PayStruct))
                    .Include(e => e.EmployeeTraining.Select(a => a.Employee.FuncStruct))
                    //.Include(e => e.EmployeeTraining.Select(a => a.EmpTrainingNeed))
                    //.Include(e => e.EmployeeTraining.Select(a => a.EmpTrainingNeed.Select(t => t.ProgramList)))
                    //.Include(e => e.EmployeeTraining.Select(a => a.EmpTrainingNeed.Select(r => r.EmployeeTrainingSource)))
                    //.Include(e => e.EmployeeTraining.Select(a => a.EmpTrainingNeed.Select(r => r.TrainingSchedule)))
                    //.Include(e => e.EmployeeTraining.Select(a => a.EmpTrainingNeed.Select(r => r.TrainingSchedule)))
                    //.Include(e => e.EmployeeTraining.Select(a => a.EmpTrainingNeed.Select(r => r.TrainingSchedule.TrainingSession)))
                    .Include(e => e.EmployeeTraining.Select(a => a.Employee.EmpName))
                    .Include(e => e.EmployeeTraining.Select(a => a.Employee.ServiceBookDates)).AsNoTracking().AsParallel()
                    .SingleOrDefault();

                if (trainingsource != null)
                {
                    int TrSch = Convert.ToInt32(trainingsch);
                    //  int TrSession = Convert.ToInt32(trainingsession);
                    var TrSession = Utility.StringIdsToListIds(trainingsession);
                    List<int> PgmLstIdlist = new List<int>();

                    int id = Convert.ToInt32(trainingsource);

                    string TrSrcLkVal = db.LookupValue.Find(id).LookupVal.ToString().ToUpper();
                    var PgmLstId = db.TrainingSession.Include(e => e.TrainingProgramCalendar)
                        .Include(e => e.TrainingProgramCalendar.ProgramList).Where(e => TrSession.Contains(e.Id))
                        .ToList();
                    //.TrainingProgramCalendar.ProgramList.Id
                    foreach (var item in PgmLstId)
                    {
                        PgmLstIdlist.Add(item.TrainingProgramCalendar.ProgramList.Id);
                    }

                    if (TrSrcLkVal == "ORGANIZATION")
                    {
                        foreach (var item in empdata.EmployeeTraining)
                        {
                            if (item.Employee.ServiceBookDates.ServiceLastDate == null || (item.Employee.ServiceBookDates.ServiceLastDate != null &&
                                item.Employee.ServiceBookDates.ServiceLastDate.Value >= dtChk))
                            {
                                if (item.TrainingDetails.Count() > 0)
                                {
                                    int count = 0;
                                    foreach (var item1 in item.TrainingDetails)
                                    {
                                        if (item1.TrainingSchedule.Id != TrSch)
                                        {
                                            dataTemp.Add(item.Employee);
                                        }
                                        else if (item1.TrainigDetailSessionInfo.Where(e => !TrSession.Contains(e.TrainingSession.Id)).ToList().Count() == 0)
                                        {
                                            count = +1;
                                        }
                                    }
                                    if (count == 0)
                                    {
                                        dataTemp.Add(item.Employee);
                                    }
                                }
                                else
                                {
                                    dataTemp.Add(item.Employee);
                                }
                            }

                        }
                    }

                    if (TrSrcLkVal == "TRAININGNEED")
                    {

                        foreach (var item in empdata.EmployeeTraining)
                        {
                            if (item.Employee.ServiceBookDates.ServiceLastDate == null || (item.Employee.ServiceBookDates.ServiceLastDate != null &&
                                item.Employee.ServiceBookDates.ServiceLastDate.Value >= dtChk))
                            {
                                foreach (var item1 in item.EmpTrainingNeed.Where(e => e.TrClosed == true && e.IsCancel == false).ToList())
                                {

                                    if (item1.EmployeeTrainingSource != null)
                                    {
                                        if (item1.EmployeeTrainingSource.Id == id)
                                        {
                                            if (item1.TrainingSchedule == null)
                                            {
                                                var prgidchk = PgmLstIdlist.Contains(item1.ProgramList.Id);
                                                //  if (item1.ProgramList.Id == PgmLstId)
                                                if (prgidchk == true)
                                                {
                                                    dataTemp.Add(item.Employee);
                                                }
                                            }
                                            else if (item1.TrainingSchedule != null)
                                            {
                                                int Count = 0;
                                                foreach (var item3 in item.TrainingDetails)
                                                {
                                                    if (item3.TrainigDetailSessionInfo.Where(e => !TrSession.Contains(e.TrainingSession.Id)).ToList().Count() == 0)
                                                    {
                                                        Count = +1;
                                                    }
                                                }
                                                if (Count == 0)
                                                {
                                                    dataTemp.Add(item.Employee);
                                                }
                                                //if (item1.TrainingSchedule.Id != TrSch)
                                                //{
                                                //    dataTemp.Add(item.Employee);
                                                //}

                                            }


                                        }
                                    }
                                }
                            }
                        }
                    }
                }



                var TrSchdet = db.EmployeeTraining.Include(e => e.TrainingDetails).Include(e => e.TrainingDetails.Select(r => r.TrainingSchedule)).ToList();

                List<EmployeeTraining> List_all = new List<EmployeeTraining>();
                List<EmployeeTraining> All_Emp = new List<EmployeeTraining>();
                //if (geo_id != null && geo_id != "")
                //{
                //var Serialize = new JavaScriptSerializer();
                //var deserialize = Serialize.Deserialize<Utility.GridParaStructIdClass>(geo_id);
                if (geo_id != null && geo_id != "")
                {
                    if (deserialize.GeoStruct != null)
                    {
                        var one_id = Utility.StringIdsToListIds(deserialize.GeoStruct);
                        foreach (var ca in one_id)
                        {
                            var id = Convert.ToInt32(ca);
                            var List_all_temp = empdata.EmployeeTraining.Where(e => e.Employee.GeoStruct != null && e.Employee.GeoStruct.Id == id).ToList();
                            if (List_all_temp != null && List_all_temp.Count != 0)
                            {
                                List_all.AddRange(List_all_temp);
                            }
                        }
                    }
                    if (deserialize.PayStruct != null)
                    {
                        var one_id = Utility.StringIdsToListIds(deserialize.PayStruct);
                        var List_all_temp = new List<EmployeeTraining>();
                        if (List_all.Count > 0)
                        {
                            List_all_temp = List_all.Where(e => e.Employee.PayStruct != null && one_id.Contains(e.Employee.PayStruct.Id)).ToList();
                        }
                        else
                        {
                            List_all_temp = empdata.EmployeeTraining.Where(e => e.Employee.PayStruct != null && one_id.Contains(e.Employee.PayStruct.Id)).ToList();
                        }
                        if (List_all_temp != null && List_all_temp.Count != 0)
                        {
                            List_all = List_all_temp;
                        }

                    }
                    if (deserialize.FunStruct != null)
                    {
                        var one_id = Utility.StringIdsToListIds(deserialize.FunStruct);
                        var List_all_temp = new List<EmployeeTraining>();
                        if (List_all.Count > 0)
                        {
                            List_all_temp = List_all.Where(e => e.Employee.FuncStruct != null && one_id.Contains(e.Employee.FuncStruct.Id)).ToList();
                        }
                        else
                        {
                            List_all_temp = empdata.EmployeeTraining.Where(e => e.Employee.FuncStruct != null && one_id.Contains(e.Employee.FuncStruct.Id)).ToList();
                        }
                        //var List_all_temp = List_all.Where(e => e.Employee.FuncStruct != null && one_id.Contains(e.Employee.FuncStruct.Id)).ToList();
                        if (List_all_temp != null && List_all_temp.Count != 0)
                        {
                            List_all = List_all_temp;
                        }

                    }
                }

                List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                if (List_all != null && List_all.Count != 0)
                {
                    List<Employee> data = new List<Employee>();
                    data = List_all.Select(e => e.Employee).ToList();
                    if (deserialize != null && deserialize.CheckAll == "check")
                    {
                        foreach (var item in data.Distinct().OrderBy(a => a.EmpCode))
                        {
                            returndata.Add(new Utility.returndataclass
                            {
                                code = item.Id.ToString(),
                                value = item.FullDetails,
                            });
                        }
                    }
                    else
                    {
                        foreach (var item in data.Distinct().OrderBy(a => a.EmpCode))
                        {
                            if (item.ServiceBookDates.ServiceLastDate == null || (item.ServiceBookDates.ServiceLastDate != null &&
                                item.ServiceBookDates.ServiceLastDate.Value > dtChk))
                            {
                                returndata.Add(new Utility.returndataclass
                                {
                                    code = item.Id.ToString(),
                                    value = item.FullDetails,
                                });
                            }
                        }
                    }

                    var returnjson = new
                    {
                        data = returndata,
                        tablename = "employee-table1"
                    };
                    return Json(new List<Object> { returnjson }, JsonRequestBehavior.AllowGet);
                }
                if (List_all.Count == 0)
                {
                    List<Employee> data = new List<Employee>();
                    data = db.Employee.Include(e => e.ServiceBookDates).Include(e => e.EmpName).ToList();
                    if (deserialize != null && deserialize.CheckAll == "check")
                    {
                        foreach (var item in data.Distinct().OrderBy(a => a.EmpCode))
                        {
                            returndata.Add(new Utility.returndataclass
                            {
                                code = item.Id.ToString(),
                                value = item.FullDetails,
                            });
                        }
                    }
                    else
                    {
                        foreach (var item in data.Distinct().OrderBy(a => a.EmpCode))
                        {
                            if (item.ServiceBookDates.ServiceLastDate == null || (item.ServiceBookDates.ServiceLastDate != null &&
                                item.ServiceBookDates.ServiceLastDate.Value > dtChk))
                            {
                                returndata.Add(new Utility.returndataclass
                                {
                                    code = item.Id.ToString(),
                                    value = item.FullDetails,
                                });
                            }
                        }
                    }

                    var returnjson = new
                    {
                        data = returndata,
                        tablename = "employee-table1"
                    };
                    return Json(new List<Object> { returnjson }, JsonRequestBehavior.AllowGet);
                }

                else
                {
                    return Json("No Record Found", JsonRequestBehavior.AllowGet);
                }
            }
        }
        //    return null;
        //}

        public ActionResult LoadEmp(P2BGrid_Parameters gp, FormCollection form)
        {


            try
            {
                DateTime? dt = null;
                string monthyr = "";
                DataBaseContext db = new DataBaseContext();
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                double totalSum = 0;
                var jsonData = (Object)null;
                IEnumerable<empdetails> EmpList = null;
                List<empdetails> model = new List<empdetails>();
                empdetails view = null;
                string v = "";

                var emptraining = db.EmployeeTraining.Include(q => q.Employee.EmpName).Include(q => q.TrainingDetails.Select(a => a.LvWFDetails)).Where(s => s.TrainingDetails.Count > 0).ToList();

                List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();

                foreach (var leo in emptraining)
                {
                    foreach (var messi in leo.TrainingDetails)
                    {
                        if (messi.LvWFDetails.Count > 0)
                        {

                            if (messi.LvWFDetails.LastOrDefault().WFStatus == 1)
                            {
                                model.Add(new empdetails
                                {
                                    Id = messi.Id,
                                    EmpCode = leo.Employee.EmpCode,
                                    EmpName = leo.Employee.EmpName.FullNameFML,
                                    //Training = messi.BatchName
                                    //for
                                });
                            }
                        }
                    }
                }

                EmpList = model;

                IEnumerable<empdetails> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = EmpList;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.EmpCode.ToString().Contains(gp.searchString))
                            || (e.Id.ToString().Contains(gp.searchString))
                            || (e.EmpName.ToString().Contains(gp.searchString))
                            || (e.Training.ToString().Contains(gp.searchString))
                            ).Select(a => new { a.Id, a.EmpCode, a.EmpName, a.Training }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.EmpCode, a.EmpName, a.Training }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = EmpList;
                    Func<empdetails, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? int.Parse(c.EmpCode) : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id.ToString() :
                                         gp.sidx == "Emp Code" ? c.EmpName.ToString() :
                                         gp.sidx == "Emp Name" ? c.EmpName.ToString() :
                                         gp.sidx == "Training" ? c.Training.ToString() : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.EmpCode, Convert.ToString(a.EmpName), a.Training }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.EmpCode, Convert.ToString(a.EmpName), a.Training }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.EmpCode, Convert.ToString(a.EmpName), a.Training }).ToList();
                    }
                    totalRecords = EmpList.Count();
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
                    total = totalPages,
                };
                return Json(JsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<int> one_ids(string form)
        {
            string itsec_id = form;
            List<int> ids = itsec_id.Split(',').Select(e => int.Parse(e)).ToList();
            return (ids);
        }

        public ActionResult UpdateStatus(FormCollection tr, FormCollection form, String forwarddata)
        {
            List<string> Msg = new List<string>();

            string Emp = forwarddata == "0" ? "" : forwarddata;
            string selected = form["selected"] == null ? null : form["selected"];
            string ReasonSanction = form["ReasonSanction"] == null ? "false" : form["ReasonSanction"];
            string oforwarddata = form["forwarddata"] == null ? null : form["forwarddata"];
            string ReasonApproval = form["ReasonApproval"] == null ? "false" : form["ReasonApproval"];
            var isClose = form["isClose"] == null ? null : form["isClose"];

            List<int> ids = null;
            if (Emp != "null")
            {
                ids = one_ids(Emp);
            }
            else
            {
                Msg.Add(" Kindly Select Employee ");
                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
            }
            if (selected != null || ReasonApproval != null || ReasonSanction != null || oforwarddata != null)
            {

            }
            string authority = Convert.ToString(Session["auho"]);

            // bool SanctionRejected = false;
            try
            {
                using (DataBaseContext db = new DataBaseContext())
                {

                    foreach (var tra in ids)
                    {
                        LvWFDetails oLvWFDetails = new LvWFDetails();

                        oLvWFDetails = new LvWFDetails
                        {
                            WFStatus = 3,
                            Comments = ReasonSanction,
                            DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName }
                        };

                        var datatr = db.TrainingDetails.Where(q => q.Id == tra).SingleOrDefault();
                        datatr.LvWFDetails = new List<LvWFDetails> { oLvWFDetails };
                        datatr.WFStatus = db.LookupValue.Where(e => e.LookupVal == "3").SingleOrDefault();
                        datatr.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                        datatr.TrClosed = Convert.ToBoolean(isClose) == true ? true : false;

                        //  db.TrainingDetails.Attach(datatr);
                        db.Entry(datatr).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                    }

                    return Json(new { status = true, responseText = "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
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

            return null;
        }


        public class LoanAdvReqChildDataClass //childgrid
        {
            public int Id { get; set; }
            public string FullDetails { get; set; }
            public string ProgramList { get; set; }

        }

        public ActionResult Get_TrainingSession(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var db_data = db.EmployeeTraining
                        .Include(e => e.TrainingDetails)
                        .Include(e => e.TrainingDetails.Select(t => t.TrainigDetailSessionInfo))
                        .Include(e => e.TrainingDetails.Select(t => t.TrainigDetailSessionInfo.Select(y => y.TrainingSession.SessionType)))
                        .Include(e => e.TrainingDetails.Select(t => t.TrainigDetailSessionInfo.Select(y => y.TrainingSession.TrainingProgramCalendar)))
                        .Include(e => e.TrainingDetails.Select(t => t.TrainigDetailSessionInfo.Select(y => y.TrainingSession.TrainingProgramCalendar.ProgramList)))
                        .Include(e => e.TrainingDetails.Select(t => t.TrainingSchedule))
                        .Include(e => e.TrainingDetails.Select(t => t.TrainingSchedule.TrainingSession))
                        .Include(e => e.TrainingDetails.Select(t => t.TrainingSchedule.TrainingSession.Select(x => x.TrainingProgramCalendar)))
                        .Include(e => e.TrainingDetails.Select(t => t.TrainingSchedule.TrainingSession.Select(x => x.TrainingProgramCalendar.ProgramList)))
                        .Include(e => e.TrainingDetails.Select(t => t.TrainingSchedule.TrainingSession.Select(x => x.SessionType)))
                        .Where(e => e.Id == data)
                        .SingleOrDefault();



                    if (db_data.TrainingDetails != null)
                    {
                        List<LoanAdvReqChildDataClass> returndata = new List<LoanAdvReqChildDataClass>();

                        foreach (var item1 in db_data.TrainingDetails.OrderByDescending(e => e.Id))
                        {
                            foreach (var item in item1.TrainigDetailSessionInfo)
                            {
                                returndata.Add(new LoanAdvReqChildDataClass
                                {
                                    Id = item.Id,
                                    FullDetails = item.TrainingSession != null && item.TrainingSession.FullDetails != null ? item.TrainingSession.FullDetails : "",
                                    ProgramList = item.TrainingSession != null && item.TrainingSession.TrainingProgramCalendar != null && item.TrainingSession.TrainingProgramCalendar.ProgramList != null ? item.TrainingSession.TrainingProgramCalendar.ProgramList.FullDetails : "",
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

        public class TrainingDetails1
        {
            public Array TrainingSession_Id { get; set; }
            public Array TrainingSession_val { get; set; }

            public string TrainingSchedule_Id { get; set; }
            public string TrainingSchedule_val { get; set; }

            public int TrainingSource_val { get; set; }

        }

        [HttpPost]
        public ActionResult Edit(int data)
        {

            using (DataBaseContext db = new DataBaseContext())
            {
                //var Trainingassignment = db.EmployeeTraining
                //    .Where(e => e.Id == data)
                //    .Include(e => e.TrainingDetails)

                //                    .Select(q => q.TrainingDetails).ToList();

                //var r = (from ca in Trainingassignment
                //         select new
                //         {

                //             Id = ca.,
                //             Code = ca.Code,

                //             Details = ca.Details,
                //             Action = ca.DBTrack.Action

                //         }).Distinct();

                //List<Budget1> fext = new List<Budget1>();

                List<TrainingDetails1> cat1 = new List<TrainingDetails1>();

                var add_data1 = db.EmployeeTraining
                    .Where(e => e.Id == data)
                    .Include(e => e.TrainingDetails)
                    .Include(e => e.TrainingDetails.Select(t => t.TrainingSchedule))
                    .Include(e => e.TrainingDetails.Select(t => t.TrainingSchedule.TrainingEmployeeSource))
                    .Include(e => e.TrainingDetails.Select(t => t.TrainingSchedule.TrainingEmployeeSource.Select(x => x.EmployeeTrainingSource)))

                    .ToList();

                foreach (var ca in add_data1)
                {

                    var a = ca.TrainingDetails.ToList();
                    foreach (var ca1 in a)
                    {
                        var b = ca1.TrainingSchedule.TrainingEmployeeSource.ToList();
                        foreach (var ca2 in b)
                        {

                            cat1.Add(new TrainingDetails1
                            {

                                TrainingSource_val = ca2.EmployeeTrainingSource.Id == null ? 0 : ca2.EmployeeTrainingSource.Id,
                            });
                        }
                    }

                }

                List<TrainingDetails1> cat = new List<TrainingDetails1>();

                var add_data = db.EmployeeTraining
                    .Where(e => e.Id == data)
                    .Include(e => e.TrainingDetails)
                    .Include(e => e.TrainingDetails.Select(t => t.TrainingSchedule))
                    .Include(e => e.TrainingDetails.Select(t => t.TrainingSchedule.TrainingEmployeeSource))
                    .Include(e => e.TrainingDetails.Select(t => t.TrainingSchedule.TrainingEmployeeSource.Select(x => x.EmployeeTrainingSource)))
                    .Include(e => e.TrainingDetails.Select(t => t.TrainingSchedule.TrainingSession))
                    .Include(e => e.TrainingDetails.Select(t => t.TrainingSchedule.TrainingSession.Select(x => x.SessionType)))
                    .ToList();

                foreach (var ca in add_data)
                {

                    var a = ca.TrainingDetails.ToList();
                    foreach (var ca1 in a)
                    {
                        //var b = ca1.TrainingSchedule.TrainingEmployeeSource.ToList();
                        //foreach (var ca2 in ca1)
                        //{

                        cat.Add(new TrainingDetails1
                        {
                            TrainingSession_Id = ca1.TrainingSchedule.TrainingSession.Select(e => e.Id.ToString()).ToArray(),
                            TrainingSession_val = ca1.TrainingSchedule.TrainingSession.Select(e => e.FullDetails).ToArray(),

                            TrainingSchedule_Id = ca1.TrainingSchedule.Id.ToString(),
                            TrainingSchedule_val = ca1.TrainingSchedule.FullDetails,


                            //TrainingSource_val = ca1.TrainingSchedule.EmployeeTrainingSource.Id == null ? 0 : ca2.EmployeeTrainingSource.Id,
                        });
                        //}
                    }

                }

                //var W = db.dt_tra
                //     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                //     (e => new
                //     {
                //         DT_Id = e.Id,
                //         Code = e.Code,
                //         Details = e.Details,

                //     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();
                var LKup = db.EmployeeTraining.Find(data);
                TempData["RowVersion"] = LKup.RowVersion;
                var Auth = LKup.DBTrack.IsModified;

                return this.Json(new Object[] { cat1, cat, "", Auth, JsonRequestBehavior.AllowGet });
            }
        }

    }
}