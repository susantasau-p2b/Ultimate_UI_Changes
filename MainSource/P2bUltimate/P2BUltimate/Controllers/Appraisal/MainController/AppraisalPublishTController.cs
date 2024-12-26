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
using Training;


namespace P2BUltimate.Controllers.Appraisal.MainController
{
    [AuthoriseManger]
    public class AppraisalPublishTController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();
        //
        // GET: /AppraisalPublishT/ 
        public ActionResult Index()
        {
            return View("~/Views/Appraisal/MainViews/AppraisalPublishT/Index.cshtml");
        }

        public ActionResult partial()
        {
            return View("~/Views/Shared/Appraisal/_AppraisalPublishT.cshtml");
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

        public ActionResult GetApplicableData(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                int Id = Convert.ToInt32(data);

                var OPayscale = db.AppraisalSchedule.Where(e => e.AppraisalPeriodCalendar.Id == Id).FirstOrDefault();

                var qurey = OPayscale;
                return Json(qurey, JsonRequestBehavior.AllowGet);
            }
        }

        public class DeserializeClass
        {
            public string Id { get; set; }
            public int MaxPoints { get; set; }
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
                    var db_data = db.EmployeeAppraisal
                           .Include(e => e.Employee)
                        .Include(e => e.EmpAppEvaluation)
                        .Include(e => e.EmpAppEvaluation.Select(a => a.EmpAppRatingConclusion))
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

                            // var obj = item2.AppAssignment.AppRatingObjective.Select(e => e.ObjectiveWordings.LookupVal.ToString()).SingleOrDefault();


                            returndata.Add(new DeserializeClass
                            {
                                Id = item.Id.ToString(),
                                MaxPoints = item.MaxPoints
                                // AppraisalPeriodCalendar = item.AppraisalPeriodCalendar,

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

        [HttpPost]
        public ActionResult Create(AppraisalSchedule p, FormCollection form) //Create submit
        {
            List<String> Msg = new List<String>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string AppCalendardrop = form["AppCalendardrop"] == "0" ? null : form["AppCalendardrop"];
                    string Emp = form["Employee-Table"] == "0" ? "" : form["Employee-Table"];
                    string pubdate = form["lblPubDateD"] == "0" ? "" : form["lblPubDateD"];
                    string Batch_Name = form["Batch_Name"] == "0" ? "" : form["Batch_Name"];
                    //string pay = form["pay_id"] == "0" ? "" : form["pay_id"];
                    //string fun = form["fun_id"] == "0" ? "" : form["fun_id"];

                    //if (pay != null)
                    //{
                    //    var v = db.PayStruct.Find(int.Parse(geo));
                    //    p.PayStruct = v;
                    //}
                    //if (geo != null)
                    //{
                    //    var v = db.GeoStruct.Find(int.Parse(geo));
                    //    p.GeoStruct = v;
                    //}
                    //if (fun != null)
                    //{
                    //    var v = db.FuncStruct.Find(int.Parse(geo));
                    //    p.FuncStruct = v;
                    //}


                    if (AppCalendardrop == null)
                    {
                        Msg.Add(" Kindly select Appriasal Calendar  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    if (AppCalendardrop != null)
                    {
                        var val = db.Calendar.Find(int.Parse(AppCalendardrop));
                    }

                    int SchId = Convert.ToInt32(Batch_Name);
                    int CalId = Convert.ToInt32(AppCalendardrop);

                    var OAppSche = db.AppraisalSchedule.Where(e => e.AppraisalPeriodCalendar.Id == CalId && e.Id == SchId).FirstOrDefault();

                 

                    List<int> ids = null;
                    if (Emp != null && Emp != "0" && Emp != "false")
                    {
                        ids = Utility.StringIdsToListIds(Emp);
                    }
                    else
                    {
                        Msg.Add(" Kindly select employee  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    Employee OEmployee = null;
                    EmployeePayroll OEmployeePayroll = null;

                    if (ids.Count() > 0)
                    {
                        foreach (int EmId in ids)
                        {
                            string Calid = Convert.ToString(CalId);
                            string schId = Convert.ToString(SchId);
                            var getEmployee = db.Employee.Include(a => a.GeoStruct).Include(a => a.PayStruct).Include(a => a.FuncStruct).Where(a => a.Id == EmId).SingleOrDefault();

                            var chkAppSchedule = db.AppraisalSchedule.Select(a => new
                            {
                                appraisalCalendarid = a.AppraisalPeriodCalendar.Id.ToString(),
                                geoStructid = a.GeoStruct.Id.ToString(),
                                payStructid = a.PayStruct.Id.ToString(),
                                funStructid = a.FuncStruct.Id.ToString(),
                                appraisalSchid = a.Id.ToString(),
                            }).Where(e => e.appraisalCalendarid == Calid && e.appraisalSchid == schId &&
                                (e.funStructid == getEmployee.FuncStruct.Id.ToString() ||
                                e.geoStructid == getEmployee.GeoStruct.Id.ToString() ||
                                e.payStructid == getEmployee.PayStruct.Id.ToString())).FirstOrDefault();

                            var chkEmpAppraisalExist = db.EmployeeAppraisal.Include(e => e.Employee)
                                                        .Include(e => e.EmpAppEvaluation)
                                                        .Include(e => e.EmpAppEvaluation.Select(s => s.AppraisalSchedule))
                                                        .Include(e => e.EmpAppEvaluation.Select(s => s.AppraisalSchedule.AppraisalPeriodCalendar))
                                                        .Where(e => e.Employee.Id == EmId).FirstOrDefault();
                            var chkEmpAppEvaluationList = chkEmpAppraisalExist.EmpAppEvaluation.Where(e => e.AppraisalPeriodCalendar.Id == CalId && e.AppraisalSchedule.Id.ToString() == chkAppSchedule.appraisalSchid.ToString()).ToList();
                            foreach (var itemchkevaluation in chkEmpAppEvaluationList)
                            {
                                if (itemchkevaluation != null)
                                {
                                    Msg.Add("For " + getEmployee.EmpCode + " Appraisal Publish already exist.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }

                            }
                        }
                    }


                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                       new System.TimeSpan(0, 30, 0)))
                        {

                            //if (db.AppraisalPublishT.Any(o => o.Id == p.Id))
                            //{
                            //    Msg.Add("Code Already Exists.");
                            //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //}


                            p.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };                     //???????

                            AppAssignment appas = null;
                            foreach (var items in ids)
                            {
                                var emp = db.Employee.Include(a => a.GeoStruct).Include(a => a.PayStruct).Include(a => a.FuncStruct).Where(a => a.Id == items).SingleOrDefault();
                                p.GeoStruct = emp.GeoStruct;
                                p.PayStruct = emp.PayStruct;
                                p.FuncStruct = emp.FuncStruct;

                                if (p.GeoStruct != null)
                                {
                                    appas = db.AppAssignment.Include(e => e.AppCategory).Include(e => e.AppSubCategory).Include(a => a.FuncStruct).Include(a => a.GeoStruct).Include(a => a.PayStruct).Where(a => a.GeoStruct.Id == p.GeoStruct.Id).FirstOrDefault();
                                    if (appas != null)
                                    {
 



                                        EmpAppEvaluation evp = new EmpAppEvaluation()
                                         {
                                             //EmpAppRatingConclusion = empp,
                                             AppraisalPeriodCalendar = appas.AppraisalCalendar,
                                             MaxPoints = appas.MaxRatingPoints,
                                             AppraisalSchedule = db.AppraisalSchedule.Find(SchId),
                                             DBTrack = appas.DBTrack,
                                             IsTrClose = true
                                         };
                                        db.EmpAppEvaluation.Add(evp);
                                        db.SaveChanges();

                                        var EmployeeAppraisal = new EmployeeAppraisal();


                                        List<EmpAppEvaluation> AppcategoryLost = new List<EmpAppEvaluation>();
                                        AppcategoryLost.Add(db.EmpAppEvaluation.Find(evp.Id));

                                        EmployeeAppraisal = db.EmployeeAppraisal.Include(e => e.EmpAppEvaluation).Where(e => e.Employee.Id == items).FirstOrDefault();
                                        if (EmployeeAppraisal != null)
                                        {
                                            if (EmployeeAppraisal.EmpAppEvaluation.Count() > 0)
                                            {
                                                AppcategoryLost.AddRange(EmployeeAppraisal.EmpAppEvaluation);
                                            }
                                            EmployeeAppraisal.EmpAppEvaluation = AppcategoryLost;
                                            db.EmployeeAppraisal.Attach(EmployeeAppraisal);
                                            db.Entry(EmployeeAppraisal).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();
                                            //db.Entry(EmployeeAppraisal).State = System.Data.Entity.EntityState.Detached;
                                            //db.SaveChanges();
                                            //Msg.Add("Code Already Exists.");
                                            //return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        }
                                        else
                                        {
                                            EmployeeAppraisal.DBTrack = p.DBTrack;
                                            EmployeeAppraisal.Employee = emp;
                                            EmployeeAppraisal.EmpAppEvaluation = AppcategoryLost;
                                            db.EmployeeAppraisal.Add(EmployeeAppraisal);
                                            db.SaveChanges();
                                        }
                                       // db.SaveChanges();
                                    }
                                }


                                if (p.FuncStruct != null && appas == null)
                                {
                                    appas = db.AppAssignment.Include(e => e.AppCategory).Include(e => e.AppSubCategory).Include(a => a.FuncStruct).Include(a => a.GeoStruct).Include(a => a.PayStruct).Where(a => a.FuncStruct.Id == p.FuncStruct.Id).FirstOrDefault();
                                    if (appas != null)
                                    {

                                       


                                        EmpAppEvaluation evp = new EmpAppEvaluation()
                                        {
                                            //EmpAppRatingConclusion = empp,
                                            AppraisalPeriodCalendar = appas.AppraisalCalendar,
                                            MaxPoints = appas.MaxRatingPoints,
                                            AppraisalSchedule = db.AppraisalSchedule.Find(SchId),
                                            DBTrack = appas.DBTrack,
                                            IsTrClose = true
                                        };
                                        db.EmpAppEvaluation.Add(evp);


                                        var EmployeeAppraisal = new EmployeeAppraisal();


                                        List<EmpAppEvaluation> AppcategoryLost = new List<EmpAppEvaluation>();
                                        AppcategoryLost.Add(db.EmpAppEvaluation.Find(evp.Id));

                                        EmployeeAppraisal = db.EmployeeAppraisal.Include(e => e.EmpAppEvaluation).Where(e => e.Employee.Id == items).FirstOrDefault();
                                        if (EmployeeAppraisal != null)
                                        {
                                            if (EmployeeAppraisal.EmpAppEvaluation.Count() > 0)
                                            {
                                                AppcategoryLost.AddRange(EmployeeAppraisal.EmpAppEvaluation);
                                            }
                                            EmployeeAppraisal.EmpAppEvaluation = AppcategoryLost;
                                            db.EmployeeAppraisal.Attach(EmployeeAppraisal);
                                            db.Entry(EmployeeAppraisal).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();
                                            //db.Entry(EmployeeAppraisal).State = System.Data.Entity.EntityState.Detached;

                                            //db.SaveChanges();
                                            //Msg.Add("Code Already Exists.");
                                            //return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        }
                                        else
                                        {
                                            EmployeeAppraisal.DBTrack = p.DBTrack;
                                            EmployeeAppraisal.Employee = emp;
                                            EmployeeAppraisal.EmpAppEvaluation = AppcategoryLost;
                                            db.EmployeeAppraisal.Add(EmployeeAppraisal);
                                            db.SaveChanges();
                                        }
                                        
                                    }
                                }


                                if (p.PayStruct != null && appas == null)
                                {
                                    appas = db.AppAssignment.Include(e => e.AppCategory).Include(e => e.AppSubCategory).Include(a => a.FuncStruct).Include(a => a.GeoStruct).Include(a => a.PayStruct).Where(a => a.PayStruct.Id == p.PayStruct.Id).FirstOrDefault();
                                    if (appas != null)
                                    {

                                        


                                        EmpAppEvaluation evp = new EmpAppEvaluation()
                                        {
                                            //EmpAppRatingConclusion = empp,
                                            AppraisalPeriodCalendar = appas.AppraisalCalendar,
                                            MaxPoints = appas.MaxRatingPoints,
                                            AppraisalSchedule = db.AppraisalSchedule.Find(SchId),
                                            DBTrack = appas.DBTrack,
                                            IsTrClose = true
                                        };
                                        db.EmpAppEvaluation.Add(evp);
                                        db.SaveChanges();

                                        EmployeeAppraisal EmpApp = new EmployeeAppraisal();
                                        EmpApp = db.EmployeeAppraisal.Include(e => e.EmpAppEvaluation).Where(e => e.Employee.Id == items).FirstOrDefault();

                                        List<EmpAppEvaluation> AppcategoryLost = new List<EmpAppEvaluation>();
                                        AppcategoryLost.Add(db.EmpAppEvaluation.Find(evp.Id));

                                        if (EmpApp != null)
                                        {

                                            if (EmpApp.EmpAppEvaluation.Count() > 0)
                                            {
                                                AppcategoryLost.AddRange(EmpApp.EmpAppEvaluation);
                                            }
                                                

                                            
                                            EmpApp.EmpAppEvaluation = AppcategoryLost;
                                            
                                            db.EmployeeAppraisal.Attach(EmpApp);
                                            db.Entry(EmpApp).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();
                                            //db.Entry(EmpApp).State = System.Data.Entity.EntityState.Detached;
                                        }
                                        else
                                        {
                                            EmpApp.DBTrack = p.DBTrack;
                                            EmpApp.Employee = emp;
                                            EmpApp.EmpAppEvaluation = AppcategoryLost;
                                            db.EmployeeAppraisal.Add(EmpApp);
                                            db.SaveChanges();
                                        }
                                       
                                    }
                                }



                            }


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
                try
                {
                    var all = db.EmployeeAppraisal.Include(e => e.Employee).Include(e => e.Employee.EmpName).Include(e => e.Employee.ServiceBookDates)
                        .Include(e => e.Employee.GeoStruct).Include(e => e.Employee.GeoStruct.Location.LocationObj).Include(e => e.Employee.FuncStruct)
                        .Include(e => e.Employee.FuncStruct.Job).Include(e => e.Employee.PayStruct).Include(e => e.Employee.PayStruct.Grade).Include(e => e.EmpAppEvaluation)
                        .ToList();
                    // for searchs
                    IEnumerable<EmployeeAppraisal> fall;
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
                                  || (e.Employee.ServiceBookDates.JoiningDate.Value.ToString("dd/MM/yyyy").Contains(param.sSearch))
                                  || (e.Employee.FuncStruct.Job.Name.ToString().ToUpper().Contains(param.sSearch.ToUpper()))
                                  || (e.Employee.PayStruct.Grade.Name.ToString().ToUpper().Contains(param.sSearch.ToUpper()))
                                  || (e.Employee.GeoStruct.Location.LocationObj.ToString().ToUpper().Contains(param.sSearch.ToUpper()))
                                  ).ToList();
                    }

                    //for column sorting
                    var sortindex = Convert.ToInt32(Request["iSortCol_0"]);

                    Func<EmployeeAppraisal, string> orderfunc = (c =>
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
                            result.Add(new returndatagridclass
                            {
                                Id = item.Id.ToString(),
                                Code = item.Employee != null ? item.Employee.EmpCode : null,
                                Name = item.Employee != null && item.Employee.EmpName != null ? item.Employee.EmpName.FullNameFML : null,
                                JoiningDate = item.Employee.ServiceBookDates.JoiningDate != null ? item.Employee.ServiceBookDates.JoiningDate.Value.ToString("dd/MM/yyyy") : null,
                                Job = item.Employee.FuncStruct != null && item.Employee.FuncStruct.Job != null ? item.Employee.FuncStruct.Job.Name : null,
                                Grade = item.Employee.PayStruct != null && item.Employee.PayStruct.Grade != null ? item.Employee.PayStruct.Grade.Name : null,
                                Location = item.Employee.GeoStruct != null && item.Employee.GeoStruct.Location != null && item.Employee.GeoStruct.Location.LocationObj != null ? item.Employee.GeoStruct.Location.LocationObj.LocDesc : null,
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



        //public ActionResult P2BGrid(P2BGrid_Parameters gp)
        //{
        //    List<string> Msg = new List<string>();
        //    try
        //    {
        //        DataBaseContext db = new DataBaseContext();
        //        int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
        //        int pageSize = gp.rows;
        //        int totalPages = 0;
        //        int totalRecords = 0;
        //        var jsonData = (Object)null;
        //        IEnumerable<AppraisalSchedule> Corporate = null;
        //        if (gp.IsAutho == true)
        //        {
        //            Corporate = db.AppraisalSchedule.Include(e => e.AppraisalPublish).Include(e => e.AppraisalPublish.AppraisalPeriodCalendar).Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
        //        }
        //        else
        //        {
        //            Corporate = db.AppraisalSchedule.Include(e => e.AppraisalPublish).Include(e => e.AppraisalPublish.AppraisalPeriodCalendar).AsNoTracking().ToList();
        //        }

        //        IEnumerable<AppraisalSchedule> IE;
        //        if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
        //        {
        //            IE = Corporate;
        //            if (gp.searchOper.Equals("eq"))
        //            {
        //                jsonData = IE.Where(e => (e.Id.ToString().Contains(gp.searchString))
        //                    //|| (e.AppraisalPublish.AppraisalPeriodCalendar.ToString().Contains(gp.searchString))
        //                    //|| (e.AppraisalPublish.SpanPeriod.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
        //                    ).Select(a => new { a.Id, a.AppraisalPublish.AppraisalPeriodCalendar, a.AppraisalPublish.SpanPeriod }).ToList();
        //                //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.AppraisalPublish.AppraisalPeriodCalendar.FromDate.Value.ToString("dd/MM/yyyy") + " - " + a.AppraisalPublish.AppraisalPeriodCalendar.ToDate.Value.ToString("dd/MM/yyyy"), a.AppraisalPublish.PublishDate.Value.ToString("dd/MM/yyyy"), a.AppraisalPublish.SpanPeriod, a.AppraisalPublish.Extension }).ToList();
        //            }
        //            totalRecords = IE.Count();
        //        }
        //        else
        //        {
        //            IE = Corporate;
        //            Func<AppraisalPublishT, dynamic> orderfuc;
        //            if (gp.sidx == "Id")
        //            {
        //                orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
        //            }
        //            else
        //            {
        //                orderfuc = (c => gp.sidx == "SpanPeriod" ? c.AppraisalPublish.SpanPeriod :
        //                                 gp.sidx == "Extension" ? c.AppraisalPublish.Extension : 0);
        //            }
        //            if (gp.sord == "asc")
        //            {
        //                IE = IE.OrderBy(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, a.AppraisalPublish.AppraisalPeriodCalendar.FromDate.Value.ToString("dd/MM/yyyy") + " - " + a.AppraisalPublish.AppraisalPeriodCalendar.ToDate.Value.ToString("dd/MM/yyyy"), a.AppraisalPublish.PublishDate.Value.ToString("dd/MM/yyyy"), Convert.ToString(a.AppraisalPublish.SpanPeriod), Convert.ToString(a.AppraisalPublish.Extension), 0 }).ToList();
        //            }
        //            else if (gp.sord == "desc")
        //            {
        //                IE = IE.OrderByDescending(orderfuc);
        //                jsonData = IE.Select(a => new Object[] { a.Id, a.AppraisalPublish.AppraisalPeriodCalendar.FromDate.Value.ToString("dd/MM/yyyy") + " - " + a.AppraisalPublish.AppraisalPeriodCalendar.ToDate.Value.ToString("dd/MM/yyyy"), a.AppraisalPublish.PublishDate.Value.ToString("dd/MM/yyyy"), Convert.ToString(a.AppraisalPublish.SpanPeriod), Convert.ToString(a.AppraisalPublish.Extension), 0 }).ToList();
        //            }
        //            if (pageIndex > 1)
        //            {
        //                int h = pageIndex * pageSize;
        //                jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.AppraisalPublish.AppraisalPeriodCalendar.FromDate.Value.ToString("dd/MM/yyyy") + " - " + a.AppraisalPublish.AppraisalPeriodCalendar.ToDate.Value.ToString("dd/MM/yyyy"), a.AppraisalPublish.PublishDate.Value.ToString("dd/MM/yyyy"), Convert.ToString(a.AppraisalPublish.SpanPeriod), Convert.ToString(a.AppraisalPublish.Extension) }).ToList();
        //            }
        //            totalRecords = Corporate.Count();
        //        }
        //        if (totalRecords > 0)
        //        {
        //            totalPages = (int)Math.Ceiling((float)totalRecords / (float)gp.rows);
        //        }
        //        if (gp.page > totalPages)
        //        {
        //            gp.page = totalPages;
        //        }
        //        var JsonData = new
        //        {
        //            page = gp.page,
        //            rows = jsonData,
        //            records = totalRecords,
        //            total = totalPages
        //        };
        //        return Json(JsonData, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        LogFile Logfile = new LogFile();
        //        ErrorLog Err = new ErrorLog()
        //        {
        //            ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
        //            ExceptionMessage = ex.Message,
        //            ExceptionStackTrace = ex.StackTrace,
        //            LineNo = Convert.ToString(new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
        //            LogTime = DateTime.Now
        //        };
        //        Logfile.CreateLogFile(Err);
        //        Msg.Add(ex.Message);
        //        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //    }
        //}

        public ActionResult GetAppraisalScheduleDetails(List<int> SkipIds)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {


                var fall = db.AppraisalSchedule.GroupBy(e => e.BatchName).Select(e => e.FirstOrDefault()).ToList();

                //var fall = db.AppraisalSchedule.ToList();
                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.AppraisalSchedule
                                .Include(e => e.AppraisalPublish)
                                .Include(e => e.AppraisalPeriodCalendar)
                                .Include(e => e.GeoStruct)
                                .Include(e => e.FuncStruct)
                                .Include(e => e.PayStruct)
                                .Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }
              

                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.BatchName }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);


            }
            // return View();
        }

        public ActionResult Get_Employelist(string AppCal, string SchName)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                DateTime? dt = null;
                string monthyr = "";
                DateTime? dtChk = null;
                int lastDayOfMonth;

                List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                List<Employee> data = new List<Employee>();
                var compid = Convert.ToInt32(Session["CompId"].ToString());
                db.Database.CommandTimeout = 3600;

                var empdata = db.EmployeeAppraisal.Include(e => e.Employee).Include(e => e.Employee.EmpName)
                    .Include(e => e.Employee.GeoStruct)
                    .Include(e => e.Employee.PayStruct)
                    .Include(e => e.Employee.FuncStruct)
                     .Include(e => e.Employee.ServiceBookDates)
                    .Where(e=>e.Employee.ServiceBookDates.ServiceLastDate==null)
                  .AsNoTracking().AsParallel() .ToList();

                List<Employee> Emp = new List<Employee>();
                if (SchName != null)
                {

                    var AppSchdata = db.AppraisalSchedule.Include(e => e.FuncStruct)
                        .Include(e => e.GeoStruct).Include(e => e.PayStruct).Where(e => e.BatchName == SchName).ToList();

                    int AppSchId = AppSchdata.FirstOrDefault().Id;
                    
                        foreach (var item1 in AppSchdata)
                        {
                            if (item1.PayStruct != null)
                            {
                                foreach (var item in empdata)
                                { 
                                        if (item.Employee.PayStruct != null && item.Employee.PayStruct.Id == item1.PayStruct.Id)
                                        {
                                            EmpAppEvaluation OEmpAppEval = db.EmployeeAppraisal.Include(e => e.EmpAppEvaluation)
                                                  .Include(e => e.EmpAppEvaluation.Select(r => r.AppraisalSchedule))
                                            .Where(e => e.Employee.Id == item.Employee.Id).FirstOrDefault().EmpAppEvaluation.Where(e => e.AppraisalSchedule.Id == AppSchId).FirstOrDefault();
                                            if (OEmpAppEval == null)
                                            { Emp.Add(item.Employee); }
                                            
                                        }
                                    
                                }
                            }
                            if (item1.FuncStruct != null)
                            {
                                foreach (var item in empdata)
                                {
                                    if (item.Employee.FuncStruct != null && item.Employee.FuncStruct.Id == item1.FuncStruct.Id)
                                    {
                                        EmpAppEvaluation OEmpAppEval = db.EmployeeAppraisal.Include(e => e.EmpAppEvaluation)
                                              .Include(e => e.EmpAppEvaluation.Select(r => r.AppraisalSchedule))
                                             .Where(e => e.Employee.Id == item.Employee.Id).FirstOrDefault().EmpAppEvaluation.Where(e => e.AppraisalSchedule.Id == AppSchId).FirstOrDefault();
                                        if (OEmpAppEval == null)
                                        { Emp.Add(item.Employee); }
                                       
                                    }

                                }
                            }
                            if (item1.GeoStruct != null)
                            {
                                foreach (var item in empdata)
                                {
                                    if (item.Employee.GeoStruct != null && item.Employee.GeoStruct.Id == item1.GeoStruct.Id)
                                    {
                                        EmpAppEvaluation OEmpAppEval = db.EmployeeAppraisal.Include(e => e.EmpAppEvaluation)
                                              .Include(e => e.EmpAppEvaluation.Select(r => r.AppraisalSchedule))
                                            .Where(e => e.Employee.Id == item.Employee.Id).FirstOrDefault().EmpAppEvaluation.Where(e => e.AppraisalSchedule.Id == AppSchId).FirstOrDefault();
                                        if (OEmpAppEval == null)
                                        { Emp.Add(item.Employee); }
                                        
                                    }

                                }
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

    }
}