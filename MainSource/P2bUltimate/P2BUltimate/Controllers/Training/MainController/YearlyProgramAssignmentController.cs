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
//using P2BUltimate.App_Start;
using P2BUltimate.Models;
using System.Web.Script.Serialization;
using System.Threading.Tasks;
using P2BUltimate.Security;

namespace P2BUltimate.Controllers.Training.MainController
{
    public class YearlyProgramAssignmentController : Controller
    {
        List<String> Msg = new List<String>();
        //private DataBaseContext db = new DataBaseContext();
        //
        // GET: /TrainingCalendar/

        public ActionResult Index()
        {
            return View("~/Views/Training/MainViews/YearlyProgramAssignment/Index.cshtml");

        }


        public ActionResult partial()
        {
            return View("~/Views/Shared/Training/_YearlyProgramAssignment.cshtml");
        }


        [HttpPost]
        //validation data
        public ActionResult PopulateLookupDropDownList(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var lookupQuery = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "015").SingleOrDefault();

                List<SelectListItem> values = new List<SelectListItem>();

                if (lookupQuery != null)
                {
                    foreach (var item in lookupQuery.LookupValues)
                    {
                        if (item.IsActive == true)
                        {
                            values.Add(new SelectListItem
                            {
                                Text = item.LookupVal,
                                Value = item.Id.ToString(),
                                Selected = (item.Id == Convert.ToInt32(data) ? true : false)
                            });
                        }
                    }
                }
                return Json(values, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult Create(YearlyProgramAssignment NOBJ, FormCollection form, string TrainingYear)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string ProgramList = form["ProgramListlist"] == null ? "" : form["ProgramListlist"];
                string StartDate = form["TrainingYear.FromDate"] == null ? "" : form["TrainingYear.FromDate"];
                string EndDate = form["TrainingYear.ToDate"] == null ? "" : form["TrainingYear.ToDate"];
                int CompId = 0;
                if (!String.IsNullOrEmpty(Session["CompId"].ToString()))
                {
                    CompId = int.Parse(Session["CompId"].ToString());
                }
                if (Convert.ToDateTime(StartDate) > Convert.ToDateTime(EndDate))
                {
                    Msg.Add("Start Date should not be greater than End date.");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }

                if (ProgramList == "")
                {
                    Msg.Add("Please select program list.");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }

                if (ProgramList != "")
                {
                    int prog = Convert.ToInt32(ProgramList);
                    DateTime Startdte = Convert.ToDateTime(StartDate);
                    DateTime Enddte = Convert.ToDateTime(EndDate);

                    var check = db.TrainingProgramCalendar.Where(e => e.ProgramList.Id == prog && e.StartDate == Startdte && e.EndDate == Enddte).FirstOrDefault();
                    if (check != null)
                    {
                        Msg.Add("Program already assigned for this Period .");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                }

                using (TransactionScope ts = new TransactionScope())
                {
                    NOBJ.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                    YearlyTrainingCalendar TrnCalendarList = new YearlyTrainingCalendar();
                    List<TrainingProgramCalendar> ProgramCalendarList = new List<TrainingProgramCalendar>();


                    TrainingProgramCalendar ProgramCalendar = new TrainingProgramCalendar()
                    {
                        StartDate = Convert.ToDateTime(StartDate),
                        EndDate = Convert.ToDateTime(EndDate),
                        ProgramList = db.ProgramList.Find(Convert.ToInt32(ProgramList)),
                        DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                    };

                    ProgramCalendarList.Add(ProgramCalendar);

                    

                    int TrYear = Convert.ToInt32(TrainingYear);
                    YearlyProgramAssignment YrPgmAssign = db.YearlyProgramAssignment.Where(e => e.TrainingYear.Id == TrYear).FirstOrDefault();
                    YearlyTrainingCalendar YrTrCal = db.YearlyTrainingCalendar.Where(e => e.TrainingCalendar.Id == TrYear).FirstOrDefault();
                    if (YrTrCal == null)
                    {
                        YearlyTrainingCalendar YearlyTrainingCalendar = new YearlyTrainingCalendar()
                        {
                            TrainingCalendar = db.Calendar.Where(e => e.Name.LookupVal.ToString().ToUpper() == "TRAININGCALENDAR" && e.Default == true).FirstOrDefault(),
                            TrainigProgramCalendar = ProgramCalendarList,
                            DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                        };

                        db.YearlyTrainingCalendar.Add(YearlyTrainingCalendar);
                        db.SaveChanges();
                        TrnCalendarList = YearlyTrainingCalendar;
                    }
                    else
                    {
                        YrTrCal.TrainigProgramCalendar = ProgramCalendarList;
                        db.YearlyTrainingCalendar.Attach(YrTrCal);
                        db.Entry(YrTrCal).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        TrnCalendarList = YrTrCal;
                        //db.Entry(YrTrCal).State = System.Data.Entity.EntityState.Detached;
                    }
                    if (YrPgmAssign == null)
                    {
                        YearlyProgramAssignment td = new YearlyProgramAssignment()
                        {

                            TrainingYear = db.Calendar.Where(e => e.Name.LookupVal.ToString().ToUpper() == "TRAININGCALENDAR" && e.Default == true).FirstOrDefault(),
                            YearlyTrainingCalendar = TrnCalendarList,
                            DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                        };

                        db.YearlyProgramAssignment.Add(td);
                        db.SaveChanges();

                        CompanyTraining OCompanyTraining = null;
                        OCompanyTraining = db.CompanyTraining.Where(e => e.Company.Id == CompId).SingleOrDefault();
                        List<YearlyProgramAssignment> YearPgmAssign = new List<YearlyProgramAssignment>();
                        YearPgmAssign.Add(td);

                        if (OCompanyTraining == null)
                        {
                            CompanyTraining OTEP = new CompanyTraining()
                            {
                                Company = db.Company.Find(SessionManager.CompanyId),
                                YearlyProgramAssignment = YearPgmAssign,
                                DBTrack = NOBJ.DBTrack
                            };


                            db.CompanyTraining.Add(OTEP);
                            db.SaveChanges();
                        }
                        else
                        {
                            var aa = db.CompanyTraining.Find(OCompanyTraining.Id);

                            if (aa.YearlyProgramAssignment != null)
                                YearPgmAssign.AddRange(aa.YearlyProgramAssignment);
                            aa.YearlyProgramAssignment = YearPgmAssign;
                            db.CompanyTraining.Attach(aa);
                            db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            db.Entry(aa).State = System.Data.Entity.EntityState.Detached;

                        }

                    }
                   

                    ts.Complete();
                    //db.TrainingDetails.Add(td);
                }
                Msg.Add("Data Saved Successfully.");
                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

            }
        }


        /*----------------------------- Grid View ------------------------------------- */

        public class GridClass
        {
            public int Id { get; set; }
            public string StartDate { get; set; }
            public string EndDate { get; set; }
            public string ProgramList { get; set; }


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
                IEnumerable<GridClass> PFMasterList = null;

                var compid = int.Parse(Session["CompId"].ToString());
                var data = db.CompanyTraining.Where(e => e.Company.Id == compid)
                        .Include(e => e.YearlyProgramAssignment)
                        .Include(e => e.YearlyProgramAssignment.Select(r => r.YearlyTrainingCalendar))
                        .Include(e => e.YearlyProgramAssignment.Select(r => r.TrainingYear))
                        .Include(e => e.YearlyProgramAssignment.Select(r => r.YearlyTrainingCalendar.TrainigProgramCalendar))
                        .Include(e => e.YearlyProgramAssignment.Select(r => r.YearlyTrainingCalendar.TrainigProgramCalendar.Select(y => y.ProgramList)))
                        .Include(e => e.YearlyProgramAssignment.Select(r => r.YearlyTrainingCalendar.TrainigProgramCalendar.Select(y => y.ProgramList.TrainingType)))
                       .AsNoTracking().AsParallel()
                        .SingleOrDefault();
                List<GridClass> model = new List<GridClass>();
                if (data.YearlyProgramAssignment != null && data.YearlyProgramAssignment.Count > 0)
                {
                    foreach (var item in data.YearlyProgramAssignment)
                    {
                        Calendar TrCalendar = db.Calendar.Where(e => e.Name.LookupVal.ToString().ToUpper() == "TRAININGCALENDAR" && e.Default == true).FirstOrDefault();
                        if (item.TrainingYear.Id == TrCalendar.Id)
                        {
                            var PrCal = item.YearlyTrainingCalendar.TrainigProgramCalendar.ToList();
                            foreach (var item2 in PrCal)
                            {
                                //foreach (var item2 in item1)
                                //{
                                    model.Add(new GridClass
                                    {
                                        Id = item2.Id,
                                        StartDate = item2.StartDate.Value.ToShortDateString(),//item.TrainingYear.FromDate.Value.ToShortDateString(),
                                        EndDate = item2.EndDate.Value.ToShortDateString(),
                                        ProgramList = item2.ProgramList.FullDetails
                                    });
                                //}

                            }
                        }

                    }
                }
                PFMasterList = model;
                //if (gp.IsAutho == true)
                //{
                //    PFMaster = db.CompanyPayroll.Include(e => e.PFMaster.Select(a => a.PFTrustType)).AsNoTracking().ToList();
                //}
                //else
                //{
                // PFMaster = db.CompanyPayroll.Include(e=>e.PFMaster.Select(a=>a.PFTrustType)).AsNoTracking().ToList();
                //}

                IEnumerable<GridClass> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = PFMasterList;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.Id.ToString().Contains(gp.searchString))
                            || (e.StartDate.Contains(gp.searchString.ToUpper()))
                            || (e.EndDate.Contains(gp.searchString.ToUpper()))
                            || (e.ProgramList.ToUpper().ToString().Contains(gp.searchString.ToUpper().ToString()))
                            ).Select(a => new Object[] { Convert.ToString(a.StartDate), Convert.ToString(a.EndDate), a.ProgramList, a.Id }).ToList();
                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] {  a.StartDate, a.EndDate, a.ProgramList, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = PFMasterList;
                    Func<GridClass, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "StartDate" ? c.StartDate.ToString() :
                                         gp.sidx == "EndDate" ? c.EndDate.ToString() :
                                          "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.StartDate), Convert.ToString(a.EndDate), a.ProgramList, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] {  Convert.ToString(a.StartDate), Convert.ToString(a.EndDate), a.ProgramList, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] {  Convert.ToString(a.StartDate), Convert.ToString(a.EndDate), a.ProgramList, a.Id }).ToList();
                    }
                    totalRecords = PFMasterList.Count();
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



        //[HttpPost]
        //public ActionResult Create(YearlyProgramAssignment c, FormCollection form)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        List<string> Msg = new List<string>();
        //        try
        //        {
        //            string Addrs = form["ProgramListlist"] == "0" ? "" : form["ProgramListlist"];
        //            string prog_assign = form["prog_assign"] == null ? "" : form["prog_assign"];

        //            if (prog_assign != "")
        //            {
        //                int calendarid = Convert.ToInt16(prog_assign);
        //                var chk = db.Calendar.Where(e => e.Id == calendarid).SingleOrDefault();
        //                if (chk.FromDate <= c.TrainingYear.FromDate && chk.ToDate >= c.TrainingYear.ToDate)
        //                {
        //                    //var alrDq = db.YearlyProgramAssignment
        //                    //       .Any(q => ((q.StartDate <= c.StartDate && q.EndDate <= c.EndDate) || (q.StartDate <= c.StartDate && q.EndDate >= c.EndDate)) && (q.EndDate >= c.StartDate));
        //                    //if (alrDq == true)
        //                    //{
        //                    //    Msg.Add("Yearly Program With this Period already exist.  ");
        //                    //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                    //}
        //                }
        //                else
        //                {
        //                    Msg.Add("  Kindly check the StartDate & ToDate inserted for record");
        //                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                }
        //            }
        //            else
        //            {
        //                Msg.Add("  Kindly insert record for Training Calendar ");
        //                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //            }

        //            //if (Addrs != null)
        //            //{
        //            //    if (Addrs != "")
        //            //    {
        //            //        int AddId = Convert.ToInt32(Addrs);
        //            //        var val = db.ProgramList
        //            //            //.Include(e => e.Budget)
        //            //                            .Include(e => e.TrainingType)
        //            //                            .Where(e => e.Id == AddId).SingleOrDefault();
        //            //        c.ProgramList = val;
        //            //    }

        //            //}
        //            //else
        //            //{
        //            //    Msg.Add("  Kindly insert record for Program List ");
        //            //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //            //}

        //            var chkdata = db.YearlyProgramAssignment
        //                //.Include(e => e.ProgramList)
        //                .Any(e => e.StartDate == c.StartDate && e.EndDate == c.EndDate && e.ProgramList.Id == e.ProgramList.Id);

        //            if (chkdata == true)
        //            {
        //                Msg.Add(" Record already exist with this data...");
        //                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //            }

        //            if (ModelState.IsValid)
        //            {
        //                using (TransactionScope ts = new TransactionScope())
        //                {
        //                    if (db.YearlyProgramAssignment.Any(o => o.StartDate == c.StartDate && o.EndDate == c.EndDate))
        //                    {
        //                        Msg.Add("  Yearly Program Assignment With This Period Already Exists.  ");
        //                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                    }
        //                    if (c.StartDate > c.EndDate)
        //                    {
        //                        Msg.Add(" Start Date Should Be Less Than End Date ");
        //                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                    }

        //                    c.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

        //                    YearlyProgramAssignment TrnCalender = new YearlyProgramAssignment()
        //                    {
        //                        StartDate = c.StartDate,
        //                        EndDate = c.EndDate,
        //                        ProgramList = c.ProgramList,

        //                        DBTrack = c.DBTrack
        //                    };
        //                    try
        //                    {
        //                        db.YearlyProgramAssignment.Add(TrnCalender);
        //                        var rtn_Obj = DBTrackFile.DBTrackSave("Training/Training", null, db.ChangeTracker, c.DBTrack);
        //                        DT_YearlyProgramAssignment DT_Corp = (DT_YearlyProgramAssignment)rtn_Obj;
        //                        //DT_Corp.ProgramList_Id = c.ProgramList.Id == null ? 0 : c.ProgramList.Id;
        //                        db.Create(DT_Corp);
        //                        db.SaveChanges();
        //                        //DBTrackFile.DBTrackSave("Core/P2b.Global", "C", corporate, null, "Corporate", null);
        //                        ts.Complete();
        //                        Msg.Add("  Data Saved successfully  ");
        //                        return Json(new Utility.JsonReturnClass { Id = TrnCalender.Id, Val = TrnCalender.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);


        //                    }
        //                    catch (DbUpdateConcurrencyException)
        //                    {
        //                        return RedirectToAction("Create", new { concurrencyError = true, id = c.Id });
        //                    }
        //                    catch (DataException /* dex */)
        //                    {

        //                        Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
        //                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                    }
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

        /* ---------------------------- Details Program>List -------------------------*/

        public ActionResult GetLookupDetailsProgramList(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.ProgramList.ToList();
                IEnumerable<ProgramList> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.ProgramList.ToList().Where(d => d.FullDetails.Contains(data));

                }
                else
                {
                    var list1 = db.ProgramList.ToList();

                    var r = (from a in list1 select new { srno = a.Id, lookupvalue = a.FullDetails }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.FullDetails }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        public ActionResult GetCalendarDetails(List<int> SkipIds)
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

        public ActionResult GetYearlyProgramAssignmentLKDetails(string data)
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
                    var list1 = db.YearlyProgramAssignment
                        //.Include(e => e.ProgramList)
                        .ToList();
                    // var list2 = fall.Except(list1);
                    //var details = "";
                    //foreach (var a in list2)
                    //{
                    //    details="Start Date: "+a.StartDate.Value.ToShortDateString()+",End Date: "+a.EndDate.Value.ToShortDateString()+"Program List: "+ a.ProgramList.FullDetails+"."; 
                    //}
                    var r = (from a in list1 select new { srno = a.Id, lookupvalue = "Start Date: " + a.TrainingYear.FromDate.Value.ToShortDateString() + ",End Date: " + a.TrainingYear.ToDate.Value.ToShortDateString() + ", Program List: " + a.Budget.Select(e => e.ProgramList.FullDetails) + "." }).Distinct();
                    return Json(r, JsonRequestBehavior.AllowGet);
                }
                var result = (from c in all
                              select new { c.Id, c.FullDetails }).Distinct();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        //public ActionResult GetTrainingCalendar(string data)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var fall = db.Calendar.Include(a => a.Name.ToString().ToUpper() == "TRAININGCALENDAR").Where(a => a.Default == true).SingleOrDefault();

        //       var SD = fall.FromDate.Value.ToShortDateString();
        //       var ED = fall.ToDate.Value.ToShortDateString(); 
        //       return Json(SD,ED, JsonRequestBehavior.AllowGet);
        //    }
        //}

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




        //[HttpPost]
        //public async Task<ActionResult> Delete(int data)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        List<string> Msg = new List<string>();
        //        try
        //        {
        //            YearlyProgramAssignment corporates = db.YearlyProgramAssignment
        //                //.Include(e => e.ProgramList)
        //                                               .Where(e => e.Id == data).SingleOrDefault();

        //            //ProgramList add = corporates.ProgramList;
        //            //if (add != null)
        //            //{
        //            //    Msg.Add(" Child record exists.Cannot remove it..  ");
        //            //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //            //}
        //            //var chk = db.TrainingSchedule.Include(e => e.TrainingCalendar).Any(q => q.TrainingCalendar.Id == data);
        //            //if (chk == true)
        //            //{
        //            //    Msg.Add(" Record used in Training Schedule .Cannot remove it..  ");
        //            //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //            //}
        //            //Corporate corporates = db.Corporate.Where(e => e.Id == data).SingleOrDefault();
        //            if (corporates.DBTrack.IsModified == true)
        //            {
        //                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                {
        //                    //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack, corporates, null, "Corporate");
        //                    DBTrack dbT = new DBTrack
        //                    {
        //                        Action = "D",
        //                        CreatedBy = corporates.DBTrack.CreatedBy != null ? corporates.DBTrack.CreatedBy : null,
        //                        CreatedOn = corporates.DBTrack.CreatedOn != null ? corporates.DBTrack.CreatedOn : null,
        //                        IsModified = corporates.DBTrack.IsModified == true ? true : false
        //                    };
        //                    corporates.DBTrack = dbT;
        //                    db.Entry(corporates).State = System.Data.Entity.EntityState.Modified;
        //                    var rtn_Obj = DBTrackFile.DBTrackSave("Training/Training", null, db.ChangeTracker, corporates.DBTrack);
        //                    //DT_YearlyProgramAssignment DT_Corp = (DT_YearlyProgramAssignment)rtn_Obj;
        //                    //DT_Corp.ProgramList_Id = corporates.ProgramList == null ? 0 : corporates.ProgramList.Id;

        //                    //db.Create(DT_Corp);
        //                    // db.SaveChanges();
        //                    //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack);
        //                    // await db.SaveChangesAsync();
        //                    //using (var context = new DataBaseContext())
        //                    //{
        //                    //   // DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack );
        //                    //}
        //                    ts.Complete();
        //                    Msg.Add("  Data removed successfully.  ");
        //                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                }
        //            }
        //            else
        //            {
        //                //if (add != null)
        //                //{
        //                //    Msg.Add(" Child record exists.Cannot remove it..  ");
        //                //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                //}

        //                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                {

        //                    try
        //                    {
        //                        DBTrack dbT = new DBTrack
        //                        {
        //                            Action = "D",
        //                            ModifiedBy = SessionManager.UserName,
        //                            ModifiedOn = DateTime.Now,
        //                            CreatedBy = corporates.DBTrack.CreatedBy != null ? corporates.DBTrack.CreatedBy : null,
        //                            CreatedOn = corporates.DBTrack.CreatedOn != null ? corporates.DBTrack.CreatedOn : null,
        //                            IsModified = corporates.DBTrack.IsModified == true ? false : false//,
        //                            //AuthorizedBy = SessionManager.UserName,
        //                            //AuthorizedOn = DateTime.Now
        //                        };

        //                        // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.UserName, ModifiedOn = DateTime.Now };

        //                        db.Entry(corporates).State = System.Data.Entity.EntityState.Deleted;
        //                        var rtn_Obj = DBTrackFile.DBTrackSave("Training/Training", null, db.ChangeTracker, dbT);
        //                        DT_YearlyProgramAssignment DT_Corp = (DT_YearlyProgramAssignment)rtn_Obj;

        //                        //DT_Corp.ProgramList_Id = add == null ? 0 : add.Id;

        //                        db.Create(DT_Corp);

        //                        await db.SaveChangesAsync();


        //                        //using (var context = new DataBaseContext())
        //                        //{
        //                        //    corporates.Address = add;
        //                        //    corporates.ContactDetails = conDet;
        //                        //    corporates.BusinessType = val;
        //                        //    DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, dbT);
        //                        //    //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", dbT);
        //                        //}
        //                        ts.Complete();
        //                        Msg.Add("  Data removed successfully.  ");
        //                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);


        //                    }
        //                    catch (RetryLimitExceededException /* dex */)
        //                    {
        //                        //Log the error (uncomment dex variable name and add a line here to write a log.)
        //                        //ModelState.AddModelError("", "Unable to delete. Try again, and if the problem persists, see your system administrator.");
        //                        //return RedirectToAction("Delete");
        //                        Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
        //                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                    }
        //                }
        //            }
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

        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    YearlyProgramAssignment corporates = db.YearlyProgramAssignment
                         .Include(e => e.YearlyTrainingCalendar)
                        .Include(e => e.TrainingYear)
                        .Include(e => e.YearlyTrainingCalendar.TrainigProgramCalendar)
                        .Include(e => e.YearlyTrainingCalendar.TrainigProgramCalendar.Select(x => x.ProgramList))
                        .Include(e => e.YearlyTrainingCalendar.TrainigProgramCalendar.Select(r => r.ProgramList.TrainingType))
                                                       .Where(e => e.Id == data).SingleOrDefault();

                    //Corporate corporates = db.Corporate.Where(e => e.Id == data).SingleOrDefault();
                    var subcategory = corporates.YearlyTrainingCalendar.TrainigProgramCalendar;
                    if (corporates.DBTrack.IsModified == true)
                    {

                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            if (subcategory != null)
                            {
                                var objITSection = corporates.YearlyTrainingCalendar.TrainigProgramCalendar.ToList();
                                if (objITSection.Count > 0)
                                {
                                    Msg.Add(" Child record exists.Cannot remove it..  ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    // return Json(new Object[] { "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                                    // return Json(new Object[] { "", "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                                }
                            }
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack, corporates, null, "Corporate");
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = corporates.DBTrack.CreatedBy != null ? corporates.DBTrack.CreatedBy : null,
                                CreatedOn = corporates.DBTrack.CreatedOn != null ? corporates.DBTrack.CreatedOn : null,
                                IsModified = corporates.DBTrack.IsModified == true ? true : false
                            };
                            corporates.DBTrack = dbT;
                            db.Entry(corporates).State = System.Data.Entity.EntityState.Deleted;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Training/Training", null, db.ChangeTracker, corporates.DBTrack);
                            //DT_Corporate DT_Corp = (DT_Corporate)rtn_Obj;
                            //DT_Corp.Address_Id = corporates.Address == null ? 0 : corporates.Address.Id;
                            //DT_Corp.BusinessType_Id = corporates.BusinessType == null ? 0 : corporates.BusinessType.Id;
                            //DT_Corp.ContactDetails_Id = corporates.ContactDetails == null ? 0 : corporates.ContactDetails.Id;
                            //db.Create(DT_Corp);
                            //// db.SaveChanges();
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack);
                            await db.SaveChangesAsync();
                            //using (var context = new DataBaseContext())
                            //{
                            //   // DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack );
                            //}
                            ts.Complete();
                            //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                            Msg.Add("  Data removed.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        var asq = corporates.YearlyTrainingCalendar.TrainigProgramCalendar != null;

                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            if (asq != null)
                            {
                                var corpRegion = (corporates.YearlyTrainingCalendar.TrainigProgramCalendar != null);
                                if (corpRegion != null)
                                {
                                    Msg.Add(" Child record exists.Cannot remove it..  ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }
                            }


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
                            //DT_Corporate DT_Corp = (DT_Corporate)rtn_Obj;
                            //DT_Corp.Address_Id = add == null ? 0 : add.Id;
                            //DT_Corp.BusinessType_Id = val == null ? 0 : val.Id;
                            //DT_Corp.ContactDetails_Id = conDet == null ? 0 : conDet.Id;
                            //db.Create(DT_Corp);

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
                            //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                            Msg.Add("  Data removed.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
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
            }
        }


        /*-----------------------------------------Edit --------------------------------*/
        public class returnDataClass
        {
            public int Id { get; set; }
            public string StartDate { get; set; }
            public string EndDate { get; set; }
        }

        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.TrainingProgramCalendar
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        StartDate = e.StartDate,
                        EndDate = e.EndDate,
                        Action = e.DBTrack.Action
                    }).ToList();

                var add_data = db.TrainingProgramCalendar //.YearlyProgramAssignment
                                  .Where(e => e.Id == data)
                                .Include(e => e.ProgramList)
                                 .Select(e => new
                                 {
                                     ProgramListId = e.ProgramList == null ? "" : e.ProgramList.Id.ToString(),
                                     ProgramListVal = e.ProgramList == null ? "" : e.ProgramList.FullDetails.ToString()
                                 })
                                .ToList();



                var W = db.DT_TrainingProgramCalendar
                     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                     (e => new
                     {
                         DT_Id = e.Id,
                         StartDate = e.StartDate,
                         EndDate = e.EndDate,
                         ProgramList_Val = e.ProgramList_Id == 0 ? "" : db.ProgramList.Where(x => x.Id == e.ProgramList_Id).Select(x => x.FullDetails).FirstOrDefault()

                     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var Corp = db.TrainingProgramCalendar.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, add_data, W, Auth, JsonRequestBehavior.AllowGet });
            }
        }

        //public int EditS(string Addrs, int data, YearlyProgramAssignment c, DBTrack dbT)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        var olddata1 = db.YearlyProgramAssignment.Where(e => e.Id == data).SingleOrDefault();

        //        if (Addrs != null)
        //        {
        //            if (Addrs != "")
        //            {
        //                //var val = db.ProgramList.Find(int.Parse(Addrs));
        //                //c.ProgramList = val;

        //                var add = db.YearlyProgramAssignment
        //                    //.Include(e => e.ProgramList)
        //                    .Where(e => e.Id == data).SingleOrDefault();
        //                IList<YearlyProgramAssignment> addressdetails = null;
        //                if (add.ProgramList != null)
        //                {
        //                    addressdetails = db.YearlyProgramAssignment.Where(x => x.ProgramList.Id == add.ProgramList.Id && x.Id == data).ToList();
        //                }
        //                else
        //                {
        //                    addressdetails = db.YearlyProgramAssignment.Where(x => x.Id == data).ToList();
        //                }
        //                if (addressdetails != null)
        //                {
        //                    foreach (var s in addressdetails)
        //                    {
        //                        s.ProgramList = c.ProgramList;
        //                        db.YearlyProgramAssignment.Attach(s);
        //                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                        // await db.SaveChangesAsync(false);
        //                        db.SaveChanges();
        //                        TempData["RowVersion"] = s.RowVersion;
        //                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //                    }
        //                }
        //            }
        //        }
        //        else
        //        {
        //            var addressdetails = db.YearlyProgramAssignment.Include(e => e.ProgramList).Where(x => x.Id == data).ToList();
        //            foreach (var s in addressdetails)
        //            {
        //                s.ProgramList = null;
        //                db.YearlyProgramAssignment.Attach(s);
        //                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
        //                //await db.SaveChangesAsync();
        //                db.SaveChanges();
        //                TempData["RowVersion"] = s.RowVersion;
        //                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
        //            }
        //        }


        //        var CurCorp = db.YearlyProgramAssignment.Find(data);
        //        TempData["CurrRowVersion"] = CurCorp.RowVersion;
        //        db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
        //        if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
        //        {
        //            c.DBTrack = dbT;
        //            YearlyProgramAssignment corp = new YearlyProgramAssignment()
        //            {
        //                StartDate = olddata1.StartDate,
        //                EndDate = olddata1.EndDate,
        //                Id = data,
        //                DBTrack = c.DBTrack
        //            };


        //            db.YearlyProgramAssignment.Attach(corp);
        //            db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
        //            db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //            //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
        //            return 1;
        //        }
        //        return 0;
        //    }
        //}

        //[HttpPost]
        //public async Task<ActionResult> EditSave(YearlyProgramAssignment c, int data, FormCollection form) // Edit submit
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        List<string> Msg = new List<string>();
        //        try
        //        {
        //            string Addrs = form["ProgramListlist"] == "0" ? "" : form["ProgramListlist"];

        //            bool Auth = form["Autho_Allow"] == "true" ? true : false;
        //            //string prog_assign = form["prog_assign"] == null ? "" : form["prog_assign"];

        //            //if (prog_assign != "")
        //            //{
        //            //    int calendarid = Convert.ToInt16(prog_assign);
        //            //    var chk = db.Calendar.Where(e => e.Id == calendarid).SingleOrDefault();
        //            //    if (chk.FromDate <= c.StartDate && chk.ToDate >= c.EndDate)
        //            //    {
        //            //        var alrDq = db.YearlyProgramAssignment
        //            //               .Any(q => ((q.StartDate <= c.StartDate && q.EndDate <= c.EndDate) || (q.StartDate <= c.StartDate && q.EndDate >= c.EndDate)) && (q.EndDate >= c.StartDate));
        //            //        if (alrDq == true)
        //            //        {
        //            //            Msg.Add("Yearly Program With this Period already exist.  ");
        //            //            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //            //        }
        //            //    }
        //            //    else
        //            //    {
        //            //        Msg.Add("  Kindly check the StartDate & ToDate inserted for record");
        //            //        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //            //    }
        //            //}
        //            //else
        //            //{
        //            //    Msg.Add("  Kindly insert record for Training Calendar ");
        //            //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //            //}


        //            if (Addrs != null)
        //            {
        //                if (Addrs != "")
        //                {
        //                    int AddId = Convert.ToInt32(Addrs);
        //                    var val = db.ProgramList.Include(e => e.Budget)
        //                                        .Include(e => e.TrainingType)
        //                                         .Where(e => e.Id == AddId).SingleOrDefault();
        //                    c.ProgramList = val;
        //                }
        //            }
        //            else
        //            {
        //                var chk = db.TrainingSchedule.Include(e => e.TrainingCalendar).Any(q => q.TrainingCalendar.Id == data);
        //                if (chk == true)
        //                {
        //                    Msg.Add(" Record used in Training Schedule .So Kindly insert record for Program List ");
        //                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                }
        //            }



        //            if (Auth == false)
        //            {


        //                if (ModelState.IsValid)
        //                {
        //                    try
        //                    {

        //                        if (c.StartDate > c.EndDate)
        //                        {
        //                            Msg.Add(" Start Date Should Be Less Than End Date ");
        //                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                        }
        //                        //DbContextTransaction transaction = db.Database.BeginTransaction();

        //                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                        {
        //                            YearlyProgramAssignment blog = null; // to retrieve old data
        //                            DbPropertyValues originalBlogValues = null;

        //                            using (var context = new DataBaseContext())
        //                            {
        //                                blog = context.YearlyProgramAssignment.Where(e => e.Id == data)
        //                                                        .Include(e => e.ProgramList).SingleOrDefault();
        //                                originalBlogValues = context.Entry(blog).OriginalValues;
        //                            }

        //                            c.DBTrack = new DBTrack
        //                            {
        //                                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                                Action = "M",
        //                                ModifiedBy = SessionManager.UserName,
        //                                ModifiedOn = DateTime.Now
        //                            };

        //                            int a = EditS(Addrs, data, c, c.DBTrack);



        //                            using (var context = new DataBaseContext())
        //                            {
        //                                var obj = DBTrackFile.DBTrackSave("Training/Training", originalBlogValues, db.ChangeTracker, c.DBTrack);
        //                                DT_YearlyProgramAssignment DT_Corp = (DT_YearlyProgramAssignment)obj;
        //                                //DT_Corp.ProgramList_Id = blog.ProgramList == null ? 0 : blog.ProgramList.Id;
        //                                //DT_Corp.StartDate = blog.StartDate == null ? null : blog.StartDate;
        //                                // DT_Corp.EndDate = blog.EndDate.ToString("dd/MM/yyyy");
        //                                db.Create(DT_Corp);
        //                                db.SaveChanges();
        //                            }
        //                            await db.SaveChangesAsync();
        //                            ts.Complete();



        //                            Msg.Add("  Record Updated");
        //                            return Json(new Utility.JsonReturnClass { Id = c.Id, Val = c.StartDate.ToString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                        }
        //                    }
        //                    catch (DbUpdateConcurrencyException ex)
        //                    {
        //                        var entry = ex.Entries.Single();
        //                        var clientValues = (YearlyProgramAssignment)entry.Entity;
        //                        var databaseEntry = entry.GetDatabaseValues();
        //                        if (databaseEntry == null)
        //                        {

        //                            Msg.Add(" Unable to save changes. The record was deleted by another user.");
        //                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);


        //                        }
        //                        else
        //                        {
        //                            var databaseValues = (YearlyProgramAssignment)databaseEntry.ToObject();
        //                            c.RowVersion = databaseValues.RowVersion;

        //                        }
        //                    }


        //                    Msg.Add("Record modified by another user.So refresh it and try to save again.");
        //                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                }
        //            }
        //            else
        //            {
        //                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //                {

        //                    YearlyProgramAssignment blog = null; // to retrieve old data
        //                    DbPropertyValues originalBlogValues = null;
        //                    YearlyProgramAssignment Old_Corp = null;

        //                    using (var context = new DataBaseContext())
        //                    {
        //                        blog = context.YearlyProgramAssignment.Where(e => e.Id == data).SingleOrDefault();
        //                        originalBlogValues = context.Entry(blog).OriginalValues;
        //                    }
        //                    c.DBTrack = new DBTrack
        //                    {
        //                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
        //                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
        //                        Action = "M",
        //                        IsModified = blog.DBTrack.IsModified == true ? true : false,
        //                        ModifiedBy = SessionManager.UserName,
        //                        ModifiedOn = DateTime.Now
        //                    };
        //                    YearlyProgramAssignment corp = new YearlyProgramAssignment()
        //                    {
        //                        StartDate = c.StartDate,
        //                        EndDate = c.EndDate,
        //                        Id = data,
        //                        DBTrack = c.DBTrack,
        //                        RowVersion = (Byte[])TempData["RowVersion"]
        //                    };


        //                    //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

        //                    using (var context = new DataBaseContext())
        //                    {
        //                        var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, corp, "YearlyProgramAssignment", c.DBTrack);
        //                        // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

        //                        Old_Corp = context.YearlyProgramAssignment.Where(e => e.Id == data)
        //                            .Include(e => e.ProgramList).SingleOrDefault();
        //                        DT_YearlyProgramAssignment DT_Corp = (DT_YearlyProgramAssignment)obj;
        //                        DT_Corp.ProgramList_Id = DBTrackFile.ValCompare(Old_Corp.ProgramList, c.ProgramList);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
        //                        db.Create(DT_Corp);
        //                        //db.SaveChanges();
        //                    }
        //                    blog.DBTrack = c.DBTrack;
        //                    db.YearlyProgramAssignment.Attach(blog);
        //                    db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
        //                    db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
        //                    db.SaveChanges();
        //                    ts.Complete();

        //                    Msg.Add("  Record Updated");
        //                    return Json(new Utility.JsonReturnClass { Id = c.Id, Val = c.StartDate.ToString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);


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

        [HttpPost]
        public async Task<ActionResult> EditSave(TrainingProgramCalendar tra, int data, FormCollection form) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    string Addrs = form["ProgramListlist"] == "0" ? "" : form["ProgramListlist"];

                    bool Auth = form["Autho_Allow"] == "true" ? true : false;


                    if (Auth == false)
                    {
                        if (ModelState.IsValid)
                        {
                            try
                            {

                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    TrainingProgramCalendar blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.TrainingProgramCalendar.Where(e => e.Id == data)
                                            .Include(e => e.ProgramList)
                                            .SingleOrDefault();
                                        originalBlogValues = context.Entry(blog).OriginalValues;
                                    }

                                    tra.DBTrack = new DBTrack
                                    {
                                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = SessionManager.UserName,
                                        ModifiedOn = DateTime.Now
                                    };




                                    if (Addrs != null)
                                    {
                                        if (Addrs != "")
                                        {
                                            var val = db.ProgramList.Find(int.Parse(Addrs));
                                            tra.ProgramList = val;

                                            var add = db.TrainingProgramCalendar.Include(e => e.ProgramList).Where(e => e.Id == data).SingleOrDefault();
                                            TrainingProgramCalendar addressdetails = null;
                                            if (add.ProgramList != null)
                                            {
                                                addressdetails = db.TrainingProgramCalendar.Where(x => x.ProgramList.Id == add.ProgramList.Id && x.Id == data).FirstOrDefault();
                                            }
                                            else
                                            {
                                                addressdetails = db.TrainingProgramCalendar.Where(x => x.Id == data).FirstOrDefault();
                                            }
                                            if (addressdetails != null)
                                            {
                                                addressdetails.ProgramList = tra.ProgramList;
                                                db.TrainingProgramCalendar.Attach(addressdetails);
                                                db.Entry(addressdetails).State = System.Data.Entity.EntityState.Modified;
                                                db.SaveChanges();
                                                TempData["RowVersion"] = addressdetails.RowVersion;
                                                db.Entry(addressdetails).State = System.Data.Entity.EntityState.Detached;

                                            }
                                        }
                                    }
                                    else
                                    {
                                        var addressdetails = db.TrainingProgramCalendar.Include(e => e.ProgramList).Where(x => x.Id == data).FirstOrDefault();
                                        addressdetails.ProgramList = null;
                                        db.TrainingProgramCalendar.Attach(addressdetails);
                                        db.Entry(addressdetails).State = System.Data.Entity.EntityState.Modified;
                                        //await db.SaveChangesAsync();
                                        db.SaveChanges();
                                        TempData["RowVersion"] = addressdetails.RowVersion;
                                        db.Entry(addressdetails).State = System.Data.Entity.EntityState.Detached;
                                    }



                                    var CurCorp = db.TrainingProgramCalendar.Find(data);
                                    TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                    db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;

                                    TrainingProgramCalendar corp = new TrainingProgramCalendar()
                                    {
                                        StartDate = tra.StartDate,
                                        EndDate = tra.EndDate,
                                        ProgramList = tra.ProgramList,
                                        Id = data,
                                        DBTrack = tra.DBTrack
                                    };


                                    db.TrainingProgramCalendar.Attach(corp);
                                    db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                                    db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                    //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);


                                    using (var context = new DataBaseContext())
                                    {
                                        var obj = DBTrackFile.DBTrackSave("Training/Training", originalBlogValues, db.ChangeTracker, tra.DBTrack);
                                        DT_TrainingProgramCalendar DT_Corp = (DT_TrainingProgramCalendar)obj;
                                        DT_Corp.ProgramList_Id = blog.ProgramList == null ? 0 : blog.ProgramList.Id;
                                        //DT_Corp.StartDate = blog.StartDate == null ? null : blog.StartDate;
                                        // DT_Corp.EndDate = blog.EndDate.ToString("dd/MM/yyyy");
                                        db.Create(DT_Corp);
                                        db.SaveChanges();
                                    }
                                    await db.SaveChangesAsync();
                                    ts.Complete();



                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = corp.Id, Val = corp.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (YearlyProgramAssignment)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {

                                    Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);


                                }
                                else
                                {
                                    var databaseValues = (YearlyProgramAssignment)databaseEntry.ToObject();
                                    tra.RowVersion = databaseValues.RowVersion;

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

                            YearlyProgramAssignment blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            YearlyProgramAssignment Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.YearlyProgramAssignment.Where(e => e.Id == data).SingleOrDefault();
                                originalBlogValues = context.Entry(blog).OriginalValues;
                            }
                            tra.DBTrack = new DBTrack
                            {
                                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                Action = "M",
                                IsModified = blog.DBTrack.IsModified == true ? true : false,
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };
                            YearlyProgramAssignment corp = new YearlyProgramAssignment()
                            {
                                //StartDate = c.StartDate,
                                //EndDate = c.EndDate,
                                Id = data,
                                DBTrack = tra.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };


                            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, corp, "YearlyProgramAssignment", tra.DBTrack);
                                // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                Old_Corp = context.YearlyProgramAssignment.Where(e => e.Id == data)
                                    //.Include(e => e.ProgramList)
                                    .SingleOrDefault();
                                DT_YearlyProgramAssignment DT_Corp = (DT_YearlyProgramAssignment)obj;
                                //DT_Corp.ProgramList_Id = DBTrackFile.ValCompare(Old_Corp.ProgramList, c.ProgramList);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
                                db.Create(DT_Corp);
                                //db.SaveChanges();
                            }
                            blog.DBTrack = tra.DBTrack;
                            db.YearlyProgramAssignment.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();

                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = tra.Id, Val = tra.StartDate.ToString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);


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
        public async Task<ActionResult> GridEditSave(TrainingProgramCalendar tra, int data, FormCollection form) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    string Addrs = form["ProgramListlistS1"] == "0" ? "" : form["ProgramListlistS1"];

                    bool Auth = form["Autho_Allow"] == "true" ? true : false;


                    if (Auth == false)
                    {
                        if (ModelState.IsValid)
                        {
                            try
                            {

                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    TrainingProgramCalendar blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.TrainingProgramCalendar.Where(e => e.Id == data)
                                            .Include(e => e.ProgramList)
                                            .SingleOrDefault();
                                        originalBlogValues = context.Entry(blog).OriginalValues;
                                    }

                                    tra.DBTrack = new DBTrack
                                    {
                                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = SessionManager.UserName,
                                        ModifiedOn = DateTime.Now
                                    };




                                    if (Addrs != null)
                                    {
                                        if (Addrs != "")
                                        {
                                            var val = db.ProgramList.Find(int.Parse(Addrs));
                                            tra.ProgramList = val;

                                            var add = db.TrainingProgramCalendar.Include(e => e.ProgramList).Where(e => e.Id == data).SingleOrDefault();
                                            TrainingProgramCalendar addressdetails = null;
                                            if (add.ProgramList != null)
                                            {
                                                addressdetails = db.TrainingProgramCalendar.Where(x => x.ProgramList.Id == add.ProgramList.Id && x.Id == data).FirstOrDefault();
                                            }
                                            else
                                            {
                                                addressdetails = db.TrainingProgramCalendar.Where(x => x.Id == data).FirstOrDefault();
                                            }
                                            if (addressdetails != null)
                                            {
                                                addressdetails.ProgramList = tra.ProgramList;
                                                db.TrainingProgramCalendar.Attach(addressdetails);
                                                db.Entry(addressdetails).State = System.Data.Entity.EntityState.Modified;
                                                db.SaveChanges();
                                                TempData["RowVersion"] = addressdetails.RowVersion;
                                                db.Entry(addressdetails).State = System.Data.Entity.EntityState.Detached;

                                            }
                                        }
                                    }
                                    else
                                    {
                                        var addressdetails = db.TrainingProgramCalendar.Include(e => e.ProgramList).Where(x => x.Id == data).FirstOrDefault();
                                        addressdetails.ProgramList = null;
                                        db.TrainingProgramCalendar.Attach(addressdetails);
                                        db.Entry(addressdetails).State = System.Data.Entity.EntityState.Modified;
                                        //await db.SaveChangesAsync();
                                        db.SaveChanges();
                                        TempData["RowVersion"] = addressdetails.RowVersion;
                                        db.Entry(addressdetails).State = System.Data.Entity.EntityState.Detached;
                                    }



                                    var CurCorp = db.TrainingProgramCalendar.Find(data);
                                    TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                    db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;

                                    TrainingProgramCalendar corp = new TrainingProgramCalendar()
                                    {
                                        StartDate = tra.StartDate,
                                        EndDate = tra.EndDate,
                                        ProgramList = tra.ProgramList,
                                        Id = data,
                                        DBTrack = tra.DBTrack
                                    };


                                    db.TrainingProgramCalendar.Attach(corp);
                                    db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                                    db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                    //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);


                                    using (var context = new DataBaseContext())
                                    {
                                        var obj = DBTrackFile.DBTrackSave("Training/Training", originalBlogValues, db.ChangeTracker, tra.DBTrack);
                                        DT_TrainingProgramCalendar DT_Corp = (DT_TrainingProgramCalendar)obj;
                                        DT_Corp.ProgramList_Id = blog.ProgramList == null ? 0 : blog.ProgramList.Id;
                                        //DT_Corp.StartDate = blog.StartDate == null ? null : blog.StartDate;
                                        // DT_Corp.EndDate = blog.EndDate.ToString("dd/MM/yyyy");
                                        db.Create(DT_Corp);
                                        db.SaveChanges();
                                    }
                                    await db.SaveChangesAsync();
                                    ts.Complete();



                                    //Msg.Add("  Record Updated");
                                    //return Json(new Utility.JsonReturnClass { Id = corp.Id, Val = corp.FullDetails,  = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    return Json(new { status = true, data = corp, responseText = "Record Updated..!" }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (YearlyProgramAssignment)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {

                                    Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);


                                }
                                else
                                {
                                    var databaseValues = (YearlyProgramAssignment)databaseEntry.ToObject();
                                    tra.RowVersion = databaseValues.RowVersion;

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

                            YearlyProgramAssignment blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            YearlyProgramAssignment Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.YearlyProgramAssignment.Where(e => e.Id == data).SingleOrDefault();
                                originalBlogValues = context.Entry(blog).OriginalValues;
                            }
                            tra.DBTrack = new DBTrack
                            {
                                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                Action = "M",
                                IsModified = blog.DBTrack.IsModified == true ? true : false,
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };
                            YearlyProgramAssignment corp = new YearlyProgramAssignment()
                            {
                                //StartDate = c.StartDate,
                                //EndDate = c.EndDate,
                                Id = data,
                                DBTrack = tra.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };


                            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Core/P2b.Global", "M", blog, corp, "YearlyProgramAssignment", tra.DBTrack);
                                // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                Old_Corp = context.YearlyProgramAssignment.Where(e => e.Id == data)
                                    //.Include(e => e.ProgramList)
                                    .SingleOrDefault();
                                DT_YearlyProgramAssignment DT_Corp = (DT_YearlyProgramAssignment)obj;
                                //DT_Corp.ProgramList_Id = DBTrackFile.ValCompare(Old_Corp.ProgramList, c.ProgramList);//Old_Corp.Address == c.Address ? 0 : Old_Corp.Address == null && c.Address != null ? c.Address.Id : Old_Corp.Address.Id;
                                db.Create(DT_Corp);
                                //db.SaveChanges();
                            }
                            blog.DBTrack = tra.DBTrack;
                            db.YearlyProgramAssignment.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();

                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = tra.Id, Val = tra.StartDate.ToString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);


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

                        YearlyProgramAssignment corp = db.YearlyProgramAssignment
                            //.Include(e => e.ProgramList)
                            .FirstOrDefault(e => e.Id == auth_id);

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

                        db.YearlyProgramAssignment.Attach(corp);
                        db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                        db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                        //db.SaveChanges();
                        var rtn_Obj = DBTrackFile.DBTrackSave("Training/Training", null, db.ChangeTracker, corp.DBTrack);
                        DT_YearlyProgramAssignment DT_Corp = (DT_YearlyProgramAssignment)rtn_Obj;
                        //DT_Corp.ProgramList_Id = corp.ProgramList == null ? 0 : corp.ProgramList.Id;
                        db.Create(DT_Corp);
                        await db.SaveChangesAsync();

                        ts.Complete();
                        return Json(new Object[] { corp.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                    }
                }
                else if (auth_action == "M")
                {

                    YearlyProgramAssignment Old_Corp = db.YearlyProgramAssignment
                        //.Include(e => e.ProgramList)
                                                      .Where(e => e.Id == auth_id).SingleOrDefault();


                    DT_YearlyProgramAssignment Curr_Corp = db.DT_YearlyProgramAssignment
                                                .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                                                .OrderByDescending(e => e.Id)
                                                .FirstOrDefault();

                    if (Curr_Corp != null)
                    {
                        YearlyProgramAssignment corp = new YearlyProgramAssignment();

                        string Addrs = Curr_Corp.ProgramList_Id == null ? null : Curr_Corp.ProgramList_Id.ToString();
                        // corp.StartDate = Curr_Corp.StartDate == null ? Old_Corp.StartDate : Curr_Corp.StartDate;
                        // corp.EndDate = Curr_Corp.EndDate == null ? Old_Corp.EndDate : Curr_Corp.EndDate;
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

                                    //int a = EditS(Addrs, auth_id, corp, corp.DBTrack);


                                    await db.SaveChangesAsync();

                                    ts.Complete();
                                    return Json(new Object[] { corp.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (YearlyProgramAssignment)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (YearlyProgramAssignment)databaseEntry.ToObject();
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
                        YearlyProgramAssignment corp = db.YearlyProgramAssignment.AsNoTracking()
                            //.Include(e => e.ProgramList)
                            .FirstOrDefault(e => e.Id == auth_id);

                        //ProgramList add = corp.ProgramList;


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

                        db.YearlyProgramAssignment.Attach(corp);
                        db.Entry(corp).State = System.Data.Entity.EntityState.Deleted;


                        var rtn_Obj = DBTrackFile.DBTrackSave("Training/Training", null, db.ChangeTracker, corp.DBTrack);
                        DT_YearlyProgramAssignment DT_Corp = (DT_YearlyProgramAssignment)rtn_Obj;
                        // DT_Corp.ProgramList_Id = corp.ProgramList == null ? 0 : corp.ProgramList.Id;

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

        public class LoanAdvReqChildDataClass //childgrid
        {
            public int Id { get; set; }
            public string StartDate { get; set; }
            public string EndDate { get; set; }
            public string FullDetails { get; set; }

        }

        public ActionResult Get_ProgramList(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var compid = int.Parse(Session["CompId"].ToString());
                    var db_data = db.CompanyTraining.Where(e => e.Company.Id == compid)
                            .Include(e => e.YearlyProgramAssignment)
                            .Include(e => e.YearlyProgramAssignment.Select(r => r.YearlyTrainingCalendar))
                            .Include(e => e.YearlyProgramAssignment.Select(r => r.TrainingYear))
                            .Include(e => e.YearlyProgramAssignment.Select(r => r.YearlyTrainingCalendar.TrainigProgramCalendar))
                            .Include(e => e.YearlyProgramAssignment.Select(r => r.YearlyTrainingCalendar.TrainigProgramCalendar.Select(y => y.ProgramList)))
                            .Include(e => e.YearlyProgramAssignment.Select(r => r.YearlyTrainingCalendar.TrainigProgramCalendar.Select(y => y.ProgramList.TrainingType)))
                           .AsNoTracking().AsParallel()
                            .SingleOrDefault();

                    if (db_data != null)
                    {
                        List<GridClass> returndata = new List<GridClass>();

                        foreach (var item in db_data.YearlyProgramAssignment)
                        {
                            Calendar TrCalendar = db.Calendar.Where(e => e.Name.LookupVal.ToString().ToUpper() == "TRAININGCALENDAR" && e.Default == true).FirstOrDefault();
                            if (item.TrainingYear.Id == TrCalendar.Id)
                            {
                                var PrCal = item.YearlyTrainingCalendar.TrainigProgramCalendar.ToList();
                                foreach (var item2 in PrCal)
                                {
                                    //foreach (var item2 in item1)
                                    //{
                                    returndata.Add(new GridClass
                                    {
                                        Id = item2.Id,
                                        StartDate = item2.StartDate.Value.ToShortDateString(),//item.TrainingYear.FromDate.Value.ToShortDateString(),
                                        EndDate = item2.EndDate.Value.ToShortDateString(),
                                        ProgramList = item2.ProgramList.FullDetails
                                    });
                                    //}

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

        public class returndatagridclass //Parentgrid
        {
            public string Id { get; set; }
            public string FullDetails { get; set; }

        }

        public ActionResult Emp_Grid(ParamModel param, string y)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var all = db.YearlyProgramAssignment
                        .Include(e => e.YearlyTrainingCalendar)
                        .Include(e => e.TrainingYear)
                        .Include(e => e.YearlyTrainingCalendar.TrainigProgramCalendar)
                        .Include(e => e.YearlyTrainingCalendar.TrainigProgramCalendar.Select(x => x.ProgramList))
                        .Include(e => e.YearlyTrainingCalendar.TrainigProgramCalendar.Select(r => r.ProgramList.TrainingType)).AsNoTracking()
                        .ToList();
                    // for searchs
                    IEnumerable<YearlyProgramAssignment> fall;

                    if (param.sSearch == null)
                    {
                        fall = all;
                    }
                    else
                    {

                        fall = all.Where(e => (e.Id.ToString().Contains(param.sSearch))

                                  || (e.FullDetails.ToString().ToUpper().Contains(param.sSearch.ToUpper()))

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
                        foreach (var item in fall)
                        {
                            if (y == null)
                            {
                                result.Add(new returndatagridclass
                                {
                                    Id = item.Id.ToString(),
                                    FullDetails = "Training Calendar : " + item.TrainingYear.FullDetails != null ? "Training Calendar : " + item.TrainingYear.FullDetails : null,

                                });
                            }
                            else
                            {
                                int TrainingId = Convert.ToInt32(y);

                                if (item.TrainingYear.Id == TrainingId)
                                {
                                    result.Add(new returndatagridclass
                                    {
                                        Id = item.Id.ToString(),
                                        FullDetails = "Training Calendar : " + item.TrainingYear.FullDetails != null ? "Training Calendar : " + item.TrainingYear.FullDetails : null,

                                    });
                                }

                            }

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




        public ActionResult GridDelete(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var LvEP = db.TrainingProgramCalendar.Find(data);
                db.TrainingProgramCalendar.Remove(LvEP);
                db.SaveChanges();
                List<string> Msgs = new List<string>();
                Msgs.Add("Record Deleted Successfully ");
                return Json(new Utility.JsonReturnClass { success = true, responseText = Msgs }, JsonRequestBehavior.AllowGet);

            }
        }

        public ActionResult GridEditData(int data)
        {
            var returnlist = new List<returnDataClass>();
            using (DataBaseContext db = new DataBaseContext())
            {
                if (data != null && data != 0)
                {
                    var retrundataList = db.TrainingProgramCalendar.Where(e => e.Id == data).ToList();
                    foreach (var a in retrundataList)
                    {
                        returnlist.Add(new returnDataClass()
                        {
                            Id = a.Id,
                            StartDate = a.StartDate.Value.ToShortDateString(),
                            EndDate = a.EndDate.Value.ToShortDateString(),

                        });
                    }



                    var add_data = db.TrainingProgramCalendar //.YearlyProgramAssignment
                                  .Where(e => e.Id == data)
                                .Include(e => e.ProgramList)
                                 .Select(e => new
                                 {
                                     ProgramListId = e.ProgramList == null ? "" : e.ProgramList.Id.ToString(),
                                     ProgramListVal = e.ProgramList == null ? "" : e.ProgramList.FullDetails.ToString()
                                 })
                                .ToList();


                    // return Json(new { returndata = returnlist }, JsonRequestBehavior.AllowGet);
                    return this.Json(new Object[] { returnlist, add_data, "", "", "", JsonRequestBehavior.AllowGet });
                }
                else
                {
                    return View();
                }
            }
        }

    }
}
