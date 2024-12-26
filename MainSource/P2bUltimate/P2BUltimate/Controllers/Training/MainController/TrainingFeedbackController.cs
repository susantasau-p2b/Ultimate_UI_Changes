
///
/// Created by Sarika
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
using Payroll;
using Appraisal;
using System.Diagnostics;


namespace P2BUltimate.Controllers.Training.MainController
{
    public class TrainingFeedbackController : Controller
    {
        //
        // GET: /TrainingFeedback/
        List<string> Msg = new List<string>();
        public ActionResult Index()
        {
            return View("~/Views/Training/MainViews/TrainingFeedback/Index.cshtml");
        }
        public ActionResult GridPartial()
        {
            return View("~/Views/Shared/Training/_TrainingFeedbackPartial.cshtml");
        }

        //private DataBaseContext db = new DataBaseContext();

        /*----------------------------- Grid View ------------------------------------- */

        public class DeserializeClass
        {
            public int Id { get; set; }
            public string BatchName { get; set; }
            public string FullDetails { get; set; }
            public string ProgramList { get; set; }
            public string FacultyFeedback { get; set; }
            public string FaultyRating { get; set; }
            public string TrainingFeedback { get; set; }
            public string TrainingRating { get; set; }

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
                      .Include(e => e.TrainingDetails.Select(t => t.TrainigDetailSessionInfo.Select(z => z.TrainingSession)))
                      .Include(e => e.TrainingDetails.Select(t => t.TrainigDetailSessionInfo.Select(z => z.TrainingSession.SessionType)))
                      .Include(e => e.TrainingDetails.Select(t => t.TrainigDetailSessionInfo.Select(z => z.TrainingSession.TrainingProgramCalendar.ProgramList)))
                        .Where(e => e.Id == data).SingleOrDefault();
                    if (db_data != null)
                    {
                        List<DeserializeClass> returndata = new List<DeserializeClass>();

                        // var obj = item2.AppAssignment.AppRatingObjective.Select(e => e.ObjectiveWordings.LookupVal.ToString()).SingleOrDefault();
                        foreach (var item in db_data.TrainingDetails)
                        {
                            foreach (var item2 in item.TrainigDetailSessionInfo)
                            {
                                if (item2.TrainingSession != null && item2.TrainingSession.TrainingProgramCalendar != null && item2.TrainingSession.TrainingProgramCalendar.ProgramList != null)
                                {
                                    string ProgrameFulldetails = "Start Date:" + item2.TrainingSession.TrainingProgramCalendar.StartDate.Value.ToShortDateString() + ", End Date:"
                                                       + item2.TrainingSession.TrainingProgramCalendar.EndDate.Value.ToShortDateString() + ", Subject Details:" + item2.TrainingSession.TrainingProgramCalendar.ProgramList.SubjectDetails;
                                    returndata.Add(new DeserializeClass
                                    {
                                        Id = item2.Id,
                                        BatchName = item.TrainingSchedule != null ? item.TrainingSchedule.TrainingBatchName : "",
                                        FullDetails = item2.TrainingSession.FullDetails,
                                        ProgramList = ProgrameFulldetails,
                                        FacultyFeedback = item2.FacultyFeedback,
                                        FaultyRating = item2.FaultyRating,
                                        TrainingFeedback = item2.TrainingFeedback,
                                        TrainingRating = item2.TrainingRating
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


        public class returnDataClass
        {

            public string FacultyFeedback { get; set; }
            public string FaultyRating { get; set; }
            public string TrainingFeedback { get; set; }
            public string TrainingRating { get; set; }
            public string Batchname { get; set; }
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
                        //var EmpLst = item.TrainingDetails.Where(r => r.TrainingSchedule.Id == id).SingleOrDefault();
                        int count = 0;
                        foreach (var item1 in item.TrainingDetails)
                        {

                            if (item1.TrainingSchedule.Id == id)
                            {
                                if (item1.TrainigDetailSessionInfo.Where(t => t.TrainingSession.Id == Sessionid).Count() > 0)
                                {
                                    if (item1.TrainigDetailSessionInfo.Where(t => t.TrainingFeedback == null && t.TrainingRating == null && t.FacultyFeedback == null && t.FaultyRating == null && t.IsPresent == true && t.IsCancelled == false).ToList().Count() > 0)
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

        public ActionResult GetLookupDetailsSessionInfo(string data, int ts)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                var fall1 = db.TrainingSchedule.Include(e => e.TrainingSession)
                    .Include(e => e.TrainingSession.Select(r => r.SessionType))
                    .Where(e => e.Id == ts).FirstOrDefault();
                var fall = fall1.TrainingSession;
                IEnumerable<TrainingSession> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.TrainingSession.ToList();

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
        public ActionResult GridEditData(int data, string batch)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var returnlist = new List<returnDataClass>();
                if (data != null && data != 0)
                {
                    var retrundataList = db.TrainigDetailSessionInfo.Where(e => e.Id == data).ToList();
                    //  var returnl = retrundataList.Select(a => a.EmpAppRating).ToList();
                    foreach (var a in retrundataList)
                    {

                        //var rp = a.Select(b => new { b.RatingPoints, b.Comments }).SingleOrDefault();
                        returnlist.Add(new returnDataClass()
                        {

                            FacultyFeedback = a.FacultyFeedback,
                            FaultyRating = a.FaultyRating,
                            TrainingFeedback = a.TrainingFeedback,
                            TrainingRating = a.TrainingRating,
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
        //        var FacultyFeedback = form["FacultyFeedback1"] == "0" ? "" : form["FacultyFeedback1"];
        //        var FaultyRating = form["FaultyRating1"] == "0" ? "" : form["FaultyRating1"];
        //        var TrainingFeedback = form["TrainingFeedback1"] == null ? "" : form["TrainingFeedback1"];
        //        var TrainingRating = form["TrainingRating1"] == "0" ? "" : form["TrainingRating1"];

        //        if (FacultyFeedback == "" || FaultyRating == "" || TrainingFeedback == "" || TrainingRating == "")
        //        {
        //            return Json(new { status = false, responseText = "Fill all the fields..!" }, JsonRequestBehavior.AllowGet);
        //        }


        //        //    var dat = form["data"] == null ? "" : form["data"];
        //        var blog = db.TrainingDetails.Where(a => a.Id == empidintrnal).SingleOrDefault();


        //        if (blog != null)
        //        {
        //            //var id = Convert.ToInt32(dat);
        //            var db_data = db.TrainingDetails.Where(e => e.Id == empidintrnal).SingleOrDefault();
        //            //db_data.FacultyFeedback = FacultyFeedback;
        //            //db_data.FaultyRating = FaultyRating;
        //            //db_data.TrainingFeedback = TrainingFeedback;
        //            //db_data.TrainingRating = TrainingRating;
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


        [HttpPost]
        public ActionResult GridEditSave(TrainigDetailSessionInfo ITP, FormCollection form, string data) // Edit submit
        {


            List<String> Msg = new List<String>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var FacultyFeedback1 = form["FacultyFeedback1"] == "0" ? "" : form["FacultyFeedback1"];
                    var FaultyRating1 = form["FaultyRating1"] == "0" ? "" : form["FaultyRating1"];
                    var TrainingFeedback1 = form["TrainingFeedback1"] == null ? "" : form["TrainingFeedback1"];
                    var TrainingRating1 = form["TrainingRating1"] == "0" ? "" : form["TrainingRating1"];
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
                    var db_data = db.TrainigDetailSessionInfo.Where(e => e.Id == empidintrnal).SingleOrDefault();



                    db.TrainigDetailSessionInfo.Attach(db_data);
                    db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    TempData["RowVersion"] = db_data.RowVersion;
                    db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;


                    if (ModelState.IsValid)
                    {
                        //DbContextTransaction transaction = db.Database.BeginTransaction();

                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            TrainigDetailSessionInfo blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.TrainigDetailSessionInfo.Where(e => e.Id == empidintrnal)
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


                            var CurOBJ = db.TrainigDetailSessionInfo.Find(empidintrnal);
                            TempData["CurrRowVersion"] = CurOBJ.RowVersion;
                            db.Entry(CurOBJ).State = System.Data.Entity.EntityState.Detached;
                            if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                            {
                                TrainigDetailSessionInfo ESIOBJ = new TrainigDetailSessionInfo()
                                {
                                    Id = empidintrnal,
                                    FacultyFeedback = FacultyFeedback1,
                                    FaultyRating = FaultyRating1,
                                    TrainingFeedback = TrainingFeedback1,
                                    TrainingRating = TrainingRating1,
                                    DBTrack = ITP.DBTrack

                                };
                                db.TrainigDetailSessionInfo.Attach(ESIOBJ);
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
                    var clientValues = (TrainigDetailSessionInfo)entry.Entity;
                    var databaseEntry = entry.GetDatabaseValues();
                    if (databaseEntry == null)
                    {
                        Msg.Add(ex.Message);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //  return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                    }
                    else
                    {
                        var databaseValues = (TrainigDetailSessionInfo)databaseEntry.ToObject();
                        ITP.RowVersion = databaseValues.RowVersion;

                    }
                }
                catch (Exception e)
                {
                    Msg.Add(e.Message);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
                return View();
            }
        }

        public ActionResult P2BGrid(P2BGrid_Parameters gp)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                    int pageSize = gp.rows;
                    int totalPages = 0;
                    int totalRecords = 0;
                    var jsonData = (Object)null;
                    var TrainingDetails = db.TrainingDetails.ToList();
                    IEnumerable<TrainingDetails> IE;
                    if (!string.IsNullOrEmpty(gp.searchField))
                    {
                        IE = TrainingDetails;
                        if (gp.searchOper.Equals("eq"))
                        {
                            jsonData = IE.Select(a => new Object[] { a.Id }).ToList();
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
                        IE = TrainingDetails;
                        Func<TrainingDetails, string> orderfuc = (c => gp.sidx == "ID" ? c.Id.ToString() :
                            // gp.sidx == "BatchName" ? Convert.ToString(c.BatchName) :
                            //gp.sidx == "FacultyFeedback" ? Convert.ToString(c.FacultyFeedback) :
                            //gp.sidx == "TrainingFeedback" ? Convert.ToString(c.TrainingFeedback) :
                            //gp.sidx == "FaultyRating" ? Convert.ToString(c.FaultyRating) :
                                                                   "");

                        if (gp.sord == "asc")
                        {
                            IE = IE.OrderBy(orderfuc);
                            //jsonData = IE.Select(a => new Object[] { a.Id,  a.FacultyFeedback, a.TrainingFeedback, a.FaultyRating }).ToList();
                        }
                        else if (gp.sord == "desc")
                        {
                            IE = IE.OrderByDescending(orderfuc);
                            //jsonData = IE.Select(a => new Object[] { a.Id,  a.FacultyFeedback, a.TrainingFeedback, a.FaultyRating }).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            // jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.FacultyFeedback, a.TrainingFeedback, a.FaultyRating }).ToList();
                        }
                        totalRecords = TrainingDetails.Count();
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

        public class TrainingScheduledtl
        {
            public Array TrainingScheduleDtl_Id { get; set; }
            public Array TrainingScheduleDtl_val { get; set; }
            public string TrainingEvaluationDtl_val { get; set; }
            public string TrainingEvaluationDtl_Id { get; set; }

        }

        [HttpPost]
        public ActionResult Create1(TrainingDetails c, FormCollection form)
        {
            // TrainingSchedule
            using (DataBaseContext db = new DataBaseContext())
            {
                var IsPresent = form["IsPresent"] == "0" ? "" : form["IsPresent"];
                var IsBatchClose = form["IsBatchClose"] == "0" ? "" : form["IsBatchClose"];
                var IsCancelled = form["IsCancelled"] == "0" ? "" : form["IsCancelled"];

                //c.IsPresent = Convert.ToBoolean(IsPresent);
                //c.IsBatchClose = Convert.ToBoolean(IsBatchClose);
                //c.IsCancelled = Convert.ToBoolean(IsCancelled);


                List<TrainingSchedule> lookupTrainingSchedule = new List<TrainingSchedule>();
                //string TraiShe = form["TrainingScheduleList"];
                //if (TraiShe != null)
                //{
                //    var ids = Utility.StringIdsToListIds(TraiShe);
                //    foreach (var ca in ids)
                //    {
                //        var Lookup_val = db.TrainingSchedule.Find(ca);
                //        lookupTrainingSchedule.Add(Lookup_val);
                //        c.TrainingSchedule = lookupTrainingSchedule;
                //    }
                //}
                //else
                //{
                //    c.TrainingSchedule = null;
                //}

                string Trevl = form["TrainingEvaluationList"] == "0" ? "" : form["TrainingEvaluationList"];

                if (Trevl != null)
                {
                    if (Trevl != "")
                    {
                        int TrnevId = Convert.ToInt32(Trevl);
                        var val = db.TrainingEvaluation
                                            .Where(e => e.Id == TrnevId).SingleOrDefault();
                        //c.TrainingEvaluation = val;
                    }
                }


                if (ModelState.IsValid)
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        //if (db.TrainingDetails.Any(o => o.BatchName == c.BatchName))
                        //{
                        //    Msg.Add("  Code Already Exists.  ");
                        //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //}

                        c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                        TrainingDetails lvencash = new TrainingDetails()
                        {
                            //BatchName = c.BatchName == null ? "" : c.BatchName.Trim(),
                            //FaultyRating = c.FaultyRating,
                            //TrainingFeedback = c.TrainingFeedback,
                            //FacultyFeedback = c.FacultyFeedback,
                            TrainingSchedule = c.TrainingSchedule,
                            //TrainingEvaluation = c.TrainingEvaluation,
                            //IsCancelled = c.IsCancelled,
                            //IsPresent = c.IsPresent,
                            // IsBatchClose = c.IsBatchClose,
                            DBTrack = c.DBTrack
                        };
                        try
                        {
                            db.TrainingDetails.Add(lvencash);
                            var rtn_Obj = DBTrackFile.DBTrackSave("Training/Training", null, db.ChangeTracker, c.DBTrack);
                            DT_TrainingDetails DT_Corp = (DT_TrainingDetails)rtn_Obj;

                            db.Create(DT_Corp);
                            db.SaveChanges();
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", "C", corporate, null, "Corporate", null);


                            ts.Complete();
                            Msg.Add("  Data Saved successfully  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            return RedirectToAction("Create", new { concurrencyError = true, id = c.Id });
                        }
                        catch (DataException /* dex */)
                        {
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
                    Msg.Add(errorMsg);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
            }
        }


        //[HttpPost]
        //public ActionResult LaodEmp(string databatch, string geo_id)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        int idd = Convert.ToInt32(databatch);
        //        //var BatchName = db.TrainingDetails.Where(a => a.Id == idd).Select(a => a.BatchName).FirstOrDefault();
        //        var EmpTr = db.EmployeeTraining.Include(e => e.TrainingDetails)
        //                                      .Include(e => e.Employee)
        //                                      .Include(e => e.Employee.EmpName)
        //                                      //.Include(e => e.TrainingDetails.Select(q => q.TrainingEvaluation))
        //                                      .Include(e => e.Employee.GeoStruct)
        //            //.Include(e => e.Employee.GeoStruct.Location)
        //            //.Include(e => e.Employee.GeoStruct.Location.LocationObj)
        //                                      .Include(e => e.Employee.FuncStruct)
        //                                      .Include(e => e.Employee.PayStruct)
        //                                      .ToList();
        //        List<Employee> Emp = new List<Employee>();
        //        foreach (var E in EmpTr)
        //        {
        //            var TrDet = E.TrainingDetails.Where(e => e.TrainingSchedule.TrainingBatchName == databatch).SingleOrDefault();
        //            if (TrDet != null)
        //            {
        //                Emp.Add(E.Employee);
        //            }
        //        }

        //        List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
        //        if (Emp != null && Emp.Count != 0)
        //        {
        //            foreach (var item in Emp)
        //            {
        //                if (geo_id == "")
        //                {
        //                    returndata.Add(new Utility.returndataclass
        //                    {
        //                        code = item.Id.ToString(),
        //                        value = item.FullDetails,
        //                    });
        //                }
        //                else
        //                {
        //                    DateTime? dt = null;
        //                    string monthyr = "";
        //                    DateTime? dtChk = null;
        //                    int lastDayOfMonth;
        //                    var Serialize = new JavaScriptSerializer();
        //                    var deserialize = Serialize.Deserialize<Utility.GridParaStructIdClass>(geo_id);

        //                    if (deserialize.Filter != "" && deserialize.Filter != null)
        //                    {
        //                        dt = Convert.ToDateTime("01/" + deserialize.Filter).AddMonths(-1);
        //                        monthyr = dt.Value.ToString("MM/yyyy");
        //                        dtChk = Convert.ToDateTime(DateTime.DaysInMonth(dt.Value.Year, dt.Value.Month) + "/" + monthyr);
        //                    }
        //                    else
        //                    {
        //                        dt = Convert.ToDateTime("01/" + DateTime.Now.ToString("MM/yyyy")).AddMonths(-1);
        //                        monthyr = dt.Value.ToString("MM/yyyy");
        //                        dtChk = Convert.ToDateTime(DateTime.DaysInMonth(dt.Value.Year, dt.Value.Month) + "/" + monthyr);
        //                    }
        //                    var compid = Convert.ToInt32(Session["CompId"].ToString());
        //                    //var empdata = db.Employee
        //                    //    .Include(a => a.GeoStruct)
        //                    //    .Include(a => a.FuncStruct)
        //                    //    .Include(a => a.PayStruct)
        //                    //    .Include(a => a.EmpName)
        //                    //    .Include(a => a.ServiceBookDates)
        //                    //    .Where(e =>e.Id==item.Id).SingleOrDefault();
        //                    List<Employee> List_all = new List<Employee>();

        //                    if (deserialize.GeoStruct != null)
        //                    {
        //                        var one_id = Utility.StringIdsToListIds(deserialize.GeoStruct);
        //                        foreach (var ca in one_id)
        //                        {
        //                            var id = Convert.ToInt32(ca);
        //                            if (item.GeoStruct != null && item.GeoStruct.Id == id)
        //                            {

        //                                returndata.Add(new Utility.returndataclass
        //                                {
        //                                    code = item.Id.ToString(),
        //                                    value = item.FullDetails,
        //                                });
        //                            }
        //                        }
        //                    }
        //                    if (deserialize.PayStruct != null)
        //                    {
        //                        var one_id = Utility.StringIdsToListIds(deserialize.PayStruct);
        //                        foreach (var ca in one_id)
        //                        {
        //                            var id = Convert.ToInt32(ca);
        //                            if (item.PayStruct != null && item.PayStruct.Id == id)
        //                            {

        //                                returndata.Add(new Utility.returndataclass
        //                                {
        //                                    code = item.Id.ToString(),
        //                                    value = item.FullDetails,
        //                                });
        //                            }
        //                        }

        //                    }
        //                    if (deserialize.FunStruct != null)
        //                    {
        //                        var one_id = Utility.StringIdsToListIds(deserialize.FunStruct);
        //                        foreach (var ca in one_id)
        //                        {
        //                            var id = Convert.ToInt32(ca);
        //                            if (item.FuncStruct != null && item.FuncStruct.Id == id)
        //                            {

        //                                returndata.Add(new Utility.returndataclass
        //                                {
        //                                    code = item.Id.ToString(),
        //                                    value = item.FullDetails,
        //                                });
        //                            }
        //                        }

        //                    }

        //                }
        //            }
        //            var returnjson = new
        //            {
        //                data = returndata,
        //                tablename = "Employee-Table"
        //            };
        //            return Json(new List<Object> { returnjson }, JsonRequestBehavior.AllowGet);
        //            //return Json(returnjson, JsonRequestBehavior.AllowGet);
        //        }
        //        else
        //        {
        //            return Json("No Record Found", JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //}
        public class returndatagridclass //Parentgrid
        {
            public string Id { get; set; }
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
                                            count += 1;

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
                                            count += 1;

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
                            // var OnlyPresentEmployee = item1.TrainigDetailSessionInfo.Where(e => e.IsPresent == true && e.IsCancelled == false).ToList();
                            //foreach (var item2 in OnlyPresentEmployee)
                            //{
                            result.Add(new returndatagridclass
                            {
                                Id = item.Id.ToString(),
                                Code = item.Employee != null ? item.Employee.EmpCode : null,
                                Name = item.Employee != null && item.Employee.EmpName != null ? item.Employee.EmpName.FullNameFML : null,
                                // StartDate = item2.TrainingSession != null && item2.TrainingSession.TrainingProgramCalendar != null ? item2.TrainingSession.TrainingProgramCalendar.StartDate.Value.ToShortDateString() : "",
                                // EndDate = item2.TrainingSession != null && item2.TrainingSession.TrainingProgramCalendar != null ? item2.TrainingSession.TrainingProgramCalendar.EndDate.Value.ToShortDateString() : "",
                                //ProgramList = item2.TrainingSession != null && item2.TrainingSession.TrainingProgramCalendar != null && item2.TrainingSession.TrainingProgramCalendar.ProgramList != null ? item2.TrainingSession.TrainingProgramCalendar.ProgramList.FullDetails : "",
                            });
                            //}
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

        public class EditDataClass
        {
            public string Id { get; set; }
            public string Val { get; set; }
            public string SalId { get; set; }
        }


        public async Task<ActionResult> Create(TrainingDetails c, FormCollection form, List<EditDataClass> stringify_JsonObj) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                var BatchName = form["Batch_Name"] == null ? "" : form["Batch_Name"];
                var Emplist = form["Employee-Table"] == null ? "" : form["Employee-Table"];
                // var batchnm1 = db.TrainingDetails.Where(a => a.Id.ToString() == BatchName).Select(e => e.BatchName).SingleOrDefault();
                string OverallFaultyRating = form["TrainingSchedule.OverallFaultyRating"] == null ? "" : form["TrainingSchedule.OverallFaultyRating"];
                string FacultyFeedback = form["TrainingSchedule.FacultyFeedback"] == null ? "" : form["TrainingSchedule.FacultyFeedback"];
                string OverallTrainingRating = form["TrainingSchedule.OverallTrainingRating"] == null ? "" : form["TrainingSchedule.OverallTrainingRating"];
                string OverallTrainingFeedback = form["TrainingSchedule.OverallTrainingFeedback"] == null ? "" : form["TrainingSchedule.OverallTrainingFeedback"];
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

                            //DbContextTransaction transaction = db.Database.BeginTransaction();

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
                                                    SessionLstInfo.FacultyFeedback = FacultyFeedback;
                                                    SessionLstInfo.FaultyRating = OverallFaultyRating;
                                                    SessionLstInfo.TrainingFeedback = OverallTrainingFeedback;
                                                    SessionLstInfo.TrainingRating = OverallTrainingRating;
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
                        //catch (DbUpdateConcurrencyException ex)
                        //{
                        //    var entry = ex.Entries.Single();
                        //    var clientValues = (TrainingDetails)entry.Entity;
                        //    var databaseEntry = entry.GetDatabaseValues();
                        //    if (databaseEntry == null)
                        //    {
                        //        Msg.Add(" Unable to save changes. The record was deleted by another user.");
                        //        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //    }
                        //    else
                        //    {
                        //        var databaseValues = (TrainingDetails)databaseEntry.ToObject();
                        //        c.RowVersion = databaseValues.RowVersion;
                        //    }
                        //}
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
        public async Task<ActionResult> Delete(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                TrainingDetails dellvencash = db.TrainingDetails.Where(e => e.Id == data).SingleOrDefault();

                //Corporate corporates = db.Corporate.Where(e => e.Id == data).SingleOrDefault();
                if (dellvencash.DBTrack.IsModified == true)
                {
                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack, corporates, null, "Corporate");
                        DBTrack dbT = new DBTrack
                        {
                            Action = "D",
                            CreatedBy = dellvencash.DBTrack.CreatedBy != null ? dellvencash.DBTrack.CreatedBy : null,
                            CreatedOn = dellvencash.DBTrack.CreatedOn != null ? dellvencash.DBTrack.CreatedOn : null,
                            IsModified = dellvencash.DBTrack.IsModified == true ? true : false
                        };
                        dellvencash.DBTrack = dbT;
                        db.Entry(dellvencash).State = System.Data.Entity.EntityState.Modified;
                        var rtn_Obj = DBTrackFile.DBTrackSave("Training/Training", null, db.ChangeTracker, dellvencash.DBTrack);
                        DT_TrainingDetails DT_Corp = (DT_TrainingDetails)rtn_Obj;

                        db.Create(DT_Corp);
                        // db.SaveChanges();
                        //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack);
                        await db.SaveChangesAsync();
                        //using (var context = new DataBaseContext())
                        //{
                        //   // DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack );
                        //}
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
                                CreatedBy = dellvencash.DBTrack.CreatedBy != null ? dellvencash.DBTrack.CreatedBy : null,
                                CreatedOn = dellvencash.DBTrack.CreatedOn != null ? dellvencash.DBTrack.CreatedOn : null,
                                IsModified = dellvencash.DBTrack.IsModified == true ? false : false//,
                                //AuthorizedBy =SessionManager.UserName,
                                //AuthorizedOn = DateTime.Now
                            };

                            // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy =SessionManager.UserName, ModifiedOn = DateTime.Now };

                            db.Entry(dellvencash).State = System.Data.Entity.EntityState.Deleted;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Training/Training", null, db.ChangeTracker, dbT);
                            DT_TrainingDetails DT_Corp = (DT_TrainingDetails)rtn_Obj;

                            db.Create(DT_Corp);

                            await db.SaveChangesAsync();


                            //using (var context = new DataBaseContext())
                            //{
                            //    corporates.Address = add;
                            //    corporates.ContactDetails = conDet;
                            //    corporates.BusinessType = val;
                            //    DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, dbT);
                            //    //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", dbT);
                            //}
                            ts.Complete();
                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                        catch (RetryLimitExceededException /* dex */)
                        {
                            Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
            }
        }



        //public ActionResult GetLookupDetailsTrainingEvaluation(string data)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var fall = db.TrainingEvaluation.ToList();
        //        IEnumerable<TrainingEvaluation> all;
        //        if (!string.IsNullOrEmpty(data))
        //        {
        //            all = db.TrainingEvaluation.ToList().Where(d => d.FullDetails.Contains(data));

        //        }
        //        else
        //        {
        //            var list1 = db.TrainingDetails.ToList().Select(e => e.TrainingEvaluation);
        //            var list2 = fall.Except(list1);

        //            var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

        //            return Json(r, JsonRequestBehavior.AllowGet);
        //        }
        //        var result = (from c in all
        //                      select new { c.Id, c.FullDetails }).Distinct();
        //        return Json(result, JsonRequestBehavior.AllowGet);
        //    }
        //    // return View();
        //}
        public ActionResult GetLookupDetailsTrainingSchedule(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.TrainingSchedule.Include(e => e.Expenses)
                    //.Include(e => e.TrainingCalendar)
                    .Include(e => e.Venue).ToList();
                IEnumerable<TrainingSchedule> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.TrainingSchedule.ToList().Where(d => d.FullDetails.Contains(data));

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

        //public ActionResult Getschedule1(string data)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var v = db.TrainingDetails.Include(a => a.TrainingSchedule).Where(a => a.BatchName == data).ToList();
        //        foreach (var item in v)
        //        {
        //            var v1 = item.TrainingSchedule.Select(a => new { a.Id, a.FullDetails }).FirstOrDefault();
        //            return Json(v1);
        //        }
        //        return View();
        //    }
        //}

        //public ActionResult Getschedule(string data)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var BatchName = db.TrainingDetails.Where(a => a.Id.ToString() == data).Select(a => a.BatchName).Distinct().FirstOrDefault();

        //        {
        //            //  var qurey = db.TrainingDetails.Include(e => e.TrainingSchedule.Select(b => b.City)).Include(e => e.TrainingSchedule.Select(b => b.Expenses)).Include(e => e.TrainingSchedule.Select(b => b.Session)).Where(a => a.BatchName == BatchName)
        //            var qurey = db.TrainingDetails.Include(e => e.TrainingSchedule).Where(a => a.BatchName == BatchName)
        //                .Select(e => new
        //                {
        //                    Id1 = e.TrainingSchedule.Select(a => a.Id),
        //                    //Lvcalendardesc = "From Date :" + e.FromDate.Value.ToShortDateString() + " To Date :" + e.ToDate.Value.ToShortDateString(),
        //                    desc = e.TrainingSchedule.Select(a => a.FullDetails)
        //                }).FirstOrDefault();


        //            return Json(qurey, JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //}



        //[HttpPost]
        //public ActionResult Edit(int data)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        List<TrainingScheduledtl> contno = new List<TrainingScheduledtl>();
        //        var Q = db.TrainingDetails
        //             .Include(e => e.TrainingSchedule)
        //          //.Include(e => e.TrainingEvaluation)

        //            .Where(e => e.Id == data).Select
        //            (e => new
        //            {
        //                //BatchName = e.BatchName,
        //                FaultyRating = e.FaultyRating,
        //                TrainingFeedback = e.TrainingFeedback,
        //                FacultyFeedback = e.FacultyFeedback,
        //                //IsBatchClose = e.IsBatchClose,
        //                IsCancelled = e.IsCancelled,
        //                IsPresent = e.IsPresent,

        //                Action = e.DBTrack.Action
        //            }).ToList();




        //        var add_data = db.TrainingDetails.Include(e => e.TrainingSchedule)
        //            //.Include(e => e.TrainingSchedule.Select(s => s.Expenses))
        //            //.Include(e => e.TrainingEvaluation)

        //          .Where(e => e.Id == data).ToList();
        //        //.Select(e => new { e.Id,e.TrainingEvaluation,e.TrainingSchedule}).ToList(); 


        //        foreach (var ca in add_data)
        //        {
        //            contno.Add(
        //        new TrainingScheduledtl
        //        {
        //            //TrainingScheduleDtl_Id = ca.TrainingSchedule.Select(e => e.Id.ToString()).ToArray(),
        //            //TrainingScheduleDtl_val = ca.TrainingSchedule.Select(e => e.FullDetails.ToString()).ToArray(),
        //            TrainingEvaluationDtl_val = ca.TrainingEvaluation.FullDetails == null ? "" : ca.TrainingEvaluation.FullDetails,
        //            TrainingEvaluationDtl_Id = ca.TrainingEvaluation.Id == null ? "" : ca.TrainingEvaluation.Id.ToString(),

        //        });
        //        }
        //        //TempData["RowVersion"] = db.TrainingDetails.Find(data).RowVersion;

        //        var W = db.DT_TrainingDetails
        //             .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
        //             (e => new
        //             {
        //                 DT_Id = e.Id,
        //                 //BatchName = e.BatchName,
        //                 FaultyRating = e.FaultyRating,
        //                 TrainingFeedback = e.TrainingFeedback,
        //                 FacultyFeedback = e.FacultyFeedback,
        //                 TrainingEvaluation_Val = e.TrainingEvaluation_Id == 0 ? "" : db.TrainingEvaluation.Where(x => x.Id == e.TrainingEvaluation_Id).Select(x => x.FullDetails).FirstOrDefault(),
        //                 TrainingSchedule_Val = e.TrainingSchedule_Id == 0 ? "" : db.TrainingSchedule.Where(x => x.Id == e.TrainingSchedule_Id).Select(x => x.FullDetails).FirstOrDefault()
        //             }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

        //        var Corp = db.TrainingDetails.Find(data);
        //        TempData["RowVersion"] = Corp.RowVersion;
        //        var Auth = Corp.DBTrack.IsModified;
        //        return Json(new Object[] { Q, contno, W, Auth, JsonRequestBehavior.AllowGet });
        //    }
        //}

        //public int EditS(string trnEvln, int data, TrainingDetails c, DBTrack dbT)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        if (trnEvln != null)
        //        {
        //            if (trnEvln != "")
        //            {
        //                var val = db.TrainingEvaluation.Find(int.Parse(trnEvln));
        //                //c.TrainingEvaluation = val;

        //                var type = db.TrainingDetails.Include(e => e.TrainingEvaluation).Where(e => e.Id == data).SingleOrDefault();
        //                IList<TrainingDetails> typedetails = null;
        //                if (type.TrainingEvaluation != null)
        //                {
        //                    typedetails = db.TrainingDetails.Where(x => x.TrainingEvaluation.Id == type.TrainingEvaluation.Id && x.Id == data).ToList();
        //                }
        //                else
        //                {
        //                    typedetails = db.TrainingDetails.Where(x => x.Id == data).ToList();
        //                }
        //                //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
        //                foreach (var s in typedetails)
        //                {
        //                    s.TrainingEvaluation = c.TrainingEvaluation;
        //                    db.TrainingDetails.Attach(s);
        //                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                    //await db.SaveChangesAsync();
        //                    db.SaveChanges();
        //                    TempData["RowVersion"] = s.RowVersion;
        //                    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //                }
        //            }
        //            else
        //            {
        //                var BusiTypeDetails = db.TrainingDetails.Include(e => e.TrainingEvaluation).Where(x => x.Id == data).ToList();
        //                foreach (var s in BusiTypeDetails)
        //                {
        //                    s.TrainingEvaluation = null;
        //                    db.TrainingDetails.Attach(s);
        //                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                    //await db.SaveChangesAsync();
        //                    db.SaveChanges();
        //                    TempData["RowVersion"] = s.RowVersion;
        //                    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //                }
        //            }
        //        }
        //        else
        //        {
        //            var BusiTypeDetails = db.TrainingDetails.Include(e => e.TrainingEvaluation).Where(x => x.Id == data).ToList();
        //            foreach (var s in BusiTypeDetails)
        //            {
        //                s.TrainingEvaluation = null;
        //                db.TrainingDetails.Attach(s);
        //                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                //await db.SaveChangesAsync();
        //                db.SaveChanges();
        //                TempData["RowVersion"] = s.RowVersion;
        //                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //            }
        //        }


        //        var CurCorp = db.TrainingDetails.Find(data);
        //        TempData["CurrRowVersion"] = CurCorp.RowVersion;
        //        db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
        //        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
        //        {
        //            c.DBTrack = dbT;
        //            TrainingDetails corp = new TrainingDetails()
        //            {
        //                //BatchName = c.BatchName == null ? "" : c.BatchName.Trim(),
        //                FaultyRating = c.FaultyRating,
        //                TrainingFeedback = c.TrainingFeedback,
        //                FacultyFeedback = c.FacultyFeedback,
        //                TrainingSchedule = c.TrainingSchedule,
        //                TrainingEvaluation = c.TrainingEvaluation,
        //                IsCancelled = false,
        //                IsPresent = true,
        //                Id = data,
        //                DBTrack = c.DBTrack
        //            };


        //            db.TrainingDetails.Attach(corp);
        //            db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
        //            db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //            //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
        //            return 1;
        //        }
        //        return 0;
        //    }
        //}

        [HttpPost]
        public async Task<ActionResult> EditSave(TrainingDetails c, int data, FormCollection form) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                bool Auth = form["Autho_Allow"] == "true" ? true : false;

                var IsPresent = form["IsPresent"] == "0" ? "" : form["IsPresent"];
                var IsBatchClose = form["IsBatchClose"] == "0" ? "" : form["IsBatchClose"];
                var IsCancelled = form["IsCancelled"] == "0" ? "" : form["IsCancelled"];

                //c.IsPresent = Convert.ToBoolean(IsPresent);
                //c.IsBatchClose = Convert.ToBoolean(IsBatchClose);
                //c.IsCancelled = Convert.ToBoolean(IsCancelled);

                string Values = form["TrainingScheduleList"];
                string trnevlns = form["TrainingEvaluationList"] == "0" ? "" : form["TrainingEvaluationList"];

                var db_Data = db.TrainingDetails.Include(e => e.TrainingSchedule)
                    //.Include(e => e.TrainingEvaluation)
                    .Where(e => e.Id == data).SingleOrDefault();
                db_Data.TrainingSchedule = null;


                List<TrainingSchedule> lookupval = new List<TrainingSchedule>();

                if (Values != null)
                {
                    //var ids = Utility.StringIdsToListIds(Values);
                    //foreach (var ca in ids)
                    //{
                    //    var Lookup_val = db.TrainingSchedule.Find(ca);
                    //    lookupval.Add(Lookup_val);
                    //    db_Data.TrainingSchedule = lookupval;
                    //}
                }
                else
                {
                    db_Data.TrainingSchedule = null;
                }



                //if (trnevlns != null)
                //{
                //    if (trnevlns != "")
                //    {
                //        int trnevlnsId = Convert.ToInt32(trnevlns);
                //        var val = db.TrainingEvaluation
                //                            .Where(e => e.Id == trnevlnsId).SingleOrDefault();
                //        c.TrainingEvaluation = val;
                //    }
                //}


                if (Auth == false)
                {


                    if (ModelState.IsValid)
                    {
                        try
                        {

                            //DbContextTransaction transaction = db.Database.BeginTransaction();

                            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                            {
                                db.TrainingDetails.Attach(db_Data);
                                db.Entry(db_Data).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                TempData["RowVersion"] = db_Data.RowVersion;
                                db.Entry(db_Data).State = System.Data.Entity.EntityState.Detached;

                                var Curr_Lookup = db.TrainingDetails.Find(data);
                                TempData["CurrRowVersion"] = Curr_Lookup.RowVersion;
                                db.Entry(Curr_Lookup).State = System.Data.Entity.EntityState.Detached;


                                TrainingDetails blog = null; // to retrieve old data
                                DbPropertyValues originalBlogValues = null;

                                using (var context = new DataBaseContext())
                                {
                                    blog = context.TrainingDetails
                                        //.Include(e => e.TrainingEvaluation)
                                        .Where(e => e.Id == data).SingleOrDefault();
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


                                TrainingDetails lk = new TrainingDetails
                                {
                                    Id = data,
                                    TrainingSchedule = db_Data.TrainingSchedule,
                                    //TrainingEvaluation = db_Data.TrainingEvaluation,
                                    //BatchName = c.BatchName,
                                    //FacultyFeedback = c.FacultyFeedback,
                                    //FaultyRating = c.FaultyRating,
                                    //TrainingFeedback = c.TrainingFeedback,
                                    //IsCancelled = c.IsCancelled,
                                    //IsPresent = c.IsPresent,
                                    //IsBatchClose = c.IsBatchClose,
                                    DBTrack = c.DBTrack
                                };


                                db.TrainingDetails.Attach(lk);
                                db.Entry(lk).State = System.Data.Entity.EntityState.Modified;

                                // db.Entry(c.DBTrack).State = System.Data.Entity.EntityState.Modified;
                                //db.SaveChanges();
                                db.Entry(lk).OriginalValues["RowVersion"] = TempData["RowVersion"];

                                var obj = DBTrackFile.DBTrackSave("Training/Training", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                DT_TrainingDetails DT_LK = (DT_TrainingDetails)obj;
                                //  DT_LK.Allergy = lk.Allergy.Select(e => e.Id);
                                db.Create(DT_LK);
                                db.SaveChanges();
                                ////DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, look.DBTrack);
                                await db.SaveChangesAsync();
                                //DisplayTrackedEntities(db.ChangeTracker);
                                db.Entry(lk).State = System.Data.Entity.EntityState.Detached;
                                ts.Complete();
                                Msg.Add("  Record Updated");
                                return Json(new Utility.JsonReturnClass { Id = lk.Id, Val = lk.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                        }


                        catch (DbUpdateException e) { throw e; }
                        catch (DataException e) { throw e; }
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
                else
                {

                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {

                        TrainingDetails blog = null; // to retrieve old data
                        DbPropertyValues originalBlogValues = null;
                        TrainingDetails Old_OBJ = null;

                        using (var context = new DataBaseContext())
                        {
                            blog = context.TrainingDetails
                                .Where(e => e.Id == data).SingleOrDefault();
                            TempData["RowVersion"] = blog.RowVersion;
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
                        TrainingDetails trainingDetails = new TrainingDetails()
                        {
                            TrainingSchedule = db_Data.TrainingSchedule,
                            //TrainingEvaluation = db_Data.TrainingEvaluation,
                            //BatchName = blog.BatchName,
                            //FaultyRating = blog.FaultyRating,
                            //TrainingRating = blog.TrainingRating,
                            //FacultyFeedback = blog.FacultyFeedback,
                            Id = data,
                            DBTrack = c.DBTrack,
                            RowVersion = (Byte[])TempData["RowVersion"]
                        };


                        db.Entry(db_Data).State = System.Data.Entity.EntityState.Detached;
                        db.TrainingDetails.Attach(trainingDetails);
                        db.Entry(trainingDetails).State = System.Data.Entity.EntityState.Modified;
                        db.Entry(trainingDetails).OriginalValues["RowVersion"] = TempData["RowVersion"];
                        db.SaveChanges();
                        TempData["RowVersion"] = db_Data.RowVersion;
                        db.Entry(trainingDetails).State = System.Data.Entity.EntityState.Detached;


                        //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                        using (var context = new DataBaseContext())
                        {
                            var obj = DBTrackFile.ModifiedDataHistory("Training/Training", "M", blog, c, "TrainingDetails", c.DBTrack);
                            Old_OBJ = context.TrainingDetails.Where(e => e.Id == data)
                                //.Include(e => e.TrainingEvaluation)
                                .Include(e => e.TrainingSchedule).SingleOrDefault();
                            DT_TrainingDetails DT_OBJ = (DT_TrainingDetails)obj;
                            // DT_OBJ.TrainingEvaluation_Id = DBTrackFile.ValCompare(Old_OBJ.TrainingEvaluation, c.TrainingEvaluation);
                            db.Create(DT_OBJ);
                            //db.SaveChanges();
                        }

                        ts.Complete();
                        Msg.Add("  Record Updated");
                        return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = c.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
                        //Corporate corp = db.Corporate.Find(auth_id);
                        //Corporate corp = db.Corporate.AsNoTracking().FirstOrDefault(e => e.Id == auth_id);

                        TrainingDetails corp = db.TrainingDetails.FirstOrDefault(e => e.Id == auth_id);

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

                        db.TrainingDetails.Attach(corp);
                        db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                        db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                        //db.SaveChanges();
                        var rtn_Obj = DBTrackFile.DBTrackSave("Training/Training", null, db.ChangeTracker, corp.DBTrack);
                        DT_TrainingDetails DT_Corp = (DT_TrainingDetails)rtn_Obj;
                        db.Create(DT_Corp);
                        await db.SaveChangesAsync();

                        ts.Complete();
                        return Json(new Object[] { corp.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                    }
                }
                else if (auth_action == "M")
                {

                    TrainingDetails Old_Corp = db.TrainingDetails
                        //.Include(e => e.TrainingEvaluation)
                        .Where(e => e.Id == auth_id).SingleOrDefault();

                    DT_TrainingDetails Curr_Corp = db.DT_TrainingDetails
                                                .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                                                .OrderByDescending(e => e.Id)
                                                .FirstOrDefault();

                    if (Curr_Corp != null)
                    {
                        TrainingDetails corp = new TrainingDetails();
                        string trevo = Curr_Corp.TrainingEvaluation_Id == null ? null : Curr_Corp.TrainingEvaluation_Id.ToString();
                        //corp.BatchName = Curr_Corp.BatchName == null ? Old_Corp.BatchName : Curr_Corp.BatchName;
                        //corp.FaultyRating = Curr_Corp.FaultyRating == null ? Old_Corp.FaultyRating : Curr_Corp.FaultyRating; ;
                        //corp.TrainingFeedback = Curr_Corp.TrainingFeedback == null ? Old_Corp.TrainingFeedback : Curr_Corp.TrainingFeedback; ;
                        //corp.FacultyFeedback = Curr_Corp.FacultyFeedback == null ? Old_Corp.FacultyFeedback : Curr_Corp.FacultyFeedback; ;
                        //corp.IsCancelled = Curr_Corp.IsCancelled;
                        //corp.IsPresent = Curr_Corp.IsPresent;


                        //      corp.Id = auth_id;

                        if (ModelState.IsValid)
                        {
                            try
                            {

                                //DbContextTransaction transaction = db.Database.BeginTransaction();

                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    // db.Configuration.AutoDetectChangesEnabled = false;
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

                                    //int a = EditS(trevo, auth_id, corp, corp.DBTrack);

                                    await db.SaveChangesAsync();

                                    ts.Complete();
                                    return Json(new Object[] { corp.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (TrainingDetails)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (TrainingDetails)databaseEntry.ToObject();
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
                        //Corporate corp = db.Corporate.Find(auth_id);
                        TrainingDetails corp = db.TrainingDetails.AsNoTracking().FirstOrDefault(e => e.Id == auth_id);

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

                        db.TrainingDetails.Attach(corp);
                        db.Entry(corp).State = System.Data.Entity.EntityState.Deleted;


                        var rtn_Obj = DBTrackFile.DBTrackSave("Training/Training", null, db.ChangeTracker, corp.DBTrack);
                        DT_TrainingDetails DT_Corp = (DT_TrainingDetails)rtn_Obj;
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
                TrainigDetailSessionInfo TrainigDetailSessionInfo = db.TrainigDetailSessionInfo

                   .Where(e => e.Id == empidintrnal).SingleOrDefault();
                try
                {


                    using (TransactionScope ts = new TransactionScope())
                    {
                        db.Entry(TrainigDetailSessionInfo).State = System.Data.Entity.EntityState.Deleted;
                        db.SaveChanges();
                        ts.Complete();
                    }
                    Msg.Add("Data removed successfully..");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
