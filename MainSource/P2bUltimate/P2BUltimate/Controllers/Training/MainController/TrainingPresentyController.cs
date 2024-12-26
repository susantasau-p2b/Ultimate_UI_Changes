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
using Payroll;
using Appraisal;
using System.Diagnostics;

namespace P2BUltimate.Controllers.Training.MainController
{
    public class TrainingPresentyController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();
        //
        // GET: /TrainingPresenty/
        public ActionResult Index()
        {
            return View("~/Views/Training/MainViews/TrainingPresenty/Index.cshtml");
        }

        public ActionResult GridPartial()
        {
            return View("~/Views/Shared/Training/_TrainingPresentyPartial.cshtml");
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
                        // var EmpLst = item.TrainingDetails.Where(r => r.TrainingSchedule.Id == id).SingleOrDefault();
                        int count = 0;
                        foreach (var item1 in item.TrainingDetails)
                        {

                            if (item1.TrainingSchedule.Id == id)
                            {
                                if (item1.TrainigDetailSessionInfo.Where(t => t.TrainingSession.Id == Sessionid).Count() > 0)
                                {
                                    if (item1.TrainigDetailSessionInfo.Where(t => t.IsPresent == false && t.IsCancelled == false && t.CancelReason != "Absent").ToList().Count() > 0)
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


        [HttpPost]
        public ActionResult LaodEmp(string databatch, string geo_id)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                // int idd = Convert.ToInt32(databatch);
                // var BatchName = db.TrainingDetails.Include(e => e.TrainingSchedule).Where(a => a.TrainingSchedule.TrainingBatchName == databatch);
                //.Select(a => a.BatchName).FirstOrDefault();
                var EmpTr = db.EmployeeTraining.Include(e => e.TrainingDetails)
                    .Include(e => e.TrainingDetails.Select(r => r.TrainingSchedule))
                                               .Include(e => e.Employee)
                                               .Include(e => e.Employee.EmpName)
                                               .Include(e => e.Employee.GeoStruct)
                                               .Include(e => e.Employee.FuncStruct)
                                               .Include(e => e.Employee.PayStruct)
                                               .ToList();
                List<Employee> Emp = new List<Employee>();
                foreach (var E in EmpTr)
                {
                    var TrDet = E.TrainingDetails.Where(e => e.TrainingSchedule.TrainingBatchName == databatch).SingleOrDefault();
                    if (TrDet != null)
                    {
                        Emp.Add(E.Employee);
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
                            DateTime? dt = null;
                            string monthyr = "";
                            DateTime? dtChk = null;
                            int lastDayOfMonth;
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
                            var compid = Convert.ToInt32(Session["CompId"].ToString());
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
                    return Json(new Utility.JsonReturnClass { success = false, responseText = "No Record Found OR Data is alredy assigned for all candidates in this batch!" }, JsonRequestBehavior.AllowGet);
                    //return Json("No Record Found", JsonRequestBehavior.AllowGet);
                }
            }
        }





        public class returnDataClass
        {

            public bool IsPresent { get; set; }
            public bool IsCancelled { get; set; }
            public string CancelReason { get; set; }
            public string Batchname { get; set; }
        }


        public ActionResult GridEditData(int data,string batch)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var returnlist = new List<returnDataClass>();
                if (data != null && data != 0)
                {
                    var retrundataList = db.TrainigDetailSessionInfo.Where(e => e.Id == data)
                        .ToList();
                    foreach (var a in retrundataList)
                    {

                        returnlist.Add(new returnDataClass()
                        {
                            IsCancelled = a.IsCancelled,
                            CancelReason = a.CancelReason,
                            IsPresent = a.IsPresent,
                            Batchname = batch
                        });
                    }
                    //return Json(new { returndata = returnlist }, JsonRequestBehavior.AllowGet);
                    return this.Json(new Object[] { returnlist, "", "", "", "", JsonRequestBehavior.AllowGet });
                }
                else
                {
                    return View();
                }
            }
        }

        //public ActionResult GridEditSave(TrainingDetails ITP, FormCollection form, string data)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        int empidintrnal = 0;
        //        int empidMain = 0;
        //        if (data != null)
        //        {
        //            var ids = Utility.StringIdsToListIds(data);

        //            empidintrnal = Convert.ToInt32(ids[0]);
        //            empidMain = Convert.ToInt32(ids[1]);

        //        }
        //        var IsCancelled = form["IsCancelled1"] == "0" ? "" : form["IsCancelled1"];
        //        var IsPresent = form["IsPresent1"] == "0" ? "" : form["IsPresent1"];
        //        var rat = form["CancelReason1"] == null ? "" : form["CancelReason1"];
        //        //ITP.IsCancelled = bool.Parse(IsCancelled);
        //        //ITP.IsPresent = bool.Parse(IsPresent);
        //        //ITP.CancelReason = rat;
        //        if (IsCancelled == "true" && IsPresent == "true")
        //        {
        //            return Json(new { status = false, responseText = "IsCancelled and IsPresent Never Be Same ..!" }, JsonRequestBehavior.AllowGet);
        //        }
        //        if (IsCancelled == "false")
        //        {
        //            // ITP.CancelReason = null;
        //        }
        //        //    var dat = form["data"] == null ? "" : form["data"];
        //        var blog = db.TrainingDetails.Where(a => a.Id == empidintrnal).SingleOrDefault();


        //        if (blog != null)
        //        {
        //            //var id = Convert.ToInt32(dat);
        //            var db_data = db.TrainingDetails.Where(e => e.Id == empidintrnal).SingleOrDefault();
        //            //db_data.IsCancelled = ITP.IsCancelled;
        //            //db_data.IsPresent = ITP.IsPresent;
        //            //db_data.CancelReason = ITP.CancelReason;
        //            try
        //            {
        //                db.TrainingDetails.Attach(db_data);
        //                db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
        //                db.SaveChanges();
        //                db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;
        //                return Json(new { status = true, responseText = "Record Updated..!" }, JsonRequestBehavior.AllowGet);
        //                //  return Json(new { data = db_data, responseText = "Record Updated..!" }, JsonRequestBehavior.AllowGet);

        //            }
        //            catch (Exception e)
        //            {

        //                throw e;
        //            }
        //        }

        //        else
        //        {
        //            return Json(new { responseText = "Data Is Null" }, JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //}

        public ActionResult GridEditSave(TrainigDetailSessionInfo c, FormCollection form, string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (data != null)
                {
                    var id = Convert.ToInt32(data);
                    var IsCancelled = form["IsCancelled1"] == "0" ? "" : form["IsCancelled1"];
                    var IsPresent = form["IsPresent1"] == "0" ? "" : form["IsPresent1"];
                    var CancelReason = form["CancelReason1"] == "" ? null : form["CancelReason1"];
                    var BatchName = form["batchname"] == "" ? null : form["batchname"];

                    if (IsCancelled == "true" && IsPresent == "true")
                    {
                        return Json(new { status = false, responseText = "IsCancelled and IsPresent Never Be Same ..!" }, JsonRequestBehavior.AllowGet);
                    }

                    if (IsCancelled == "false" && IsPresent == "false")
                    {
                        return Json(new { status = false, responseText = "IsCancelled and IsPresent Never Be Same ..!" }, JsonRequestBehavior.AllowGet);
                    }

                    if (IsCancelled == "true" && CancelReason == null)
                    {
                        return Json(new { status = false, responseText = "Please enter CancelReason ..!" }, JsonRequestBehavior.AllowGet);
                    }
                    int TrSchId = Convert.ToInt32(BatchName);
                    TrainingSchedule trSch = db.TrainingSchedule.Find(TrSchId);
                    if (trSch != null)
                    {
                        if (trSch.IsBatchClose == true)
                        {
                            return Json(new { status = false, responseText = "Batch is closed. You can't edit this record now..!" }, JsonRequestBehavior.AllowGet);
                        }
                    }

                    var db_data = db.TrainigDetailSessionInfo.Where(e => e.Id == id).SingleOrDefault();

                    db_data.IsPresent = Convert.ToBoolean(IsPresent);
                    db_data.IsCancelled = Convert.ToBoolean(IsCancelled);
                    db_data.CancelReason = CancelReason;

                    try
                    {
                        db.TrainigDetailSessionInfo.Attach(db_data);
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

        [HttpPost]
        public async Task<ActionResult> Create(TrainingDetails c, FormCollection form) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                var BatchName = form["BatchNameP_id"] == null ? "" : form["BatchNameP_id"];
                var Emplist = form["Employee-Table"] == null ? "" : form["Employee-Table"];
                // var batchnm1 = db.TrainingDetails.Where(a => a.Id.ToString() == BatchName).Select(e => e.BatchName).SingleOrDefault();
                var IsCancelled = form["IsCancelled"];
                var IsPresent = form["IsPresent"];
                var SessionList = form["SessionListSch"] == "0" ? "" : form["SessionListSch"];
                bool Auth = form["Autho_Allow"] == "true" ? true : false;
                var CancelReason = form["TrainingSchedule.CancelReason"] == "" ? null : form["TrainingSchedule.CancelReason"];

                if (IsCancelled == "true" && IsPresent == "true")
                {
                    Msg.Add("IsCancelled and IsPresent Never Be Same...! ");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }

                if (IsCancelled == "true" && CancelReason == null)
                {
                    Msg.Add("Please enter cancel reason...! ");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }

                if (IsCancelled == "false" && IsPresent == "false")
                {
                    Msg.Add("IsCancelled and IsPresent Never Be Same...! ");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                }

                List<int> EmpId = null;
                if (Emplist != null && Emplist != "")
                {

                    EmpId = Utility.StringIdsToListIds(Emplist);

                }
                if (EmpId == null)
                {
                    Msg.Add("Please select employee...! ");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }



                if (Auth == false)
                {
                    if (ModelState.IsValid)
                    {
                        try
                        {

                            //DbContextTransaction transaction = db.Database.BeginTransaction();

                            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                            {

                                int sid = 0;
                                string sfull = null;

                                foreach (var a in EmpId)
                                {
                                    var OEmployeeTraining = db.EmployeeTraining.Include(e => e.Employee).Include(e => e.TrainingDetails)
                                .Include(e => e.TrainingDetails.Select(r => r.TrainingSchedule))
                                .Include(e => e.TrainingDetails.Select(r => r.TrainigDetailSessionInfo))
                                .Include(e => e.TrainingDetails.Select(r => r.TrainigDetailSessionInfo.Select(t => t.TrainingSession)))
                                .Where(e => e.Employee.Id == a).FirstOrDefault();

                                    List<Employee> Emp = new List<Employee>();
                                    if (BatchName != null)
                                    {

                                        var id = Convert.ToInt32(BatchName);
                                        int Sessionid = Convert.ToInt32(SessionList);
                                        bool isprsentstatus = Convert.ToBoolean(IsPresent);
                                        bool IsCancelStatus = Convert.ToBoolean(IsCancelled);

                                        if (isprsentstatus == false && IsCancelStatus == false)
                                        {
                                            CancelReason = "Absent";
                                        }

                                        var EmpLst = OEmployeeTraining.TrainingDetails.Where(r => r.TrainingSchedule.Id == id).ToList();
                                        if (EmpLst.Count() > 0)
                                        {
                                            foreach (var item in EmpLst)
                                            {
                                                var SessionLstInfo = item.TrainigDetailSessionInfo.Where(t => t.TrainingSession.Id == Sessionid).FirstOrDefault();
                                                if (SessionLstInfo != null)
                                                {
                                                    SessionLstInfo.IsCancelled = IsCancelStatus;
                                                    SessionLstInfo.IsPresent = isprsentstatus;
                                                    SessionLstInfo.CancelReason = CancelReason;
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
                        Msg.Add("Record modified by another user.So refresh it and try to save again.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                }
                //else
                //{
                //    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                //    {

                //        TrainingDetails blog = null; // to retrieve old data
                //        DbPropertyValues originalBlogValues = null;
                //        TrainingDetails Old_Corp = null;

                //        using (var context = new DataBaseContext())
                //        {
                //            blog = context.TrainingDetails.Where(e => e.Id == ).SingleOrDefault();
                //            originalBlogValues = context.Entry(blog).OriginalValues;
                //        }
                //        c.DBTrack = new DBTrack
                //        {
                //            CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                //            CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                //            Action = "M",
                //            IsModified = blog.DBTrack.IsModified == true ? true : false,
                //            ModifiedBy = SessionManager.UserName,
                //            ModifiedOn = DateTime.Now
                //        };

                //        if (TempData["RowVersion"] == null)
                //        {
                //            TempData["RowVersion"] = blog.RowVersion;
                //        }

                //        TrainingDetails corp = new TrainingDetails()
                //        {
                //            Id = AID,
                //            IsBatchClose = c.IsBatchClose,
                //            IsCancelled = c.IsCancelled,
                //            IsPresent = c.IsPresent,
                //            BatchName = batchnm,
                //            //  TrainingEvaluation = c.TrainingEvaluation,
                //            CancelReason = c.CancelReason,
                //            // TrainingSchedule = c.TrainingSchedule,
                //            TrainingFeedback = c.TrainingFeedback,
                //            TrainingRating = c.TrainingRating,
                //            FacultyFeedback = c.FacultyFeedback,
                //            FaultyRating = c.FaultyRating,

                //            DBTrack = c.DBTrack,
                //            RowVersion = (Byte[])TempData["RowVersion"]
                //        };


                //        //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                //        using (var context = new DataBaseContext())
                //        {
                //            var obj = DBTrackFile.ModifiedDataHistory("Training/Training", "M", blog, corp, "TrainingDetails", c.DBTrack);
                //            // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                //            Old_Corp = context.TrainingDetails.Where(e => e.Id == AID)
                //                .Include(e => e.TrainingEvaluation).Include(e => e.TrainingSchedule).SingleOrDefault();
                //            DT_TrainingDetails DT_Corp = (DT_TrainingDetails)obj;
                //            DT_Corp.TrainingEvaluation_Id = DBTrackFile.ValCompare(Old_Corp.TrainingEvaluation, c.TrainingEvaluation);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
                //            DT_Corp.TrainingSchedule_Id = DBTrackFile.ValCompare(Old_Corp.TrainingSchedule, c.TrainingSchedule); //Old_Corp.BusinessType == c.BusinessType ? 0 : Old_Corp.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_Corp.BusinessType.Id;
                //            //DT_Corp.ContactDetails_Id = DBTrackFile.ValCompare(Old_Corp.ContactDetails, c.ContactDetails); //Old_Corp.ContactDetails == c.ContactDetails ? 0 : Old_Corp.ContactDetails == null && c.ContactDetails != null ? c.ContactDetails.Id : Old_Corp.ContactDetails.Id;
                //            db.Create(DT_Corp);
                //            //db.SaveChanges();
                //        }
                //        blog.DBTrack = c.DBTrack;
                //        db.TrainingDetails.Attach(blog);
                //        db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                //        db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                //        db.SaveChanges();
                //        ts.Complete();
                //        Msg.Add("  Record Updated");
                //        return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = c.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                //    }
                //}
                return View();
            }

        }

        public ActionResult EditSave(String forwarddata, String PayMonth) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var serialize = new JavaScriptSerializer();

                    var obj = serialize.Deserialize<List<DeserializeClass>>(forwarddata);

                    if (obj.Count < 0)
                    {
                        return Json(new { sucess = true, responseText = "You have to change reason to update record." }, JsonRequestBehavior.AllowGet);
                    }
                    List<int> ids = obj.Select(e => int.Parse(e.Id)).ToList();

                    using (TransactionScope ts = new TransactionScope())
                    {
                        try
                        {
                            foreach (int ca in ids)
                            {
                                TrainigDetailSessionInfo DetSessionInfo = db.TrainigDetailSessionInfo.Find(ca);
                                DetSessionInfo.CancelReason = Convert.ToString(obj.Where(e => e.Id == ca.ToString()).Select(e => e.CancelReason).Single());



                                db.TrainigDetailSessionInfo.Attach(DetSessionInfo);
                                db.Entry(DetSessionInfo).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(DetSessionInfo).State = System.Data.Entity.EntityState.Detached;
                            }
                        }
                        catch (Exception ex)
                        {
                            //List<string> Msg = new List<string>();
                            Msg.Add(ex.Message);
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
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }

                        ts.Complete();
                        return Json(new Object[] { "", "", "Reason Updated Successfully." }, JsonRequestBehavior.AllowGet);

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
        public class P2BGridData
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
        }

        public class LoanAdvReqChildDataClass //childgrid
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string BatchName { get; set; }
            public string FullDetails { get; set; }
            public string IsCancel { get; set; }
            public string CancelReason { get; set; }
            public string IsPresent { get; set; }
            public string ProgramList { get; set; }

        }

        public ActionResult Get_Session(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var compid = Convert.ToInt32(Session["CompId"].ToString());
                    var ab = db.CompanyTraining.Where(e => e.Company.Id == compid)
                     .Include(e => e.EmployeeTraining)
                     .Include(e => e.EmployeeTraining.Select(a => a.TrainingDetails))
                     .Include(e => e.EmployeeTraining.Select(a => a.TrainingDetails.Select(t => t.TrainingSchedule)))
                     .Include(e => e.EmployeeTraining.Select(a => a.TrainingDetails.Select(t => t.TrainigDetailSessionInfo)))
                     .Include(e => e.EmployeeTraining.Select(a => a.TrainingDetails.Select(t => t.TrainigDetailSessionInfo.Select(x => x.TrainingSession.SessionType))))
                     .Include(e => e.EmployeeTraining.Select(a => a.TrainingDetails.Select(t => t.TrainigDetailSessionInfo.Select(z => z.TrainingSession.TrainingProgramCalendar.ProgramList))))
                     .AsNoTracking().AsParallel()
                     .SingleOrDefault();

                    var db_data = ab.EmployeeTraining.Where(e => e.Id == data).SingleOrDefault();
                    //var db_data = db.EmployeeTraining
                    //   .Where(e => e.Id == data)
                    //  .Include(e => e.Employee)
                    //  .Include(e => e.TrainingDetails)
                    //  .Include(e => e.TrainingDetails.Select(r => r.TrainigDetailSessionInfo))
                    //  .Include(e => e.TrainingDetails.Select(r => r.TrainigDetailSessionInfo.Select(x => x.TrainingSession)))
                    //  .Include(e => e.TrainingDetails.Select(r => r.TrainigDetailSessionInfo.Select(x => x.TrainingSession.SessionType)))
                    //  .Include(e => e.TrainingDetails.Select(r => r.TrainigDetailSessionInfo.Select(x => x.TrainingSession.TrainingProgramCalendar.ProgramList)))
                    //  .Include(e => e.TrainingDetails.Select(r => r.TrainingSchedule))
                    //  .Include(e => e.TrainingDetails.Select(r => r.TrainingSchedule.TrainingSession))
                    //  .Include(e => e.TrainingDetails.Select(r => r.TrainingSchedule.TrainingSession.Select(t => t.SessionType)))
                    //  .Include(e => e.TrainingDetails.Select(r => r.TrainingSchedule.TrainingSession.Select(t => t.TrainingProgramCalendar)))
                    //   .Include(e => e.TrainingDetails.Select(r => r.TrainingSchedule.TrainingSession.Select(t => t.TrainingProgramCalendar.ProgramList)))

                    //  .Include(e => e.Employee.EmpName)
                    //  .AsNoTracking().AsParallel()
                    //  .SingleOrDefault();


                    if (db_data != null)
                    {
                        List<LoanAdvReqChildDataClass> returndata = new List<LoanAdvReqChildDataClass>();

                        foreach (var item in db_data.TrainingDetails.OrderByDescending(e => e.Id))
                        {
                            foreach (var item2 in item.TrainigDetailSessionInfo)
                            {
                                //foreach (var item1 in item.TrainingSchedule.TrainingSession)
                                //{
                                if (item2.TrainingSession != null && item2.TrainingSession.TrainingProgramCalendar != null && item2.TrainingSession.TrainingProgramCalendar.ProgramList != null)
                                {
                                    string ProgrameFulldetails = "Start Date:" + item2.TrainingSession.TrainingProgramCalendar.StartDate.Value.ToShortDateString() + ", End Date:"
                                                       + item2.TrainingSession.TrainingProgramCalendar.EndDate.Value.ToShortDateString() + ", Subject Details:" + item2.TrainingSession.TrainingProgramCalendar.ProgramList.SubjectDetails;
                                    returndata.Add(new LoanAdvReqChildDataClass
                                    {
                                        Id = item2.Id,
                                        BatchName = item.TrainingSchedule != null ? item.TrainingSchedule.TrainingBatchName : "",
                                        FullDetails = item2.TrainingSession.FullDetails,
                                        ProgramList = ProgrameFulldetails,
                                        IsCancel = item2.IsPresent == false && item2.IsCancelled == false && item2.CancelReason == null ? "" : item2.IsCancelled == false && item2.CancelReason != "Absent" ? "No" : "Yes",
                                        CancelReason = item2.IsPresent == false && item2.IsCancelled == false ? "" : item2.CancelReason,
                                        IsPresent = item2.IsPresent == false && item2.IsCancelled == false && item2.CancelReason == null ? "" : item2.IsPresent == false && item2.CancelReason == "Absent" ? "Absent" : item2.IsPresent == true ? "Present" : "",

                                    });
                                }
                                //}
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

        public class returndatagridclass //Parentgrid
        {
            public string Id { get; set; }
            public string EmpCode { get; set; }
            public string EmpName { get; set; }

            public string BatchName { get; set; }
        }

        public ActionResult Emp_Grid(ParamModel param, string y)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<EmployeeTraining> all1 = new List<EmployeeTraining>();
                List<EmployeeTraining> FilterEmployee = new List<EmployeeTraining>();
                try
                {
                    var compid = Convert.ToInt32(Session["CompId"].ToString());
                    var ab = db.CompanyTraining.Where(e => e.Company.Id == compid)
                      .Include(e => e.EmployeeTraining)
                      .Include(e => e.EmployeeTraining.Select(a => a.Employee))
                      .Include(e => e.EmployeeTraining.Select(a => a.TrainingDetails))
                      .Include(e => e.EmployeeTraining.Select(a => a.TrainingDetails.Select(r => r.TrainingSchedule)))
                      .Include(e => e.EmployeeTraining.Select(a => a.Employee.EmpName)).AsNoTracking().AsParallel()
                      .SingleOrDefault();
                    if (y != "")
                    {
                        int scheduleids = Convert.ToInt32(y);
                        all1 = ab.EmployeeTraining.ToList();

                        foreach (var item in all1)
                        {
                            if (item.TrainingDetails.Count() > 0)
                            {
                                var filterschedulewiseemp = item.TrainingDetails.Where(t => t.TrainingSchedule.Id == scheduleids).ToList();
                                if (filterschedulewiseemp.Count() > 0)
                                {
                                    FilterEmployee.Add(item);
                                }
                            }
                        }
                    }
                    else
                    {
                        all1 = ab.EmployeeTraining.ToList();

                        foreach (var item in all1)
                        {
                            if (item.TrainingDetails.Count() > 0)
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
                            //if (item1.TrainingSchedule.TrainingBatchName != null)
                            //{
                            result.Add(new returndatagridclass
                            {
                                Id = item.Id.ToString(),
                                EmpCode = item.Employee != null ? item.Employee.EmpCode : null,
                                EmpName = item.Employee != null && item.Employee.EmpName != null ? item.Employee.EmpName.FullNameFML : null,
                                //BatchName = item1.TrainingSchedule != null ? item1.TrainingSchedule.TrainingBatchName : "",
                            });
                            //}
                            //}
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

                                     select new[] { null, Convert.ToString(c.Id), c.Employee.EmpCode };
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


        public class DeserializeClass
        {
            public string Id { get; set; }
            public string BatchName { get; set; }
            public bool IsPresent { get; set; }
            public bool IsCancelled { get; set; }
            public string CancelReason { get; set; }

        }

        public ActionResult Get_AppAssignData(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var db_data = db.EmployeeTraining
                        .Include(e => e.TrainingDetails)
                        //.Include(e => e.EmpAppEvaluation.Select(a => a.EmpAppRatingConclusion))
                        //.Include(e => e.EmpAppEvaluation.Select(a => a.EmpAppRatingConclusion.Select(s => s.EmpAppRating)))
                        //.Include(e => e.EmpAppEvaluation.Select(a => a.EmpAppRatingConclusion.Select(s => s.EmpAppRating.Select(r => r.AppAssignment))))
                        //.Include(e => e.EmpAppEvaluation.Select(a => a.EmpAppRatingConclusion.Select(s => s.EmpAppRating.Select(r => r.AppAssignment.AppCategory))))
                        //.Include(e => e.EmpAppEvaluation.Select(a => a.EmpAppRatingConclusion.Select(s => s.EmpAppRating.Select(r => r.AppAssignment.AppSubCategory))))
                        //.Include(e => e.EmpAppEvaluation.Select(a => a.EmpAppRatingConclusion.Select(s => s.EmpAppRating.Select(r => r.AppAssignment.AppRatingObjective))))
                        //.Include(e => e.EmpAppEvaluation.Select(a => a.EmpAppRatingConclusion.Select(s => s.EmpAppRating.Select(r => r.AppAssignment.AppRatingObjective.Select(q => q.ObjectiveWordings)))))
                        .Where(e => e.Id == data).SingleOrDefault();
                    if (db_data.TrainingDetails != null)
                    {
                        List<DeserializeClass> returndata = new List<DeserializeClass>();

                        foreach (var item in db_data.TrainingDetails)
                        {

                            // var obj = item2.AppAssignment.AppRatingObjective.Select(e => e.ObjectiveWordings.LookupVal.ToString()).SingleOrDefault();


                            returndata.Add(new DeserializeClass
                            {
                                Id = item.Id.ToString(),
                                //BatchName = item.BatchName,
                                //IsPresent = item.IsPresent,
                                //IsCancelled = item.IsCancelled,
                                //CancelReason = item.CancelReason

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
                IEnumerable<TrainingDetails> Venue = null;
                if (gp.IsAutho == true)
                {
                    Venue = db.TrainingDetails.Include(e => e.TrainingSchedule).Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {
                    Venue = db.TrainingDetails.Include(e => e.TrainingSchedule).AsNoTracking().ToList();
                }

                IEnumerable<TrainingDetails> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = Venue;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Select(a => new { a.Id }).Where((e => (e.Id.ToString() == gp.searchString))).ToList();
                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Fees), Convert.ToString(a.Name), Convert.ToString(a.VenuType) != null ? Convert.ToString(a.VenuType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = Venue;
                    Func<TrainingDetails, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    //else
                    //{
                    //    orderfuc = (c => gp.sidx == "Name" ? c.BatchName :
                    //                      "");
                    //}
                    //if (gp.sord == "asc")
                    //{
                    //    IE = IE.OrderBy(orderfuc);
                    //    jsonData = IE.Select(a => new Object[] { a.Id, a.IsCancelled, a.IsPresent }).ToList();
                    //}
                    //else if (gp.sord == "desc")
                    ////////////{
                    //    IE = IE.OrderByDescending(orderfuc);
                    //    jsonData = IE.Select(a => new Object[] { a.Id, a.IsCancelled, a.IsPresent }).ToList();
                    //}
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id }).ToList();
                    }
                    totalRecords = Venue.Count();
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
            public Employee Employee { get; set; }

            public string StartDate { get; set; }
            public string EndDate { get; set; }
            public string Editable { get; set; }
            public string ProgranList { get; set; }
            public bool present { get; set; }
            public bool Cancel { get; set; }
            public string CancelReason { get; set; }
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
                IEnumerable<EditData> SEssionDetInfo = null;

                List<EditData> model = new List<EditData>();
                var view = new EditData();
                string Batch_Name = "";
                if (gp.filter != null)
                    Batch_Name = gp.filter;

                // int id = Convert.ToInt32(Batch_Name);
                var compid = Convert.ToInt32(Session["CompId"].ToString());
                db.Database.CommandTimeout = 3600;

                var empdata = db.CompanyTraining.Where(e => e.Company.Id == compid)
                    .Include(e => e.EmployeeTraining)
                    .Include(e => e.EmployeeTraining.Select(a => a.Employee))
                    .Include(e => e.EmployeeTraining.Select(a => a.TrainingDetails))
                    .Include(e => e.EmployeeTraining.Select(a => a.TrainingDetails.Select(r => r.TrainingSchedule)))
                    .Include(e => e.EmployeeTraining.Select(a => a.TrainingDetails.Select(r => r.TrainigDetailSessionInfo)))
                    .Include(e => e.EmployeeTraining.Select(a => a.TrainingDetails.Select(r => r.TrainigDetailSessionInfo.Select(t => t.TrainingSession))))
                    .Include(e => e.EmployeeTraining.Select(a => a.TrainingDetails.Select(r => r.TrainigDetailSessionInfo.Select(t => t.TrainingSession.TrainingProgramCalendar))))
                    .Include(e => e.EmployeeTraining.Select(a => a.TrainingDetails.Select(r => r.TrainigDetailSessionInfo.Select(t => t.TrainingSession.TrainingProgramCalendar.ProgramList))))
                    .Include(e => e.EmployeeTraining.Select(a => a.TrainingDetails.Select(r => r.TrainigDetailSessionInfo.Select(t => t.TrainingSession.TrainingProgramCalendar.ProgramList.TrainingType))))
                    .Include(e => e.EmployeeTraining.Select(a => a.Employee.EmpName))
                    .Include(e => e.EmployeeTraining.Select(a => a.Employee.ServiceBookDates)).AsNoTracking().AsParallel()
                    .SingleOrDefault();



                foreach (var item in empdata.EmployeeTraining)
                {
                    var EmpLst = item.TrainingDetails.Where(r => r.TrainingSchedule.TrainingBatchName == Batch_Name).ToList();
                    if (EmpLst != null)
                    {
                        foreach (var item1 in EmpLst)
                        {
                            if (item1.TrainigDetailSessionInfo != null && item1.TrainigDetailSessionInfo.Count > 0)
                            {
                                foreach (var item2 in item1.TrainigDetailSessionInfo)
                                {
                                    bool EditAppl = true;
                                    view = new EditData()
                                    {
                                        Id = item2.Id,
                                        Employee = item.Employee != null ? item.Employee : null,
                                        StartDate = item2.TrainingSession != null && item2.TrainingSession.TrainingProgramCalendar != null ? item2.TrainingSession.TrainingProgramCalendar.StartDate.ToString() : "",
                                        EndDate = item2.TrainingSession != null && item2.TrainingSession.TrainingProgramCalendar != null ? item2.TrainingSession.TrainingProgramCalendar.EndDate.ToString() : "",
                                        ProgranList = item2.TrainingSession != null && item2.TrainingSession.TrainingProgramCalendar != null && item2.TrainingSession.TrainingProgramCalendar.ProgramList != null ? item2.TrainingSession.TrainingProgramCalendar.ProgramList.Subject : "",
                                        present = item2.IsPresent,
                                        Cancel = item2.IsCancelled,
                                        CancelReason = item2.CancelReason,
                                        Editable = item2.IsCancelled == true ? "true" : "false"
                                    };

                                    model.Add(view);
                                }
                            }
                        }


                    }

                }

                SEssionDetInfo = model;

                IEnumerable<EditData> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = SEssionDetInfo;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where((e => (e.Id.ToString().Contains(gp.searchString))
                           || (e.Employee.EmpCode.ToString().Contains(gp.searchString))
                           || (e.Employee.EmpName.FullNameFML.ToUpper().Contains(gp.searchString.ToUpper()))
                           || (e.StartDate.ToString().Contains(gp.searchString))
                           || (e.EndDate.ToString().Contains(gp.searchString))
                           || (e.ProgranList.ToString().Contains(gp.searchString))
                           || (e.present.ToString().Contains(gp.searchString))
                           || (e.Cancel.ToString().Contains(gp.searchString))
                           || (e.CancelReason.ToString().Contains(gp.searchString))
                           || (e.Editable.ToUpper().Contains(gp.searchString.ToUpper()))
                       )).Select(a => new Object[] { a.Id, a.Employee.EmpCode != null ? Convert.ToString(a.Employee.EmpCode) : "", a.Employee.EmpName.FullNameFML != null ? Convert.ToString(a.Employee.EmpName.FullNameFML) : "", a.StartDate, a.EndDate, a.ProgranList, a.present, a.Cancel, a.CancelReason, a.Editable }).ToList();
                        //jsonData = IE.Where((e => (e.Contains(gp.searchString)))).Select(a => new Object[] { a.Id, a.Employee.EmpCode != null ? Convert.ToString(a.Employee.EmpCode) : "", a.Employee.EmpName.FullNameFML != null ? Convert.ToString(a.Employee.EmpName.FullNameFML) : "", a.PayMonth != null ? Convert.ToString(a.PayMonth) : "", a.PaybleDays, a.Editable }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Employee.EmpCode != null ? Convert.ToString(a.Employee.EmpCode) : "", a.Employee.EmpName.FullNameFML != null ? Convert.ToString(a.Employee.EmpName.FullNameFML) : "", a.StartDate, a.EndDate, a.ProgranList, a.present, a.Cancel, a.CancelReason, a.Editable }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = SEssionDetInfo;
                    Func<EditData, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "EmpCode" ? c.Employee.EmpCode.ToString() :
                                         gp.sidx == "EmpName" ? c.Employee.EmpName.FullNameFML.ToString() :
                                         gp.sidx == "StartDate" ? c.StartDate.ToString() :
                                         gp.sidx == "EndDate" ? c.EndDate.ToString() :
                                         gp.sidx == "ProgranList" ? c.ProgranList.ToString() :
                                         gp.sidx == "present" ? c.present.ToString() :
                                         gp.sidx == "Cancel" ? c.Cancel.ToString() :
                                         gp.sidx == "CancelReason" ? c.CancelReason.ToString() :
                                         gp.sidx == "Editable" ? c.Editable.ToString() : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.Employee.EmpCode != null ? Convert.ToString(a.Employee.EmpCode) : "", a.Employee.EmpName.FullNameFML != null ? Convert.ToString(a.Employee.EmpName.FullNameFML) : "", a.StartDate, a.EndDate, a.ProgranList, a.present, a.Cancel, a.CancelReason, a.Editable }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.Employee.EmpCode != null ? Convert.ToString(a.Employee.EmpCode) : "", a.Employee.EmpName.FullNameFML != null ? Convert.ToString(a.Employee.EmpName.FullNameFML) : "", a.StartDate, a.EndDate, a.ProgranList, a.present, a.Cancel, a.CancelReason, a.Editable }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Employee.EmpCode != null ? Convert.ToString(a.Employee.EmpCode) : "", a.Employee.EmpName.FullNameFML != null ? Convert.ToString(a.Employee.EmpName.FullNameFML) : "", a.StartDate, a.EndDate, a.ProgranList, a.present, a.Cancel, a.CancelReason, a.Editable }).ToList();
                    }
                    totalRecords = SEssionDetInfo.Count();
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

        public ActionResult ChkProcess(string typeofbtn, string month, string EmpCode)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                bool selected = false;
                var query1 = db.EmployeeTraining.Include(e => e.Employee).Include(e => e.TrainingDetails).Include(e => e.TrainingDetails.Select(q => q.TrainingSchedule)).Where(e => e.Employee.EmpCode == EmpCode).Select(e => e.TrainingDetails.Select(x => x.TrainingSchedule)).SingleOrDefault();
                var query = query1.Where(e => e.TrainingBatchName == month).ToList();

                if (query.Count > 0)
                {
                    selected = true;
                }
                var data = new
                {
                    status = selected,
                };
                return Json(data, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GridDelete(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var LvEP = db.TrainigDetailSessionInfo.Find(data);
                db.TrainigDetailSessionInfo.Remove(LvEP);
                db.SaveChanges();
                List<string> Msgs = new List<string>();
                Msgs.Add("Record Deleted Successfully ");
                return Json(new Utility.JsonReturnClass { success = true, responseText = Msgs }, JsonRequestBehavior.AllowGet);

            }
        }


    }
}