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
    public class LoginAccessController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();
        //
        // GET: /TrainingPresenty/
        public ActionResult Index()
        {
            return View("~/Views/Core/MainViews/LoginAccess/Index.cshtml");
        }

        public ActionResult GridPartial()
        {
            return View("~/Views/Shared/Core/_LoginAccessPartial.cshtml");
        }

        public class P2BCrGridData
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }

        }

        public ActionResult Get_Employelist(P2BGrid_Parameters gp, bool EmployeeSource)
        {

            try
            {
                //string monthyr = param;
                DataBaseContext db = new DataBaseContext();
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                List<Employee> data = new List<Employee>();
                List<Employee> model = new List<Employee>();

                var compid = Convert.ToInt32(Session["CompId"].ToString());
                var empdata = db.CompanyPayroll.Where(e => e.Company.Id == compid)
                    .Include(e => e.EmployeePayroll)
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee))
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee.EmpName))
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee.EmpName))
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee.ServiceBookDates))
                    .Include(e => e.EmployeePayroll.Select(a => a.TransferServiceBook))
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee.Login)).AsNoTracking().OrderBy(e => e.Id)
                   .SingleOrDefault();

                var emp = empdata.EmployeePayroll.ToList();

                foreach (var z in emp)
                {
                    if (EmployeeSource == true)
                    {
                        foreach (var z1 in z.TransferServiceBook)
                        {
                            string PayMonth = "";
                            string Month = "";

                            Month = "0" + DateTime.Now.Date.Month;
                            PayMonth = Month + "/" + DateTime.Now.Date.Year;

                            if (z.Employee.ServiceBookDates.ServiceLastDate == null && z.Employee.Login.IsUltimateHOAppl == true && z.Employee.Login.UserId.ToUpper() != "ADMIN" && z.Employee.Login != null && z1.ProcessTransDate.Value.ToString("MM/yyyy") == PayMonth && z1.ProcessTransDate.Value <= DateTime.Now)
                            {
                                model.Add(z.Employee);
                            }
                        }


                    }
                    else
                    {
                        if (z.Employee.ServiceBookDates.ServiceLastDate == null && z.Employee.Login == null)
                        {
                            model.Add(z.Employee);
                        }
                    }


                }

                data = model;

                List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                if (data != null && data.Count != 0)
                {
                    foreach (var item in data.Distinct().OrderBy(e => e.EmpCode))
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
                        tablename = "Employee-Table"
                    };
                    return Json(new List<Object> { returnjson }, JsonRequestBehavior.AllowGet);
                }

                else
                {
                    return Json(new Utility.JsonReturnClass { success = false, responseText = "No Record Found...!", data = "Employee-Table" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }



        public class returnDataClass
        {

            public bool IsPresent { get; set; }
            public bool IsCancelled { get; set; }
            public string CancelReason { get; set; }
            public string Batchname { get; set; }
        }


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
        public async Task<ActionResult> Create(Login c, FormCollection form) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                var Emplist = form["Employee-Table"] == null ? "" : form["Employee-Table"];
                var IsESSAppl = form["IsESSAppl"];
                var IsUltimateppl = form["IsUltimateppl"];
                bool Auth = form["Autho_Allow"] == "true" ? true : false;



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


                var CompId = int.Parse(SessionManager.CompanyId);
                var EmpPayAll = db.CompanyPayroll
                    .Include(e => e.Company)
                    .Include(e => e.EmployeePayroll)
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee))
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee.Login))
                    .Where(e => e.Company.Id == CompId)
                    .FirstOrDefault();

                var EmpAll = EmpPayAll.EmployeePayroll.Select(e => e.Employee).ToList();


                if (Auth == false)
                {
                    if (ModelState.IsValid)
                    {
                        try
                        {
                            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                            {

                                if (IsESSAppl == "true")
                                {

                                    int sid = 0;
                                    string sfull = null;
                                    foreach (var a in EmpId)
                                    {

                                        var Emps = EmpAll.Where(e => e.Id == a && e.Login == null).Select(e => new
                                        {
                                            _Id = e.Id,
                                            _EmpCode = e.EmpCode
                                        }).FirstOrDefault();

                                        if (Emps != null)
                                        {


                                            var newLogin = new Login();
                                            newLogin.UserId = Emps._EmpCode;
                                            newLogin.Password = Emps._EmpCode;
                                            newLogin.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                          //  newLogin.IsUltimateAppl = true;
                                            newLogin.IsUltimateHOAppl = false;
                                            newLogin.IsActive = true;
                                            db.Login.Add(newLogin);
                                            db.SaveChanges();

                                            var emp = db.Employee.Include(e => e.Login).Where(e => e.Id == Emps._Id).SingleOrDefault();
                                            emp.Login = newLogin;

                                            db.Entry(emp).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();
                                        }
                                    }
                                    ts.Complete();
                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = sid, Val = sfull, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }
                                if (IsUltimateppl == "false")
                                {
                                    int sid = 0;
                                    string sfull = null;
                                    foreach (var a in EmpId)
                                    {
                                        var chkemp = EmpAll.Where(e => e.Id == a).FirstOrDefault();
                                        if (chkemp.Login != null)
                                        {

                                            int Emps = EmpAll.Where(e => e.Id == a).FirstOrDefault().Login.Id;
                                            Login Logint = db.Login.Find(Emps);
                                            //Logint.IsUltimateAppl = true;
                                            Logint.IsUltimateHOAppl = false;
                                            Logint.IsActive = true;
                                            db.Login.Attach(Logint);
                                            db.Entry(Logint).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();
                                            db.Entry(Logint).State = System.Data.Entity.EntityState.Detached;
                                        }

                                    }
                                    ts.Complete();
                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = sid, Val = sfull, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }
                                if (IsUltimateppl == "true")
                                {
                                    Msg.Add("To make Ultimate Applicable Please Go to menu User Accountablity");
                                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }
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

                return View();
            }

        }

        public ActionResult Get_Login(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var compid = Convert.ToInt32(Session["CompId"].ToString());
                    var ab = db.CompanyPayroll.Where(e => e.Company.Id == compid)
                     .Include(e => e.EmployeePayroll)
                     .Include(e => e.EmployeePayroll.Select(a => a.Employee))
                     .Include(e => e.EmployeePayroll.Select(a => a.Employee.EmpName))
                     .Include(e => e.EmployeePayroll.Select(a => a.Employee.Login))
                     .Include(e => e.EmployeePayroll.Select(a => a.Employee.ServiceBookDates))

                     .AsNoTracking().AsParallel()
                     .SingleOrDefault();

                    var db_data = ab.EmployeePayroll.Where(e => e.Employee.Id == data).Select(e => e.Employee).SingleOrDefault();



                    if (db_data != null)
                    {
                        List<LoanAdvReqChildDataClass> returndata = new List<LoanAdvReqChildDataClass>();



                        if (db_data.ServiceBookDates.ServiceLastDate == null && db_data.Login != null)
                        {

                            returndata.Add(new LoanAdvReqChildDataClass
                            {
                                Id = db_data.Id,
                                UserId = db_data.Login.UserId,
                                IsUltimateAppl = db_data.Login.IsUltimateHOAppl.ToString(),
                                IsActive = db_data.Login.IsActive.ToString(),

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

        //public ActionResult EditSave(String forwarddata, String PayMonth) // Edit submit
        //{
        //    List<string> Msg = new List<string>();
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        try
        //        {
        //            var serialize = new JavaScriptSerializer();

        //            var obj = serialize.Deserialize<List<DeserializeClass>>(forwarddata);

        //            if (obj.Count < 0)
        //            {
        //                return Json(new { sucess = true, responseText = "You have to change reason to update record." }, JsonRequestBehavior.AllowGet);
        //            }
        //            List<int> ids = obj.Select(e => int.Parse(e.Id)).ToList();

        //            using (TransactionScope ts = new TransactionScope())
        //            {
        //                try
        //                {
        //                    foreach (int ca in ids)
        //                    {
        //                        TrainigDetailSessionInfo DetSessionInfo = db.TrainigDetailSessionInfo.Find(ca);
        //                        DetSessionInfo.CancelReason = Convert.ToString(obj.Where(e => e.Id == ca.ToString()).Select(e => e.CancelReason).Single());



        //                        db.TrainigDetailSessionInfo.Attach(DetSessionInfo);
        //                        db.Entry(DetSessionInfo).State = System.Data.Entity.EntityState.Modified;
        //                        db.SaveChanges();
        //                        db.Entry(DetSessionInfo).State = System.Data.Entity.EntityState.Detached;
        //                    }
        //                }
        //                catch (Exception ex)
        //                {
        //                    //List<string> Msg = new List<string>();
        //                    Msg.Add(ex.Message);
        //                    LogFile Logfile = new LogFile();
        //                    ErrorLog Err = new ErrorLog()
        //                    {
        //                        ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
        //                        ExceptionMessage = ex.Message,
        //                        ExceptionStackTrace = ex.StackTrace,
        //                        LineNo = Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
        //                        LogTime = DateTime.Now
        //                    };
        //                    Logfile.CreateLogFile(Err);
        //                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                }

        //                ts.Complete();
        //                return Json(new Object[] { "", "", "Reason Updated Successfully." }, JsonRequestBehavior.AllowGet);

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
        public class P2BGridData
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
        }

        public class LoanAdvReqChildDataClass //childgrid
        {
            public int Id { get; set; }
            public string UserId { get; set; }
            public string IsUltimateAppl { get; set; }
            public string IsActive { get; set; }

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

                try
                {
                    var compid = Convert.ToInt32(Session["CompId"].ToString());
                    var FilterEmployee = db.Employee
                      .Include(e => e.Login)
                      .Include(e => e.EmpName).AsNoTracking().AsParallel()
                      .ToList();


                    var all = FilterEmployee;
                    //for searchs
                    IEnumerable<Employee> fall;
                    if (param.sSearch == null)
                    {
                        fall = all;
                    }
                    else
                    {
                        fall = all.Where(e => e.EmpCode.ToUpper() == param.sSearch.ToUpper()).ToList();
                        fall = all.Where(e => (e.Id.ToString().Contains(param.sSearch))
                                  || (e.EmpCode.ToString().Contains(param.sSearch))
                                  || (e.EmpName.FullNameFML.ToString().ToUpper().Contains(param.sSearch.ToUpper()))
                                  ).ToList();
                    }

                    //for column sorting
                    var sortindex = Convert.ToInt32(Request["iSortCol_0"]);

                    Func<Employee, string> orderfunc = (c =>
                                                                Convert.ToUInt32(sortindex) == 0 ? c.Id.ToString() :
                                                                sortindex == 1 ? c.EmpCode : "");
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
                                 EmpCode = item.EmpCode,
                                 EmpName = item.EmpName.FullNameFML,

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

                                     select new[] { null, Convert.ToString(c.Id), c.EmpCode };
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
                                //Id = item.Id.ToString(),
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