///
/// Created by Sarika
///

using System;
using System.Collections.Generic;
using System.Web;
using System.Linq;
using System.Web.Mvc;
using System.Net;
using System.Data.Entity;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Transactions;
using P2b.Global;
using Training;
using System.Text;
using P2BUltimate.Controllers;
using P2BUltimate.App_Start;
using System.Data.Entity.Core.Objects;
using System.Reflection;
using P2BUltimate.Models;
using System.Web.Script.Serialization;
using System.Threading.Tasks;
using P2BUltimate.Security;

namespace P2BUltimate.Controllers.Training.MainController
{


    public class TrainingScheduleController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();


        public ActionResult Index()
        {
            return View("~/Views/Training/MainViews/TrainingSchedule/Index.cshtml");
        }
        public ActionResult Partialtrschedule()
        {
            return View("~/Views/Shared/Training/_TrainingSchedule.cshtml");
        }
        public ActionResult Partial()
        {
            return View("~/Views/Shared/Training/_TrainingSession.cshtml");
        }

        public ActionResult partialEmpSource()
        {
            return View("~/Views/Shared/Training/_Venue.cshtml");
        }
        public ActionResult partialE()
        {
            return View("~/Views/Shared/Training/_TrainingEmployeeSource.cshtml");
        }


        public ActionResult getCityList(int? id)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                IEnumerable<City> city = db.City.ToList();
                var result = new SelectList(city, "Id", "Name", "");
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }


        /* ---------------------------- GetLookup Sessions -------------------------*/

        public ActionResult GetLookupDetailsSession(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.TrainingSession.Include(e => e.SessionType)
                    .Include(e => e.TrainingProgramCalendar)
                    .Include(e => e.TrainingProgramCalendar.ProgramList)
                    .ToList();
                IEnumerable<TrainingSession> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.TrainingSession
                        .Include(e => e.SessionType)
                        .Include(e => e.TrainingProgramCalendar)
                        .Include(e => e.TrainingProgramCalendar.ProgramList).ToList();
                }
                else
                {
                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails + ", Program : " + ca.TrainingProgramCalendar.ProgramList.FullDetails }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.Faculty.FacultySpecialization, c.SessionDate, c.StartTime, c.EndTime }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        /* ---------------------------- GetLookup Venue -------------------------*/

        public ActionResult GetLookupDetailsVenue(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.Venue.Include(e => e.VenuType).ToList();
                IEnumerable<Venue> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.Venue.Include(e => e.VenuType).ToList();

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

        /* ---------------------------- GetLookup Expensive -------------------------*/

        public ActionResult GetLookupDetailsExpenses(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.TrainingExpenses.ToList();
                IEnumerable<TrainingExpenses> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.TrainingExpenses.ToList();

                }
                else
                {

                    var r = (from ca in fall select new { srno = ca.Id, lookupvalue = "Stay Fees :" + ca.StayFees + " , " + "Contact Person :" + ca.TrainingFees + "Travel Fees:" + ca.TravelFees + "/n" + "Food Fees :" + ca.FoodFees }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.StayFees, c.TrainingFees, c.TravelFees, c.FoodFees }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        /* ---------------------------- GetLookup Training material -------------------------*/
        public ActionResult GetLookupDetailsTrainingMaterial(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.TrainingMaterial.ToList();
                IEnumerable<TrainingMaterial> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.TrainingMaterial.ToList();

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


        /* ---------------------------- GetLookup Training material -------------------------*/
        public ActionResult GetLookupDetailsEmpAssignSource(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.TrainingEmployeeSource.ToList();
                IEnumerable<TrainingEmployeeSource> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.TrainingEmployeeSource.ToList();

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


        public ActionResult Get_Employelist(string geo_id)
        {
            using (DataBaseContext db = new DataBaseContext())
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
                List<Employee> data = new List<Employee>();
                var compid = Convert.ToInt32(Session["CompId"].ToString());
                db.Database.CommandTimeout = 3600;
                var empdata = db.CompanyTraining.Where(e => e.Company.Id == compid)
                .Include(e => e.EmployeeTraining)
                    .Include(e => e.EmployeeTraining.Select(a => a.Employee))
                    .Include(e => e.EmployeeTraining.Select(a => a.Employee.GeoStruct))
                    .Include(e => e.EmployeeTraining.Select(a => a.Employee.FuncStruct))
                    .Include(e => e.EmployeeTraining.Select(a => a.Employee.PayStruct))
                    .Include(e => e.EmployeeTraining.Select(a => a.Employee.EmpName))
                    .Include(e => e.EmployeeTraining.Select(a => a.Employee.ServiceBookDates)).AsNoTracking().AsParallel()
                    .SingleOrDefault();
                // .Where(e => e.Company.Id == compid).AsNoTracking().OrderBy(e => e.Id).SingleOrDefault();

                // empdata.EmployeePayroll.OrderBy(a => a.Employee.EmpCode).ToList();
                if (deserialize.GeoStruct != null)
                {
                    var one_id = Utility.StringIdsToListIds(deserialize.GeoStruct);
                    foreach (var ca in one_id)
                    {
                        var id = Convert.ToInt32(ca);
                        var emp = empdata.EmployeeTraining.Where(e => e.Employee.GeoStruct != null && e.Employee.GeoStruct.Id == id).Select(e => e.Employee).ToList();
                        if (emp != null && emp.Count != 0)
                        {
                            foreach (var item in emp)
                            {
                                data.Add(item);

                            }
                        }
                    }
                }

                List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                if (data != null && data.Count != 0)
                {
                    foreach (var item in data.Distinct().OrderBy(a => a.EmpCode))
                    {
                        if (item.ServiceBookDates.ServiceLastDate == null || (item.ServiceBookDates.ServiceLastDate != null &&
                            item.ServiceBookDates.ServiceLastDate.Value >= dtChk))
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
                        tablename = "employee_table"
                    };
                    return Json(new List<Object> { returnjson }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("No Record Found", JsonRequestBehavior.AllowGet);
                }
            }
        }
        /*---------------------------------------------------------- Create ---------------------------------------------- */
        [HttpPost]
        public ActionResult Create(TrainingSchedule c, FormCollection form, string TrCalId)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    string session = form["SessionListSch"];
                    string venue = form["VenueList"];
                    string expens = form["ExpensesList"];
                    string place = form["CityList"];
                    string state = form["StateList"];
                    string TrainingMaterial = form["TrainingMaterialList"];
                    //string EmpAssignSource = form["TrainingEmployeeSourceList"];
                    string FinancialYearList = form["Financial_id"] == "0" ? "" : form["Financial_id"];
                    string EmpAssignSource = form["TrainingEmployeeSourceList"] == "0" ? "" : form["TrainingEmployeeSourceList"];
                    string IsBatchClose = form["IsBatchClose"] == "0" ? "" : form["IsBatchClose"];


                    if (session == null)
                    {
                        Msg.Add("Please select Training Session..!! ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    if (venue == null)
                    {
                        Msg.Add("Please select Venue..!! ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    if (EmpAssignSource == null)
                    {
                        Msg.Add("Please select Training Request Parameter..!! ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    if (place != null && place != "")
                    {
                        var val = db.City.Find(int.Parse(place));
                        c.City = val;
                    }

                    if (state != null && state != "")
                    {
                        var val = db.State.Find(int.Parse(state));
                        c.State = val;
                    }



                    if (session != null && session != "")
                    {
                        List<int> IDs = session.Split(',').Select(e => int.Parse(e)).ToList();
                        foreach (var k in IDs)
                        {
                            var value = db.TrainingSession.Find(k);
                            c.TrainingSession = new List<TrainingSession>();
                            c.TrainingSession.Add(value);
                        }
                    }


                    if (TrainingMaterial != null && TrainingMaterial != "")
                    {
                        List<int> IDs = TrainingMaterial.Split(',').Select(e => int.Parse(e)).ToList();
                        foreach (var k in IDs)
                        {
                            var value = db.TrainingMaterial.Find(k);
                            c.TrainingMaterial = new List<TrainingMaterial>();
                            c.TrainingMaterial.Add(value);
                        }
                    }

                    if (EmpAssignSource != null && EmpAssignSource != "")
                    {

                        List<int> IDs = EmpAssignSource.Split(',').Select(e => int.Parse(e)).ToList();
                        c.TrainingEmployeeSource = new List<TrainingEmployeeSource>();
                        foreach (var k in IDs)
                        {
                            var value = db.TrainingEmployeeSource.Find(k);
                            c.TrainingEmployeeSource.Add(value);
                        }
                    }


                    if (venue != null && venue != "")
                    {

                        int ContId = Convert.ToInt32(venue);
                        var val = db.Venue.Include(e => e.VenuType).Include(e => e.ContactDetails)
                                            .Where(e => e.Id == ContId).SingleOrDefault();
                        c.Venue = val;
                    }


                    if (expens != null && expens != "")
                    {
                        int ContId = Convert.ToInt32(expens);
                        var val = db.TrainingExpenses
                                            .Where(e => e.Id == ContId).SingleOrDefault();
                        c.Expenses = val;
                    }

                    //if (FinancialYearList != null && FinancialYearList != "")
                    //{
                    //    var value = db.Calendar.Find(int.Parse(FinancialYearList));
                    //    var start = value.FromDate;
                    //    var end = value.ToDate;
                    //    //var v1 = db.YearlyProgramAssignment.Where(a => a.TrainingYear.FromDate == start && a.TrainingYear.ToDate == end).SingleOrDefault();
                    //    //c.TrainingCalendar = v1;

                    //}


                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            //if (db.TrainingSchedule.Any(o => o.Session == c.Session))
                            //{
                            //    return Json(new Object[] { "", "", "Code Already Exists.", JsonRequestBehavior.AllowGet });
                            //}

                            c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                            TrainingSchedule trnschedule = new TrainingSchedule()
                            {

                                TrainingSession = c.TrainingSession,
                                Venue = c.Venue,
                                Expenses = c.Expenses,
                                City = c.City,
                                State = c.State,
                                TrainingEmployeeSource = c.TrainingEmployeeSource,
                                TrainingMaterial = c.TrainingMaterial,
                                FullDetails = c.FullDetails,
                                DBTrack = c.DBTrack,
                                Id = c.Id,
                                TrainingBatchCode = c.TrainingBatchCode,
                                TrainingBatchName = c.TrainingBatchName,
                                IsBatchClose = c.IsBatchClose
                            };
                            try
                            {
                                db.TrainingSchedule.Add(trnschedule);
                                var rtn_Obj = DBTrackFile.DBTrackSave("Training/Training", null, db.ChangeTracker, c.DBTrack);
                                DT_TrainingSchedule DT_Corp = (DT_TrainingSchedule)rtn_Obj;
                                DT_Corp.City_Id = c.City == null ? 0 : c.City.Id;
                                
                                DT_Corp.Expenses_Id = c.Expenses == null ? 0 : c.Expenses.Id;
                                DT_Corp.Venue_Id = c.Venue == null ? 0 : c.Venue.Id;
                                // DT_Corp.TrainingCalendar_Id = c.TrainingCalendar == null ? 0 : c.TrainingCalendar.Id;
                                db.Create(DT_Corp);
                                db.SaveChanges();

                              //  int TrCalrId =int.Parse(FinancialYearList);
                               List<TrainingSchedule> TrSChList = new List<TrainingSchedule>();
                                YearlyTrainingCalendar YrTrCal = db.YearlyTrainingCalendar.Include(e => e.TrainingSchedule)
                                    .Include(e => e.TrainingCalendar).Include(e => e.TrainingCalendar.Name)
                                    .Where(e => e.TrainingCalendar.Name.LookupVal.ToString().ToUpper() == "TRAININGCALENDAR" && e.TrainingCalendar.Default == true).FirstOrDefault();
                                if(YrTrCal != null)
                                {
                                    TrSChList.AddRange(YrTrCal.TrainingSchedule);
                                }
                                TrSChList.Add(trnschedule);
                                YrTrCal.TrainingSchedule = TrSChList;
                                db.YearlyTrainingCalendar.Attach(YrTrCal);
                                db.Entry(YrTrCal).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(YrTrCal).State = System.Data.Entity.EntityState.Detached;
                               


                                ts.Complete();

                                Msg.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { Id = trnschedule.Id, Val = trnschedule.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);


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

        public class sessionDetails
        {
            public Array sessionDtl_Id { get; set; }
            public Array SessionDtl_val { get; set; }
            public Array TrMaterial_Id { get; set; }
            public Array TrMaterial_val { get; set; }
            public Array EmpSourceAssign_Id { get; set; }
            public Array EmpSourceAssign_val { get; set; }
        }

        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.TrainingSchedule
                  .Include(e => e.TrainingSession)
                  .Include(e => e.Venue)
                  .Include(e => e.City)
                  .Include(e =>e.State)
                  .Include(e => e.Expenses)
                  .Where(e => e.Id == data).Select
                  (e => new
                  {
                      City_Id = e.City.Id == null ? 0 : e.City.Id,
                      State_Id = e.State.Id == null ? 0 : e.State.Id,
                      Venue_Id = e.Venue.Id == null ? 0 : e.Venue.Id,
                      Expenses_Id = e.Expenses.Id == null ? 0 : e.Expenses.Id,
                      Action = e.DBTrack.Action,
                      TrainingBatchCode = e.TrainingBatchCode,
                      TrainingBatchName = e.TrainingBatchName,
                      IsBatchClose = e.IsBatchClose
                  }).ToList();

                List<sessionDetails> contno = new List<sessionDetails>();

                var add_data = db.TrainingSchedule
                    .Include(e => e.TrainingSession)
                    .Include(e => e.TrainingSession.Select(q => q.SessionType))
                     .Include(e => e.TrainingMaterial)
                      .Include(e => e.TrainingEmployeeSource)
                       .Include(e => e.TrainingEmployeeSource.Select(q => q.EmployeeTrainingSource))
                  .Include(e => e.Venue)
                  .Include(e => e.City)
                  .Include(e => e.State)
                  .Include(e => e.Expenses)
                    //.Include(e => e.TrainingCalendar)
                  .Where(e => e.Id == data)
                  .Select(e => new
                  {
                      e.Id,
                      Venue_FullName = e.Venue.FullDetails == null ? "" : e.Venue.FullDetails,
                      Venue_Id = e.Venue.Id == null ? "" : e.Venue.Id.ToString(),
                      City_FullName = e.City.Name == null ? "" : e.City.Name,
                      City_Id = e.City.Id == null ? "" : e.City.Id.ToString(),
                      Expenses_FullName = e.Expenses.FullDetails == null ? "" : e.Expenses.FullDetails,
                      Expenses_Id = e.Expenses.Id == null ? "" : e.Expenses.Id.ToString(),
                      e.TrainingSession,
                      EmpSsession = e.TrainingSession.Select(t => t.SessionType),
                      e.TrainingEmployeeSource,
                      EmpSourceAssign_lookup = e.TrainingEmployeeSource.Select(q => q.EmployeeTrainingSource),
                      e.TrainingMaterial
                  })
                  .ToList();

                foreach (var ca in add_data)
                {
                    contno.Add(
                new sessionDetails
                {
                    sessionDtl_Id = ca.TrainingSession.Select(e => e.Id.ToString()).ToArray(),
                    SessionDtl_val = ca.TrainingSession.Select(e => e.FullDetails).ToArray(),
                    TrMaterial_Id = ca.TrainingMaterial.Select(e => e.Id.ToString()).ToArray(),
                    TrMaterial_val = ca.TrainingMaterial.Select(e => e.FullDetails).ToArray(),
                    EmpSourceAssign_Id = ca.TrainingEmployeeSource.Select(e => e.Id.ToString()).ToArray(),
                    EmpSourceAssign_val = ca.TrainingEmployeeSource.Select(e => e.FullDetails.ToString()).ToArray()
                });
                }

                //var progass = db.TrainingSchedule.Include(e => e.TrainingCalendar).Include(e => e.TrainingCalendar.ProgramList).Where(e => e.Id == data).SingleOrDefault();
                //string strt = progass.TrainingCalendar.StartDate.Value.ToShortDateString();
                //string end = progass.TrainingCalendar.EndDate.Value.ToShortDateString();

                //var dat = new
                //{
                //    YearlyProgramAssignment_fullD = "Start Date: " + strt + ", End Date: " + end + ", Program List: " + progass.TrainingCalendar.ProgramList.FullDetails + ".",
                //};
                //   .Select(e => new
                //{
                //    YearlyProgramAssignment_fullD = "Start Date: " +strt + ",End Date: " + e.TrainingCalendar.EndDate.Value.ToShortDateString() + "Program List: " + e.TrainingCalendar.ProgramList.FullDetails + ".",
                //})


                var W = db.DT_TrainingSchedule
                   .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                   (e => new
                   {
                       DT_Id = e.Id,
                       Session_Val = e.Session_Id == 0 ? "" : db.TrainingSession.Where(x => x.Id == e.Session_Id).Select(x => x.FullDetails).FirstOrDefault(),
                       City_Val = e.City_Id == 0 ? "" : db.City.Where(x => x.Id == e.City_Id).Select(x => x.Name).FirstOrDefault(),
                      
                       Expenses_Val = e.Expenses_Id == 0 ? "" : db.TrainingExpenses.Where(x => x.Id == e.Expenses_Id).Select(x => x.FullDetails).FirstOrDefault(),
                       Venue_Val = e.Venue_Id == 0 ? "" : db.Venue.Where(x => x.Id == e.Venue_Id).Select(x => x.FullDetails).FirstOrDefault()
                   }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var Corp = db.TrainingSchedule.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, add_data, "", W, Auth, "", contno, JsonRequestBehavior.AllowGet });
            }
        }



        [HttpPost]
        public async Task<ActionResult> EditSave(TrainingSchedule c, int data, FormCollection form) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {

                    var db_data = db.TrainingSchedule
                                    .Include(e => e.Expenses)
                                    .Include(e => e.TrainingSession)
                                    .Include(e => e.TrainingEmployeeSource)
                                    .Where(e => e.Id == data).SingleOrDefault();

                    List<TrainingSession> lookupval = new List<TrainingSession>();
                    string Values = form["SessionListSch"];

                    if (Values == null)
                    {
                        Msg.Add("Please select Training Session..!! ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }



                    if (Values != null)
                    {
                        var ids = Utility.StringIdsToListIds(Values);
                        foreach (var ca in ids)
                        {
                            var Lookup_val = db.TrainingSession.Find(ca);
                            lookupval.Add(Lookup_val);
                            db_data.TrainingSession = lookupval;
                        }
                    }
                    else
                    {
                        db_data.TrainingSession = null;
                    }

                    List<TrainingEmployeeSource> ObjTrainingEmployeeSource = new List<TrainingEmployeeSource>();

                    string TrainingEmployeeSourceList = form["TrainingEmployeeSourceList"] == "0" ? "" : form["TrainingEmployeeSourceList"];

                    if (TrainingEmployeeSourceList == null)
                    {
                        Msg.Add("Please select Training Request Parameter..!! ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    if (TrainingEmployeeSourceList != null && TrainingEmployeeSourceList != "")
                    {
                        var ids = Utility.StringIdsToListIds(TrainingEmployeeSourceList);
                        foreach (var ca in ids)
                        {
                            var ITSeclist = db.TrainingEmployeeSource.Find(ca);
                            ObjTrainingEmployeeSource.Add(ITSeclist);
                            db_data.TrainingEmployeeSource = ObjTrainingEmployeeSource;
                        }
                    }
                    else
                    {
                        db_data.TrainingEmployeeSource = null;
                    }

                    List<TrainingMaterial> ObjTrainingMaterial = new List<TrainingMaterial>();

                    string TrainingMaterialList = form["TrainingMaterialList"] == "0" ? "" : form["TrainingMaterialList"];
                    if (TrainingMaterialList != null && TrainingMaterialList != "")
                    {
                        var ids = Utility.StringIdsToListIds(TrainingMaterialList);
                        foreach (var ca in ids)
                        {
                            var ITSeclist = db.TrainingMaterial.Find(ca);
                            ObjTrainingMaterial.Add(ITSeclist);
                            db_data.TrainingMaterial = ObjTrainingMaterial;
                        }
                    }
                    else
                    {
                        db_data.TrainingMaterial = null;
                    }


                    string Addrs = form["VenueList"] == "0" ? "" : form["VenueList"];
                    string exp = form["ExpensesList"] == "0" ? "" : form["ExpensesList"];
                    string Cit = form["CityList"] == "0" ? "" : form["CityList"];
                    string sta = form["StateList"] == "0" ? "" : form["StateList"];


                    //  bool Auth = form["Autho_Action"] == "" ? false : true;
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;

                    if (Cit != null)
                    {
                        if (Cit != "")
                        {
                            var val = db.City.Find(int.Parse(Cit));
                            c.City = val;
                        }
                    }
                    if (sta != null)
                    {
                        if (sta != "")
                        {
                            var val = db.State.Find(int.Parse(sta));
                            c.State = val;
                        }
                    }


                    if (exp != null)
                    {
                        if (exp != "")
                        {
                            int ContId = Convert.ToInt32(exp);
                            var val = db.TrainingExpenses
                                                .Where(e => e.Id == ContId).SingleOrDefault();
                            c.Expenses = val;
                        }
                    }

                    if (Addrs != null)
                    {
                        if (Addrs != "")
                        {
                            int AddId = Convert.ToInt32(Addrs);
                            var val = db.Venue.Where(e => e.Id == AddId).SingleOrDefault();
                            c.Venue = val;
                        }
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





                                    db.TrainingSchedule.Attach(db_data);
                                    db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    TempData["RowVersion"] = db_data.RowVersion;
                                    db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;

                                    var Curr_Lookup = db.TrainingSchedule.Find(data);
                                    TempData["CurrRowVersion"] = Curr_Lookup.RowVersion;
                                    db.Entry(Curr_Lookup).State = System.Data.Entity.EntityState.Detached;


                                    TrainingSchedule blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.TrainingSchedule.Where(e => e.Id == data)
                          .Include(e => e.TrainingSession)
                        .Include(e => e.Venue)
                        .Include(e => e.City)
                        .Include(e =>e.State)
                        .Include(e => e.Expenses)
                        .Include(e => e.TrainingMaterial)
                                            .Include(e => e.TrainingEmployeeSource)
                        .SingleOrDefault();
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

                                    //int a = EditS(Cit, exp, Addrs, data, c, c.DBTrack);

                                    if (Cit != null)
                                    {
                                        if (Cit != "")
                                        {
                                            var val = db.City.Find(int.Parse(Cit));
                                            c.City = val;

                                            var type = db.TrainingSchedule.Include(e => e.City).Where(e => e.Id == data).SingleOrDefault();
                                            IList<TrainingSchedule> typedetails = null;
                                            if (type.City != null)
                                            {
                                                typedetails = db.TrainingSchedule.Where(x => x.City.Id == type.City.Id && x.Id == data).ToList();
                                            }
                                            else
                                            {
                                                typedetails = db.TrainingSchedule.Where(x => x.Id == data).ToList();
                                            }
                                            //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                                            foreach (var s in typedetails)
                                            {
                                                s.City = c.City;
                                                db.TrainingSchedule.Attach(s);
                                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                                //await db.SaveChangesAsync();
                                                db.SaveChanges();
                                                TempData["RowVersion"] = s.RowVersion;
                                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                            }
                                        }
                                        else
                                        {
                                            var BusiTypeDetails = db.TrainingSchedule.Include(e => e.City).Where(x => x.Id == data).ToList();
                                            foreach (var s in BusiTypeDetails)
                                            {
                                                s.Expenses = null;
                                                db.TrainingSchedule.Attach(s);
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
                                        var BusiTypeDetails = db.TrainingSchedule.Include(e => e.City).Where(x => x.Id == data).ToList();
                                        foreach (var s in BusiTypeDetails)
                                        {
                                            s.City = null;
                                            db.TrainingSchedule.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }
                                    //state
                                    if (sta != null)
                                    {
                                        if (sta != "")
                                        {
                                            var val = db.State.Find(int.Parse(sta));
                                            c.State = val;

                                            var type = db.TrainingSchedule.Include(e => e.State).Where(e => e.Id == data).SingleOrDefault();
                                            IList<TrainingSchedule> typedetails = null;
                                            if (type.State != null)
                                            {
                                                typedetails = db.TrainingSchedule.Where(x => x.State.Id == type.State.Id && x.Id == data).ToList();
                                            }
                                            else
                                            {
                                                typedetails = db.TrainingSchedule.Where(x => x.Id == data).ToList();
                                            }
                                            //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                                            foreach (var s in typedetails)
                                            {
                                                s.State = c.State;
                                                db.TrainingSchedule.Attach(s);
                                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                                //await db.SaveChangesAsync();
                                                db.SaveChanges();
                                                TempData["RowVersion"] = s.RowVersion;
                                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                            }
                                        }
                                        else
                                        {
                                            var BusiTypeDetails = db.TrainingSchedule.Include(e => e.State).Where(x => x.Id == data).ToList();
                                            foreach (var s in BusiTypeDetails)
                                            {
                                                s.Expenses = null;
                                                db.TrainingSchedule.Attach(s);
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
                                        var BusiTypeDetails = db.TrainingSchedule.Include(e => e.State).Where(x => x.Id == data).ToList();
                                        foreach (var s in BusiTypeDetails)
                                        {
                                            s.State = null;
                                            db.TrainingSchedule.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }
                                    ///////
                                    if (exp != null)
                                    {
                                        if (exp != "")
                                        {
                                            var val = db.TrainingExpenses.Find(int.Parse(exp));
                                            c.Expenses = val;

                                            var type = db.TrainingSchedule.Include(e => e.Expenses).Where(e => e.Id == data).SingleOrDefault();
                                            IList<TrainingSchedule> typedetails = null;
                                            if (type.Expenses != null)
                                            {
                                                typedetails = db.TrainingSchedule.Where(x => x.Expenses.Id == type.Expenses.Id && x.Id == data).ToList();
                                            }
                                            else
                                            {
                                                typedetails = db.TrainingSchedule.Where(x => x.Id == data).ToList();
                                            }
                                            //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                                            foreach (var s in typedetails)
                                            {
                                                s.Expenses = c.Expenses;
                                                db.TrainingSchedule.Attach(s);
                                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                                //await db.SaveChangesAsync();
                                                db.SaveChanges();
                                                TempData["RowVersion"] = s.RowVersion;
                                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                            }
                                        }
                                        else
                                        {
                                            var BusiTypeDetails = db.TrainingSchedule.Include(e => e.Expenses).Where(x => x.Id == data).ToList();
                                            foreach (var s in BusiTypeDetails)
                                            {
                                                s.Expenses = null;
                                                db.TrainingSchedule.Attach(s);
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
                                        var BusiTypeDetails = db.TrainingSchedule.Include(e => e.Expenses).Where(x => x.Id == data).ToList();
                                        foreach (var s in BusiTypeDetails)
                                        {
                                            s.Expenses = null;
                                            db.TrainingSchedule.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }

                                    if (Addrs != null)
                                    {
                                        if (Addrs != "")
                                        {
                                            var val = db.Venue.Find(int.Parse(Addrs));
                                            c.Venue = val;

                                            var add = db.TrainingSchedule.Include(e => e.Venue).Where(e => e.Id == data).SingleOrDefault();
                                            IList<TrainingSchedule> addressdetails = null;
                                            if (add.Venue != null)
                                            {
                                                addressdetails = db.TrainingSchedule.Where(x => x.Venue.Id == add.Venue.Id && x.Id == data).ToList();
                                            }
                                            else
                                            {
                                                addressdetails = db.TrainingSchedule.Where(x => x.Id == data).ToList();
                                            }
                                            if (addressdetails != null)
                                            {
                                                foreach (var s in addressdetails)
                                                {
                                                    s.Venue = c.Venue;
                                                    db.TrainingSchedule.Attach(s);
                                                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                                    // await db.SaveChangesAsync(false);
                                                    db.SaveChanges();
                                                    TempData["RowVersion"] = s.RowVersion;
                                                    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var addressdetails = db.TrainingSchedule.Include(e => e.Venue).Where(x => x.Id == data).ToList();
                                        foreach (var s in addressdetails)
                                        {
                                            s.Venue = null;
                                            db.TrainingSchedule.Attach(s);
                                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                            //await db.SaveChangesAsync();
                                            db.SaveChanges();
                                            TempData["RowVersion"] = s.RowVersion;
                                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                        }
                                    }


                                    var CurCorp = db.TrainingSchedule.Find(data);
                                    TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                    db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                    if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                    {

                                        TrainingSchedule corp = new TrainingSchedule()
                                        {

                                            Id = data,
                                            DBTrack = c.DBTrack,
                                            TrainingBatchCode = c.TrainingBatchCode,
                                            TrainingBatchName = c.TrainingBatchName,
                                            IsBatchClose = c.IsBatchClose
                                        };


                                        db.TrainingSchedule.Attach(corp);
                                        db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                                        db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];

                                    }


                                    using (var context = new DataBaseContext())
                                    {


                                        //var obj = DBTrackFile.DBTrackSave("Training/Training", originalBlogValues, db.ChangeTracker, c.DBTrack);
                                        //DT_TrainingSchedule DT_Corp = (DT_TrainingSchedule)obj;
                                        //DT_Corp.City_Id = blog.City == null ? 0 : blog.City.Id;
                                        //DT_Corp.Expenses_Id = blog.Expenses == null ? 0 : blog.Expenses.Id;
                                        //DT_Corp.Venue_Id = blog.Venue == null ? 0 : blog.Venue.Id;
                                        //DT_Corp.TrainingCalendar_Id = blog.TrainingCalendar == null ? 0 : blog.TrainingCalendar.Id;
                                        //db.Create(DT_Corp);
                                        db.SaveChanges();
                                    }
                                    await db.SaveChangesAsync();
                                    ts.Complete();



                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = c.Id, Val = c.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (TrainingSchedule)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {

                                    Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);


                                }
                                else
                                {
                                    var databaseValues = (TrainingSchedule)databaseEntry.ToObject();
                                    c.RowVersion = databaseValues.RowVersion;

                                }
                            }

                            Msg.Add("Record modified by another user.So refresh it and try to save again.");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            TrainingSchedule blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            TrainingSchedule Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.TrainingSchedule.Where(e => e.Id == data).SingleOrDefault();
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
                            TrainingSchedule corp = new TrainingSchedule()
                            {
                                FullDetails = c.FullDetails,
                                Id = data,
                                DBTrack = c.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };


                            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Training/Training", "M", blog, corp, "TrainingSchedule", c.DBTrack);
                                // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                Old_Corp = context.TrainingSchedule.Where(e => e.Id == data)
                                    .Include(e => e.TrainingSession)
                        .Include(e => e.Venue)
                        .Include(e => e.City)
                        .Include(e => e.Expenses)
                                    //.Include(e => e.TrainingCalendar)
                        .SingleOrDefault();
                                DT_TrainingSchedule DT_Corp = (DT_TrainingSchedule)obj;
                                DT_Corp.City_Id = DBTrackFile.ValCompare(Old_Corp.City, c.City);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
                                
                                DT_Corp.Venue_Id = DBTrackFile.ValCompare(Old_Corp.Venue, c.Venue); //Old_Corp.BusinessType == c.BusinessType ? 0 : Old_Corp.BusinessType == null && c.BusinessType != null ? c.BusinessType.Id : Old_Corp.BusinessType.Id;
                                DT_Corp.Expenses_Id = DBTrackFile.ValCompare(Old_Corp.Expenses, c.Expenses); //Old_Corp.ContactDetails == c.ContactDetails ? 0 : Old_Corp.ContactDetails == null && c.ContactDetails != null ? c.ContactDetails.Id : Old_Corp.ContactDetails.Id;
                                db.Create(DT_Corp);
                                //db.SaveChanges();
                            }
                            blog.DBTrack = c.DBTrack;
                            db.TrainingSchedule.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();

                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = c.Id, Val = c.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }

                    }
                    return View();
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

                        TrainingSchedule corp = db.TrainingSchedule
                            .Include(e => e.Venue)
                            .Include(e => e.TrainingSession)
                            .Include(e => e.City)
                            //.Include(e => e.TrainingCalendar)
                            .Include(e => e.Expenses).FirstOrDefault(e => e.Id == auth_id);

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

                        db.TrainingSchedule.Attach(corp);
                        db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                        db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                        //db.SaveChanges();
                        var rtn_Obj = DBTrackFile.DBTrackSave("Training/Training", null, db.ChangeTracker, corp.DBTrack);
                        DT_TrainingSchedule DT_Corp = (DT_TrainingSchedule)rtn_Obj;
                        DT_Corp.Venue_Id = corp.Venue == null ? 0 : corp.Venue.Id;
                        DT_Corp.City_Id = corp.City == null ? 0 : corp.City.Id;
                        DT_Corp.Expenses_Id = corp.Expenses == null ? 0 : corp.Expenses.Id;
                        //DT_Corp.TrainingCalendar_Id = corp.TrainingCalendar == null ? 0 : corp.TrainingCalendar.Id;
                        //DT_Corp.Session_Id = corp.Session == null ? 0 : corp.Session;
                        db.Create(DT_Corp);
                        await db.SaveChangesAsync();

                        ts.Complete();
                        return Json(new Object[] { corp.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                    }
                }
                else if (auth_action == "M")
                {

                    TrainingSchedule Old_Corp = db.TrainingSchedule
                        .Include(e => e.Venue)
                            .Include(e => e.TrainingSession)
                            .Include(e => e.City)
                        //.Include(e => e.TrainingCalendar)
                            .Include(e => e.Expenses).Where(e => e.Id == auth_id).SingleOrDefault();



                    DT_TrainingSchedule Curr_Corp = db.DT_TrainingSchedule
                                                .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                                                .OrderByDescending(e => e.Id)
                                                .FirstOrDefault();

                    if (Curr_Corp != null)
                    {
                        TrainingSchedule corp = new TrainingSchedule();

                        string ven = Curr_Corp.Venue_Id == null ? null : Curr_Corp.Venue_Id.ToString();
                        string cit = Curr_Corp.City_Id == null ? null : Curr_Corp.City_Id.ToString();
                        string exp = Curr_Corp.Expenses_Id == null ? null : Curr_Corp.Expenses_Id.ToString();

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

                                    int a = EditS(cit, exp, ven, auth_id, corp, corp.DBTrack);


                                    await db.SaveChangesAsync();

                                    ts.Complete();
                                    return Json(new Object[] { corp.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (TrainingSchedule)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (TrainingSchedule)databaseEntry.ToObject();
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
                        TrainingSchedule corp = db.TrainingSchedule.AsNoTracking().Include(e => e.Venue)
                            .Include(e => e.TrainingSession)
                            .Include(e => e.City)
                            //.Include(e => e.TrainingCalendar)
                            .Include(e => e.Expenses).FirstOrDefault(e => e.Id == auth_id);

                        City add = corp.City;
                        Venue conDet = corp.Venue;
                        TrainingExpenses val = corp.Expenses;
                        //TrainingSession ses = corp.Session;
                        //YearlyTrainingCalendar trcal= corp.TrainingCalendar;

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

                        db.TrainingSchedule.Attach(corp);
                        db.Entry(corp).State = System.Data.Entity.EntityState.Deleted;


                        var rtn_Obj = DBTrackFile.DBTrackSave("Training/Training", null, db.ChangeTracker, corp.DBTrack);
                        DT_TrainingSchedule DT_Corp = (DT_TrainingSchedule)rtn_Obj;
                        DT_Corp.City_Id = corp.City == null ? 0 : corp.City.Id;
                        DT_Corp.Venue_Id = corp.Venue == null ? 0 : corp.Venue.Id;
                        //DT_Corp.TrainingCalendar_Id = corp.TrainingCalendar == null ? 0 : corp.TrainingCalendar.Id;
                        DT_Corp.Expenses_Id = corp.Expenses == null ? 0 : corp.Expenses.Id;
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
        public ActionResult GetLookupDetailsTrainingCalendar(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.YearlyProgramAssignment.ToList();
                IEnumerable<YearlyProgramAssignment> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.YearlyProgramAssignment.ToList().Where(d => d.FullDetails.Contains(data));

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
        public ActionResult GetLookup(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.TrainingSchedule
                    .Include(e => e.City).
                    Include(e => e.Expenses)
                    //.Include(e => e.TrainingCalendar)
                    .Include(e => e.Venue).ToList();


                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.TrainingSchedule.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
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
                IEnumerable<TrainingSchedule> Trnschedul = null;
                if (gp.IsAutho == true)
                {
                    Trnschedul = db.TrainingSchedule.Include(e => e.City).Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {
                    Trnschedul = db.TrainingSchedule.Include(e => e.City).AsNoTracking().ToList();
                }

                IEnumerable<TrainingSchedule> IE;
                if (!string.IsNullOrEmpty(gp.searchField))
                {
                    IE = Trnschedul;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Select(a => new { a.Id }).Where((e => (e.Id.ToString() == gp.searchString))).ToList();
                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.City != null ? Convert.ToString(a.City.Name) : "" }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = Trnschedul;
                    Func<TrainingSchedule, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.City != null ? Convert.ToString(a.City.Name) : "" }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.City != null ? Convert.ToString(a.City.Name) : "" }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.City != null ? Convert.ToString(a.City.Name) : "" }).ToList();
                    }
                    totalRecords = Trnschedul.Count();
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

        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {

                    TrainingSchedule corporates = db.TrainingSchedule.Include(e => e.City)
                                                       .Include(e => e.State)
                                                       .Include(e => e.Expenses)
                                                       .Include(e => e.Venue)
                                                       .Include(e => e.TrainingSession).Where(e => e.Id == data).SingleOrDefault();

                    Venue add = corporates.Venue;
                    TrainingExpenses conDet = corporates.Expenses;
                    City val = corporates.City;

                    //Corporate corporates = db.Corporate.Where(e => e.Id == data).SingleOrDefault();
                    if (corporates.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack, corporates, null, "Corporate");
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = corporates.DBTrack.CreatedBy != null ? corporates.DBTrack.CreatedBy : null,
                                CreatedOn = corporates.DBTrack.CreatedOn != null ? corporates.DBTrack.CreatedOn : null,
                                IsModified = corporates.DBTrack.IsModified == true ? true : false
                            };
                            corporates.DBTrack = dbT;
                            db.Entry(corporates).State = System.Data.Entity.EntityState.Modified;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Training/Training", null, db.ChangeTracker, corporates.DBTrack);
                            DT_TrainingSchedule DT_Corp = (DT_TrainingSchedule)rtn_Obj;
                            DT_Corp.Venue_Id = corporates.Venue == null ? 0 : corporates.Venue.Id;
                            DT_Corp.City_Id = corporates.City == null ? 0 : corporates.City.Id;
                            
                            DT_Corp.Expenses_Id = corporates.Expenses == null ? 0 : corporates.Expenses.Id;
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
                        //var selectedRegions = corporates.Session;

                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //if (selectedRegions != null)
                            //{
                            //    var corpRegion = new HashSet<int>(corporates.Session.Select(e => e.Id));
                            //    if (corpRegion.Count > 0)
                            //    {
                            //        return Json(new Object[] { "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                            //        // return Json(new Object[] { "", "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                            //    }
                            //}

                            try
                            {
                                DBTrack dbT = new DBTrack
                                {
                                    Action = "D",
                                    ModifiedBy = SessionManager.UserName,
                                    ModifiedOn = DateTime.Now,
                                    CreatedBy = corporates.DBTrack.CreatedBy != null ? corporates.DBTrack.CreatedBy : null,
                                    CreatedOn = corporates.DBTrack.CreatedOn != null ? corporates.DBTrack.CreatedOn : null,
                                    IsModified = corporates.DBTrack.IsModified == true ? false : false//,
                                    //AuthorizedBy = SessionManager.UserName,
                                    //AuthorizedOn = DateTime.Now
                                };

                                // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };

                                db.Entry(corporates).State = System.Data.Entity.EntityState.Deleted;
                                var rtn_Obj = DBTrackFile.DBTrackSave("Training/Training", null, db.ChangeTracker, dbT);
                                DT_TrainingSchedule DT_Corp = (DT_TrainingSchedule)rtn_Obj;
                                DT_Corp.Expenses_Id = add == null ? 0 : add.Id;
                                DT_Corp.City_Id = val == null ? 0 : val.Id;
                                DT_Corp.Venue_Id = conDet == null ? 0 : conDet.Id;

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
                                //Log the error (uncomment dex variable name and add a line here to write a log.)
                                //ModelState.AddModelError("", "Unable to delete. Try again, and if the problem persists, see your system administrator.");
                                //return RedirectToAction("Delete");
                                Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);


                            }
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
        }

        public int EditS(string Cit, string Corp, string Addrs, int data, TrainingSchedule c, DBTrack dbT)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (Cit != null)
                {
                    if (Cit != "")
                    {
                        var val = db.City.Find(int.Parse(Cit));
                        c.City = val;

                        var type = db.TrainingSchedule.Include(e => e.City).Where(e => e.Id == data).SingleOrDefault();
                        IList<TrainingSchedule> typedetails = null;
                        if (type.City != null)
                        {
                            typedetails = db.TrainingSchedule.Where(x => x.City.Id == type.City.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            typedetails = db.TrainingSchedule.Where(x => x.Id == data).ToList();
                        }
                        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                        foreach (var s in typedetails)
                        {
                            s.City = c.City;
                            db.TrainingSchedule.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                    else
                    {
                        var BusiTypeDetails = db.TrainingSchedule.Include(e => e.City).Where(x => x.Id == data).ToList();
                        foreach (var s in BusiTypeDetails)
                        {
                            s.Expenses = null;
                            db.TrainingSchedule.Attach(s);
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
                    var BusiTypeDetails = db.TrainingSchedule.Include(e => e.City).Where(x => x.Id == data).ToList();
                    foreach (var s in BusiTypeDetails)
                    {
                        s.City = null;
                        db.TrainingSchedule.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }

                if (Corp != null)
                {
                    if (Corp != "")
                    {
                        var val = db.TrainingExpenses.Find(int.Parse(Corp));
                        c.Expenses = val;

                        var type = db.TrainingSchedule.Include(e => e.Expenses).Where(e => e.Id == data).SingleOrDefault();
                        IList<TrainingSchedule> typedetails = null;
                        if (type.Expenses != null)
                        {
                            typedetails = db.TrainingSchedule.Where(x => x.Expenses.Id == type.Expenses.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            typedetails = db.TrainingSchedule.Where(x => x.Id == data).ToList();
                        }
                        //db.Entry(type).State = System.Data.Entity.EntityState.Detached;
                        foreach (var s in typedetails)
                        {
                            s.Expenses = c.Expenses;
                            db.TrainingSchedule.Attach(s);
                            db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = s.RowVersion;
                            db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                        }
                    }
                    else
                    {
                        var BusiTypeDetails = db.TrainingSchedule.Include(e => e.Expenses).Where(x => x.Id == data).ToList();
                        foreach (var s in BusiTypeDetails)
                        {
                            s.Expenses = null;
                            db.TrainingSchedule.Attach(s);
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
                    var BusiTypeDetails = db.TrainingSchedule.Include(e => e.Expenses).Where(x => x.Id == data).ToList();
                    foreach (var s in BusiTypeDetails)
                    {
                        s.Expenses = null;
                        db.TrainingSchedule.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }

                if (Addrs != null)
                {
                    if (Addrs != "")
                    {
                        var val = db.Venue.Find(int.Parse(Addrs));
                        c.Venue = val;

                        var add = db.TrainingSchedule.Include(e => e.Venue).Where(e => e.Id == data).SingleOrDefault();
                        IList<TrainingSchedule> addressdetails = null;
                        if (add.Venue != null)
                        {
                            addressdetails = db.TrainingSchedule.Where(x => x.Venue.Id == add.Venue.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            addressdetails = db.TrainingSchedule.Where(x => x.Id == data).ToList();
                        }
                        if (addressdetails != null)
                        {
                            foreach (var s in addressdetails)
                            {
                                s.Venue = c.Venue;
                                db.TrainingSchedule.Attach(s);
                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                // await db.SaveChangesAsync(false);
                                db.SaveChanges();
                                TempData["RowVersion"] = s.RowVersion;
                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                            }
                        }
                    }
                }
                else
                {
                    var addressdetails = db.TrainingSchedule.Include(e => e.Venue).Where(x => x.Id == data).ToList();
                    foreach (var s in addressdetails)
                    {
                        s.Venue = null;
                        db.TrainingSchedule.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }


                var CurCorp = db.TrainingSchedule.Find(data);
                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    c.DBTrack = dbT;
                    TrainingSchedule corp = new TrainingSchedule()
                    {

                        Id = data,
                        DBTrack = c.DBTrack,
                        TrainingBatchCode = c.TrainingBatchCode,
                        TrainingBatchName = c.TrainingBatchName,
                        IsBatchClose = c.IsBatchClose
                    };


                    db.TrainingSchedule.Attach(corp);
                    db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                    return 1;
                }
                return 0;
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


        public class returndatagridclass //Parentgrid
        {
            public string Id { get; set; }
            //public string Code { get; set; }
            public string Details { get; set; }
            public string Venue { get; set; }
            public string StartDate { get; set; }
            public string EndDate { get; set; }
        }

        public ActionResult Emp_Grid(ParamModel param, string y)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    List<YearlyProgramAssignment> all = new List<YearlyProgramAssignment>();

                    if (y == null)
                    {
                        all = db.YearlyProgramAssignment
                        .Include(e => e.YearlyTrainingCalendar)
                        .Include(e => e.TrainingYear)
                        .Include(e => e.YearlyTrainingCalendar.TrainigProgramCalendar)
                        .Include(e => e.YearlyTrainingCalendar.TrainigProgramCalendar.Select(x => x.ProgramList))
                        .Include(e => e.YearlyTrainingCalendar.TrainigProgramCalendar.Select(r => r.ProgramList.TrainingType))
                        .Include(e => e.YearlyTrainingCalendar.TrainingSchedule)
                        .Include(e => e.YearlyTrainingCalendar.TrainingSchedule.Select(p => p.Venue))
                        .Include(e => e.YearlyTrainingCalendar.TrainingSchedule.Select(p => p.Venue.VenuType))
                        .Include(e => e.YearlyTrainingCalendar.TrainingSchedule.Select(p => p.TrainingSession))
                        .Include(e => e.YearlyTrainingCalendar.TrainingSchedule.Select(p => p.TrainingSession.Select(r => r.TrainingProgramCalendar)))
                        .AsNoTracking()
                        .ToList();
                    }
                    else
                    {
                        int Training_id = Convert.ToInt32(y);

                        all = db.YearlyProgramAssignment
                        .Include(e => e.YearlyTrainingCalendar)
                        .Include(e => e.TrainingYear)
                        .Include(e => e.YearlyTrainingCalendar.TrainigProgramCalendar)
                        .Include(e => e.YearlyTrainingCalendar.TrainigProgramCalendar.Select(x => x.ProgramList))
                        .Include(e => e.YearlyTrainingCalendar.TrainigProgramCalendar.Select(r => r.ProgramList.TrainingType))
                        .Include(e => e.YearlyTrainingCalendar.TrainingSchedule)
                        .Include(e => e.YearlyTrainingCalendar.TrainingSchedule.Select(p => p.Venue))
                        .Include(e => e.YearlyTrainingCalendar.TrainingSchedule.Select(p => p.Venue.VenuType))
                        .Include(e => e.YearlyTrainingCalendar.TrainingSchedule.Select(p => p.TrainingSession))
                        .Include(e => e.YearlyTrainingCalendar.TrainingSchedule.Select(p => p.TrainingSession.Select(r => r.TrainingProgramCalendar)))
                        .Where(e => e.TrainingYear.Id == Training_id)
                        .AsNoTracking()
                        .ToList();
                    }

                    // for searchs
                    IEnumerable<YearlyProgramAssignment> fall;
                    if (param.sSearch == null)
                    {
                        fall = all;
                    }
                    else
                    {

                        fall = all.Where(e => (e.Id.ToString().Contains(param.sSearch))
                            //|| (e.Code.ToString().ToLower().Contains(param.sSearch.ToLower()))
                                  || (e.FullDetails.ToString().ToUpper().Contains(param.sSearch.ToUpper()))
                            //|| (e.Venue.ToString().ToUpper().Contains(param.sSearch.ToUpper()))


                                  ).ToList();
                    }

                    //for column sorting
                    var sortindex = Convert.ToInt32(Request["iSortCol_0"]);

                    Func<YearlyProgramAssignment, string> orderfunc = (c =>
                                                                Convert.ToUInt32(sortindex) == 0 ? c.Id.ToString() :
                                                                sortindex == 1 ? c.FullDetails.ToString() : "");
                    var sortcolumn = Request["sSortDir_0"];
                    if (sortcolumn == "asc")
                    {
                        fall = fall.OrderBy(orderfunc);
                    }
                    else
                    {
                        fall = fall.OrderBy(orderfunc);
                    }
                    // Paging 
                    var dcompanies = fall
                            .Skip(param.iDisplayStart).Take(param.iDisplayLength).ToList();
                    if (dcompanies.Count == 0)
                    {
                        List<returndatagridclass> result = new List<returndatagridclass>();
                        foreach (var item1 in fall)
                        {
                            //foreach (var item2 in item1.YearlyTrainingCalendar)
                            //{
                            foreach (var item in item1.YearlyTrainingCalendar.TrainingSchedule)
                            {
                                string StartDt = item.TrainingSession.Count() == 0 ? "" : item.TrainingSession.OrderBy(e => e.SessionDate).FirstOrDefault().SessionDate.Value.ToShortDateString();
                                string EndDt = item.TrainingSession.Count() == 0 ? "" : item.TrainingSession.OrderByDescending(e => e.SessionDate).FirstOrDefault().SessionDate.Value.ToShortDateString();
                                result.Add(new returndatagridclass
                                {
                                    Id = item.Id.ToString(),
                                    //Code = item.Code != null ? item.Code : null,
                                    Details = item.FullDetails != null ? item.FullDetails : null,
                                    StartDate = StartDt,
                                    EndDate = EndDt,
                                    Venue = item.Venue.FullDetails != null ? item.Venue.FullDetails : null
                                });
                            }
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

                                     select new[] { null, Convert.ToString(c.Id), c.FullDetails };
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

        public class LoanAdvReqChildDataClass //childgrid
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Details { get; set; }

        }
        public class LoanAdvReqChildDataClass1 //childgrid
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Details { get; set; }
            public string ProgramList { get; set; }

        }


        [HttpPost]
        public ActionResult GetCalendarLKDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //List<string> Ids = SkipIds.ToString();
                var fall = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "Trainingcalendar".ToUpper()).OrderByDescending(e => e.Id).ToList();

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



        public ActionResult CreateTrainingEmployeeSource(TrainingEmployeeSource look, FormCollection form)
        {

            List<string> Msg = new List<string>();

            using (DataBaseContext db = new DataBaseContext())
            {

                string TrainingEmployeeSource_drop = form["TrainingEmployeeSource_drop"] == "0" ? "" : form["TrainingEmployeeSource_drop"];

                if (TrainingEmployeeSource_drop != null)
                {
                    if (TrainingEmployeeSource_drop != "")
                    {
                        var val = db.LookupValue.Find(int.Parse(TrainingEmployeeSource_drop));
                        look.EmployeeTrainingSource = val;
                    }
                }

                if (ModelState.IsValid)
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        look.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };


                        TrainingEmployeeSource trainingemployeesource = new TrainingEmployeeSource()
                        {

                            EmployeeTrainingSource = look.EmployeeTrainingSource,
                            DBTrack = look.DBTrack
                        };
                        try
                        {
                            db.TrainingEmployeeSource.Add(trainingemployeesource);
                            db.SaveChanges();

                            ts.Complete();
                            Msg.Add("  Data Created successfully  ");
                            return Json(new Utility.JsonReturnClass { Id = trainingemployeesource.Id, Val = trainingemployeesource.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }

                        catch (DbUpdateConcurrencyException)
                        {
                            return RedirectToAction("Create", new { concurrencyError = true, id = look.Id });
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

        [HttpPost]
        public ActionResult GetLookupDetailsTrainingEmployeeSource(List<int> SkipIds)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = new List<TrainingEmployeeSource>();
                fall = db.TrainingEmployeeSource.Include(e => e.EmployeeTrainingSource).ToList();
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.TrainingEmployeeSource.Include(e => e.EmployeeTrainingSource).ToList();
                        else
                            fall = fall.Where(e => e.Id != a).ToList();

                    }
                }
                //var list1 = db.TrainingEmployeeSource.Include(e => e.EmployeeTrainingSource).ToList();
                // var list2 = fall.Except(list1);

                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();
                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult Get_Session(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var db_data = db.TrainingSchedule
                        .Include(e => e.TrainingSession)
                         .Include(e => e.TrainingSession.Select(t => t.SessionType))
                          .Include(e => e.TrainingSession.Select(t => t.TrainingProgramCalendar))
                          .Include(e => e.TrainingSession.Select(t => t.TrainingProgramCalendar.ProgramList))
                        .Where(e => e.Id == data)
                        .SingleOrDefault();

                    if (db_data.TrainingSession != null)
                    {
                        List<LoanAdvReqChildDataClass1> returndata = new List<LoanAdvReqChildDataClass1>();

                        foreach (var item in db_data.TrainingSession.OrderBy(e => e.Id))
                        {
                            returndata.Add(new LoanAdvReqChildDataClass1
                            {
                                Id = item.Id,
                                Details = item.FullDetails != null ? item.FullDetails : "",
                                ProgramList = item.TrainingProgramCalendar.ProgramList != null ? item.TrainingProgramCalendar.ProgramList.FullDetails : ""

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


    }
}
