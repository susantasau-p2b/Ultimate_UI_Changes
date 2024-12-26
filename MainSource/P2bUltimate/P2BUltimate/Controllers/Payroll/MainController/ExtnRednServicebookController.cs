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
using Payroll;
using P2BUltimate.Process;
using System.Diagnostics;
using P2BUltimate.Security;

namespace P2BUltimate.Controllers.Payroll.MainController
{
    public class ExtnRednServicebookController : Controller
    {
        //
        // GET: /ExtnRednServicebook/
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/ExtnRednServiceBook/Index.cshtml");
        }

        public ActionResult PopulateDropDownActivityList(string data, string data2)//modified by prashant 15042017
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //var qurey = db.IncrActivity.Include(e => e.IncrList).ToList();
                int OEmp_Id = Convert.ToInt32(data2);
                var query = db.EmployeePolicyStruct.AsNoTracking().Where(e => e.EmployeePayroll.Employee.Id == OEmp_Id && e.EndDate == null).Include(e => e.EmployeePayroll).Include(e => e.EmployeePayroll.Employee).Include(e => e.EmployeePolicyStructDetails)
                    .Include(e => e.EmployeePolicyStructDetails.Select(r => r.PolicyFormula)).Include(e => e.EmployeePolicyStructDetails.
                    Select(r => r.PolicyFormula.ExtnRednActivity)).FirstOrDefault();
                var ExtnRednActList = query.EmployeePolicyStructDetails.Select(r => r.PolicyFormula.ExtnRednActivity);

                var selected = (Object)null;
                //if (data2 != "" && data != "0" && data2 != "0")
                //{
                //    selected = Convert.ToInt32(data2);
                //}

                List<ExtnRednActivity> ExtnRednAct = new List<ExtnRednActivity>();
                if (ExtnRednActList.Count() > 0)
                {
                    foreach (var item in ExtnRednActList)
                    {
                        if (item.FirstOrDefault() != null)
                        {
                            foreach (var item1 in item)
                            {
                                ExtnRednAct.Add(item1);
                            }
                        }

                    }
                }
                SelectList s = new SelectList(ExtnRednAct, "Id", "Name", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }

        public class ExtnRednData
        {
            public string Period { get; set; }
            public string UnitValue { get; set; }
            public string ExpiryDate { get; set; }
            public string UtilizedTillDate { get; set; }
            public string ProcessDate { get; set; }
            public string ProcessActDate { get; set;}
            public string ProbationPeriod { get; set; }
        }

        public ActionResult GetApplicableData(string data, string data2)
        {
            //data-selected
            //data2-empid
            using (DataBaseContext db = new DataBaseContext())
            {
                if ((data2 != null && data2 != "0") && (data != null && data != "0"))
                {
                    var Empid = Convert.ToInt32(data);
                    var Policyid = Convert.ToInt32(data2);
                    var query = db.ExtnRednPolicy.Include(e => e.ExtnRednPeriodUnit).Include(e => e.ExtnRednCauseType).Where(e => e.Id == Policyid).FirstOrDefault();
                    int ExtnRednCount = db.ExtnRednServiceBook.Include(e => e.EmployeePayroll.Employee)
                        .Where(e => e.EmployeePayroll.Employee.Id == Empid).OrderByDescending(e => e.Id).FirstOrDefault() != null ? db.ExtnRednServiceBook.Include(e => e.EmployeePayroll.Employee).Where(e => e.EmployeePayroll.Employee.Id == Empid).OrderByDescending(e => e.Id).FirstOrDefault().ExtnRednCount : 0;
                    ServiceBookDates OSrBkDates = db.Employee.Include(e => e.ServiceBookDates).Where(e => e.Id == Empid).FirstOrDefault().ServiceBookDates;
                    ExtnRednData OExtnData = new ExtnRednData();
                    DateTime? ExpDate = null;
                    DateTime? ProcActDate = null;
                    string Period = null;
                    string ProbationPeriod = null; 
                    if (query != null)
                    {
                        if (query.ExtnRednCauseType.LookupVal.ToUpper().Contains("CONTRACTPERIOD") == true)
                        {
                            ProcActDate = OSrBkDates.RetirementDate;
                        }
                        if (query.ExtnRednCauseType.LookupVal.ToUpper().Contains("PROBATIONPERIOD") == true)
                        {
                            ProcActDate = OSrBkDates.ProbationDate;
                            ProbationPeriod = OSrBkDates.ProbationPeriod.ToString();
                        }
                        if (query.ExtnRednCauseType.LookupVal.ToUpper().Contains("RETIREMENTPERIOD") == true)
                        {
                            ProcActDate = OSrBkDates.RetirementDate;
                        }
                        if (query.ExtnRednCauseType.LookupVal.ToUpper().Contains("TRAINEEPERIOD") == true)
                        {
                             
                        }

                        if (query.ExtnRednPeriodUnit.LookupVal.ToUpper() == "DAYS")
                            ExpDate = ProcActDate.Value.AddDays(query.ExtnRednPeriod);
                        else if (query.ExtnRednPeriodUnit.LookupVal.ToUpper() == "MONTHS")  
                            ExpDate = ProcActDate.Value.AddMonths(query.ExtnRednPeriod);
                        else if (query.ExtnRednPeriodUnit.LookupVal.ToUpper() == "YEARS")
                            ExpDate = ProcActDate.Value.AddYears(query.ExtnRednPeriod);
                    }

                    OExtnData = new ExtnRednData()
                    {
                        ExpiryDate = ExpDate.Value.ToString("dd/MM/yyyy"),
                        Period = query.ExtnRednPeriod.ToString(),
                        UnitValue = query.ExtnRednPeriodUnit.LookupVal.ToUpper(),
                        UtilizedTillDate = ExtnRednCount.ToString(),
                        ProcessActDate = ProcActDate.Value.ToString("dd/MM/yyyy"),
                        ProcessDate = DateTime.Now.ToString("dd/MM/yyyy"),
                        ProbationPeriod = ProbationPeriod
                    };

                    return Json(OExtnData, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
              
            }
        }

        #region Create
        public ActionResult Create(ExtnRednServiceBook ExtnRednServiceBook, FormCollection form, String forwarddata) //Create submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var Emp = form["Employee-Table"] == "0" ? "" : form["Employee-Table"];

                    string ExtnRednActivity = form["ExtnRednActivitylist"] == "0" ? "" : form["ExtnRednActivitylist"];
                    string ExtnRednPolicy = form["extnrednpolicy"] == "0" ? "" : form["extnrednpolicy"];
                    string ProcessDate = form["ProcessDate"] == "0" ? "" : form["ProcessDate"];
                    string PeriodUnit = form["PeriodUnit"] == "0" ? "" : form["PeriodUnit"];
                  //  string ExpiryDate = form["PeriodUnit"] == "0" ? "" : form["PeriodUnit"];

                    var date = Convert.ToDateTime(ProcessDate).ToString("MM/yyyy");

                    int CompId = 0;
                    if (Session["CompId"] != null)
                    {
                        CompId = Convert.ToInt32(Session["CompId"]);
                    }

                    int id = 0;
                    if (Emp != null)
                    {
                        id = Convert.ToInt32(Emp.Replace(",","").ToString());
                    }
                    else
                    {
                        Msg.Add("Kindly select employee  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        // return Json(new { success = false, responseText = "Kindly select employee" }, JsonRequestBehavior.AllowGet);
                    }

                    ////////new 21/08/2019
                    //var check = db.CPIEntryT.Where(e => e.PayMonth == date).ToList();

                    //if (check.Count() == 0)
                    //{
                    //    Msg.Add("Kindly run CPI first and then try again");
                    //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //}
                    ///////////

                    Employee OEmployee = null;
                    EmployeePayroll OEmployeePayroll = null;


                    OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct).Include(e => e.PayStruct.JobStatus)
                                    .Include(e => e.EmpOffInfo)
                                    .Where(r => r.Id == id).SingleOrDefault();

                    OEmployeePayroll = db.EmployeePayroll.Include(e => e.ExtnRednServiceBook).Where(e => e.Employee.Id == id).SingleOrDefault();
                    if (OEmployeePayroll.ExtnRednServiceBook.Any(a => a.ProcessDate.Value.ToShortDateString() == ProcessDate.ToString()))
                    {
                        Msg.Add("Already Policy for Date= " + ProcessDate);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    int ExtnRednActivityPolicyId = 0;
                    int ExtnRednActivityId = 0;
                    string LookupVal = "";
                    int ExtnRednMaxCount = 0;
                    if (ExtnRednActivity != null && ExtnRednActivity != "")
                    {
                        ExtnRednActivityId = int.Parse(ExtnRednActivity);
                        LookupVal = db.LookupValue.Where(e => e.Id == ExtnRednActivityId).Select(e => e.LookupVal.ToUpper()).SingleOrDefault();
                        ExtnRednServiceBook.ExtnRednActivity = db.ExtnRednActivity.Include(e => e.ExtnRednPolicy).Where(e => e.Id == ExtnRednActivityId).SingleOrDefault();
                        ExtnRednMaxCount = ExtnRednServiceBook.ExtnRednActivity.ExtnRednPolicy.MaxCount;
                    }

                    if (ExtnRednPolicy != null && ExtnRednPolicy != "")
                    {
                        ExtnRednActivityPolicyId = int.Parse(ExtnRednPolicy);
                        //TransferServiceBook.TransActivity = db.TransActivity.Include(e => e.TranPolicy).Where(e => e.Id == TransActivityPolicyId).SingleOrDefault();
                    }

                    if (ExtnRednActivity != null && ExtnRednActivity != "")
                    {
                        ExtnRednActivityId = int.Parse(ExtnRednActivity);
                        LookupVal = db.LookupValue.Where(e => e.Id == ExtnRednActivityId).Select(e => e.LookupVal.ToUpper()).SingleOrDefault();
                        ExtnRednServiceBook.ExtnRednActivity = db.ExtnRednActivity.Include(e => e.ExtnRednPolicy).Where(e => e.Id == ExtnRednActivityId).SingleOrDefault(); 
                    }

                    if (ExtnRednServiceBook.ExtnRednActivity != null)
                    {
                        int curntCount = 0;
                        if (db.ExtnRednServiceBook.Where(e => e.EmployeePayroll.Employee.Id == id && e.ReleaseFlag == true).OrderByDescending(e => e.Id).FirstOrDefault() != null)
                        {
                            curntCount = db.ExtnRednServiceBook.Where(e => e.EmployeePayroll.Employee.Id == id && e.ReleaseFlag == true).OrderByDescending(e => e.Id).FirstOrDefault().ExtnRednCount + 1;
                        }
                        if (curntCount > ExtnRednMaxCount)
                        {
                            Msg.Add("You cannot add this record as count is reached.");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }

                    if (PeriodUnit != null)
                        ExtnRednServiceBook.Frequency = db.Lookup.Include(e=>e.LookupValues).Where(e => e.Code == "3008").FirstOrDefault().LookupValues.Where(e => e.LookupVal.ToUpper() == PeriodUnit.ToUpper()).FirstOrDefault(); //db.LookupValue.Where(e => e.LookupVal.ToUpper() == PeriodUnit.ToUpper()).FirstOrDefault();
                  

                    if (OEmployee.PayStruct != null)
                        ExtnRednServiceBook.PayStruct = db.PayStruct.Find(OEmployee.PayStruct.Id);

                    if (OEmployee.GeoStruct != null)
                        ExtnRednServiceBook.GeoStruct = db.GeoStruct.Find(OEmployee.GeoStruct.Id);

                    if (OEmployee.FuncStruct != null)
                        ExtnRednServiceBook.FuncStruct = db.FuncStruct.Find(OEmployee.FuncStruct.Id);

                    
              

                    using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                      new System.TimeSpan(0, 30, 0)))
                    {
                        ExtnRednServiceBook.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                        ExtnRednServiceBook ExtnRednServBook = new ExtnRednServiceBook()
                        {
                            GeoStruct = ExtnRednServiceBook.GeoStruct,
                            FuncStruct = ExtnRednServiceBook.FuncStruct,
                            PayStruct = ExtnRednServiceBook.PayStruct,
                            ProcessDate = ExtnRednServiceBook.ProcessDate,
                            ExtnRednActivity = ExtnRednServiceBook.ExtnRednActivity,
                            Narration = ExtnRednServiceBook.Narration,
                            DBTrack = ExtnRednServiceBook.DBTrack,
                            ExtnRednCount = 0,
                            Period = ExtnRednServiceBook.Period,
                            Frequency = ExtnRednServiceBook.Frequency,
                            ExpiryDate = ExtnRednServiceBook.ExpiryDate,
                        };


                        db.ExtnRednServiceBook.Add(ExtnRednServBook);
                        db.SaveChanges();


                        List<ExtnRednServiceBook> ExtnRednBkList = new List<ExtnRednServiceBook>();
                        ExtnRednBkList.AddRange(OEmployeePayroll.ExtnRednServiceBook);
                        ExtnRednBkList.Add(ExtnRednServBook);
                        OEmployeePayroll.ExtnRednServiceBook = ExtnRednBkList;
                        db.EmployeePayroll.Attach(OEmployeePayroll);
                        db.Entry(OEmployeePayroll).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        db.Entry(OEmployeePayroll).State = System.Data.Entity.EntityState.Detached;

                        //  DbContextExtensions.RefreshEntites<EmployeePayroll>(db, RefreshMode.StoreWins);
                        Process.ServiceBook.ServiceBookProcess("",CompId, "EXTNREDN_PROCESS", null, null, ExtnRednServBook.Id, null, OEmployeePayroll.Id, "ExtnRedn", ExtnRednServBook.ProcessDate, false, false, 0, null);

                        // db.RefreshAllEntites(RefreshMode.StoreWins);
                        List<string> Msgs = new List<string>();
                        Msgs.Add("Data Saved successfully");
  
                        ts.Complete();

                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msgs }, JsonRequestBehavior.AllowGet);
                        //return Json(new Object[] { "", "", "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);

                    }
                }
                catch (Exception ex)
                {
                    // DbContextExtensions.RefreshEntites<EmployeePayroll>(db, RefreshMode.StoreWins);
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

                    Msg.Add(ex.Message);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //return Json(new Object[] { "", "", Msg }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        #endregion


        #region Release
        public ActionResult Release(ExtnRednServiceBook ExtnRednServiceBook, FormCollection form, String forwarddata) //Create submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    //  var Emp = form["emp_Id"] == "0" ? "" : form["emp_Id"];
                    var ReleaseFlag = form["ReleaseFlag"] == null ? false : Convert.ToBoolean(form["ReleaseFlag"]);
                    var ReleaseDatestr = form["ReleaseDate"] == null ? null : form["ReleaseDate"];
                    var txtNarrationRelease = form["txtNarrationRelease"] == null ? null : form["txtNarrationRelease"];

                    int CompId = 1;
                    if (!String.IsNullOrEmpty(SessionManager.UserName))
                    {
                        //CompId = int.Parse(SessionManager.UserName.ToString());
                        CompId = Convert.ToInt32(Session["CompId"]);
                    }

                    //int Empid = 0;
                    //if (Emp != null)
                    //{
                    //    Empid = int.Parse(Emp);
                    //}
                    List<int> Empids = null;
                    //if (Emp != null && Emp != "0" && Emp != "false")
                    //{
                    //    Empids = Utility.StringIdsToListIds(Emp);
                    //}
                    if (forwarddata != null && forwarddata != "0" && forwarddata != "false")
                    {
                        Empids = Utility.StringIdsToListIds(forwarddata);
                    }
                    foreach (var Empid in Empids)
                    {


                        Employee OEmployee = null;
                        EmployeePayroll OEmployeePayroll = null;
                        //int PayScaleAgrId = int.Parse(PayScaleAgr);
                        //var OPayScalAgreement = db.PayScaleAgreement.Where(e => e.Id == PayScaleAgrId).SingleOrDefault();

                        OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct).Include(e => e.PayStruct.JobStatus)
                                        .Include(e => e.EmpOffInfo)
                                        .Where(r => r.Id == Empid).SingleOrDefault();

                        OEmployeePayroll
                   = db.EmployeePayroll.Include(e => e.ExtnRednServiceBook)
                      .Where(e => e.Employee.Id == Empid).SingleOrDefault();
                        //  int TransId = int.Parse(forwarddata);
                        //List<int> ids = null;
                        //if (forwarddata != null && forwarddata != "0" && forwarddata != "false")
                        //{
                        //    ids = Utility.StringIdsToListIds(forwarddata);
                        //}

                        //foreach (int item in ids)
                        //{
                        int ExtnRednId = OEmployeePayroll.ExtnRednServiceBook.Where(e => e.ReleaseFlag == false).SingleOrDefault().Id;

                        ExtnRednServiceBook.ReleaseFlag = Convert.ToBoolean(ReleaseFlag);
                        ExtnRednServiceBook.Narration = txtNarrationRelease;
                        //TransferServiceBook.ReleaseDate = Convert.ToDateTime(ReleaseDatestr);
                        using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                          new System.TimeSpan(0, 30, 0)))
                        {


                            if (ReleaseFlag != false)
                            {
                                ExtnRednServiceBook OExtnRednServiceBook = db.ExtnRednServiceBook.Where(e => e.Id == ExtnRednId).SingleOrDefault();
                                Process.ServiceBook.ServiceBookProcess("",CompId, "EXTNREDN_RELEASE", null, null, null, null, OEmployeePayroll.Id, "EXTNREDN", ExtnRednServiceBook.ReleaseDate, false, false, 0, OExtnRednServiceBook.Id);
                            }

                            ts.Complete();

                           

                        }
                     
                    }
                    Msg.Add("  Data Updated successfully  ");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                }
                catch (Exception ex)
                {
                    // DbContextExtensions.RefreshEntites<EmployeePayroll>(db, RefreshMode.StoreWins);
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

                    Msg.Add(ex.Message);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        #endregion


        public JsonResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.ExtnRednServiceBook
                    .Include(e => e.ExtnRednActivity.ExtnRednPolicy) 
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        ExtnRednActivity = e.ExtnRednActivity.ExtnRednList.LookupVal,
                        ProcessDate = e.ProcessDate, 
                        Narration = e.Narration
                    }).ToList();

                var ExtnRednServBook = db.ExtnRednServiceBook.Find(data);
                Session["RowVersion"] = ExtnRednServBook.RowVersion;
                var Auth = ExtnRednServBook.DBTrack.IsModified;
                return Json(new Object[] { Q, Auth }, JsonRequestBehavior.AllowGet);
            }
        }


        public class ExtnRednServBookGridData
        {
            public int Id { get; set; }
            public int EmpId { get; set; }
            public Employee Employee { get; set; }
            public ExtnRednActivity ExtnRednActivity { get; set; }
            public string ActivityDate { get; set; }
            public string Unit { get; set; }
            public string Period { get; set; }
            public string Count { get; set; }
        }

        public class returndatagridclass
        {
            public string Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
            public string JoiningDate { get; set; }
            public string Job { get; set; }
            public string Grade { get; set; }
            public string Location { get; set; }
        }

        public class ExtnRednChildDataClass
        {
            public int Id { get; set; }
            public bool Release { get; set; }
            public string ReleaseDate { get; set; }
            public string Activity { get; set; }
            public string ProcessDate { get; set; }
            public string Unit { get; set; }
            public string Period { get; set; }
            public string Count { get; set; }
        }


        public ActionResult P2BGridRelease(P2BGrid_Parameters gp)
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

                IEnumerable<ExtnRednServBookGridData> TransServBook = null;
                List<ExtnRednServBookGridData> model = new List<ExtnRednServBookGridData>();
                ExtnRednServBookGridData view = null;

                var OEmployee = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.Employee.EmpName).ToList();

                foreach (var z in OEmployee)
                {
                    var ObjExtnRednServBook = db.EmployeePayroll.Where(e => e.Id == z.Id)
                        .Select(e => e.ExtnRednServiceBook.Where(r => r.ReleaseFlag == false))
                                        .SingleOrDefault();


                    //DateTime? Eff_Date = null;
                    //PayScaleAgreement PayScaleAgr = null;
                    foreach (var a in ObjExtnRednServBook)
                    {
                        //Eff_Date = Convert.ToDateTime(a.ProcessMonth);
                        var aa = db.ExtnRednServiceBook
                            .Include(e => e.ExtnRednActivity).Include(e => e.ExtnRednActivity.ExtnRednList).Include(e => e.Frequency)
                            .Where(e => e.Id == a.Id).SingleOrDefault();
                        view = new ExtnRednServBookGridData()
                        {
                            Id = z.Employee.Id,
                            Employee = z.Employee,
                            ExtnRednActivity = aa.ExtnRednActivity,
                            ActivityDate = aa.ProcessDate.Value.ToString("dd/MM/yyyy"), 
                            Unit = aa.Frequency.LookupVal.ToString(),
                            Period = aa.Period.ToString(),
                            Count = aa.ExtnRednCount.ToString()
                        };

                        model.Add(view);
                    }

                }

                TransServBook = model;

                IEnumerable<ExtnRednServBookGridData> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = TransServBook;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.Employee.EmpCode.ToString().Contains(gp.searchString))
                            || (e.Employee.EmpName.FullNameFML.ToString().Contains(gp.searchString))
                            || (e.ExtnRednActivity.ExtnRednList.LookupVal.ToString().Contains(gp.searchString))
                            || (e.ActivityDate.ToString().Contains(gp.searchString))
                            || (e.Period.ToString().Contains(gp.searchString))
                            || (e.Unit.ToString().Contains(gp.searchString))
                            || (e.Count.ToString().Contains(gp.searchString))
                            || (e.Id.ToString().Contains(gp.searchString))
                            ).Select(a => new Object[] { a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.ExtnRednActivity.ExtnRednList.LookupVal, a.ActivityDate, a.Period, a.Unit, a.Count, a.Id }).ToList();
                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.ExtnRednActivity.ExtnRednList.LookupVal, a.ActivityDate, a.Period, a.Unit, a.Count, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = TransServBook;
                    Func<ExtnRednServBookGridData, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "EmpCode" ? c.Employee.EmpCode.ToString() :
                                         gp.sidx == "EmpName" ? c.Employee.EmpName.FullNameFML.ToString() :
                                         gp.sidx == "Activity" ? c.ExtnRednActivity.ExtnRednList.LookupVal :
                                         gp.sidx == "Process Date" ? c.ActivityDate.ToString() :
                                         gp.sidx == "Period" ? c.Period.ToString() :
                                         gp.sidx == "Unit" ? c.Unit.ToString() :
                                         gp.sidx == "Count" ? c.Count.ToString() : ""
                                          );
                    }
                    if (gp.sord == "asc")  //Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : ""
                    {
                        IE = IE.OrderBy(orderfuc);  // a.Id,a.Employeee.EmpCode,a.Employeee.EmpName, a.SalaryHead.Name, a.FromPeriod, a.ToPeriod, a.AmountPaid, a.OtherDeduction, a.ProcessMonth, a.TDSAmount, a.Narration
                        jsonData = IE.Select(a => new Object[] { a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.ExtnRednActivity.ExtnRednList != null ? a.ExtnRednActivity.ExtnRednList.LookupVal : "", a.ActivityDate, a.Period, a.Unit, a.Count, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.ExtnRednActivity.ExtnRednList != null ? a.ExtnRednActivity.ExtnRednList.LookupVal : "", a.ActivityDate, a.Period, a.Unit, a.Count, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.ExtnRednActivity.ExtnRednList != null ? a.ExtnRednActivity.ExtnRednList.LookupVal : "", a.ActivityDate, a.Period, a.Unit, a.Count, a.Id }).ToList();
                    }
                    totalRecords = TransServBook.Count();
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

        public ActionResult Emp_Grid(ParamModel param, string y)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var all = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.Employee.EmpName).Include(e => e.Employee.ServiceBookDates)
                        .Include(e => e.Employee.GeoStruct).Include(e => e.Employee.GeoStruct.Location.LocationObj).Include(e => e.Employee.FuncStruct)
                        .Include(e => e.Employee.FuncStruct.Job).Include(e => e.Employee.PayStruct).Include(e => e.Employee.PayStruct.Grade)
                        .Include(e => e.ExtnRednServiceBook).ToList();
                    // for searchs
                    IEnumerable<EmployeePayroll> fall;
                    if (param.sSearch == null)
                    {
                        fall = all;
                    }
                    else
                    {
                        fall = all.Where(e => e.Employee.EmpCode.ToUpper() == param.sSearch.ToUpper()).ToList();
                    }
                    //for column sorting
                    var sortindex = Convert.ToInt32(Request["iSortCol_0"]);

                    Func<EmployeePayroll, string> orderfunc = (c =>
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
                        foreach (var item in all)
                        {
                            result.Add(new returndatagridclass
                            {
                                Id = item.Id.ToString(),
                                Code = item.Employee.EmpCode,
                                Name = item.Employee.EmpName.FullNameFML,
                                JoiningDate = item.Employee.ServiceBookDates.JoiningDate.Value.ToString(),
                                Job = item.Employee.FuncStruct.Job.Name,
                                Grade = item.Employee.PayStruct.Grade.Name,
                                Location = item.Employee.GeoStruct.Location.LocationObj.LocDesc
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

        public ActionResult Get_ExtnRednServBook(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    

                    var db_data1 = db.EmployeePayroll
                       .Include(e => e.ExtnRednServiceBook.Select(r => r.ExtnRednActivity))
                       .Include(e => e.ExtnRednServiceBook.Select(r => r.ExtnRednActivity.ExtnRednList))
                       .Include(e => e.ExtnRednServiceBook.Select(r => r.Frequency))
                       .Where(e => e.Id == data).SingleOrDefault();

                    if (db_data1 != null)
                    {
                        List<ExtnRednChildDataClass> returndata = new List<ExtnRednChildDataClass>();
                        foreach (var item in db_data1.ExtnRednServiceBook)
                        {
                            returndata.Add(new ExtnRednChildDataClass
                            {
                                Id = item.Id,
                                Release = item.ReleaseFlag,
                                ReleaseDate = item.ReleaseDate != null ? item.ReleaseDate.Value.ToShortDateString() : null,
                                Activity = item.ExtnRednActivity != null ? item.ExtnRednActivity.ExtnRednList.LookupVal : null,
                                ProcessDate = item.ProcessDate != null ? item.ProcessDate.Value.ToShortDateString() : null,
                                Count = item.ExtnRednCount.ToString(),
                                Period = item.Period.ToString(),
                                Unit = item.Frequency.LookupVal.ToString()
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
        public ActionResult GridDelete(int data)
        {
            List<string> Msg = new List<string>();

            try
            {
                using (DataBaseContext db = new DataBaseContext())
                {
                    ExtnRednServiceBook ExtnRednServBook = db.ExtnRednServiceBook.Where(e => e.Id == data).SingleOrDefault();

                    if (ExtnRednServBook.ReleaseFlag == true)
                    {
                        return this.Json(new { status = true, valid = true, responseText = "You cannot delete as activity is already released.", JsonRequestBehavior.AllowGet });
                    }

                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        try
                        {
                            db.ExtnRednServiceBook.Attach(ExtnRednServBook);
                            db.Entry(ExtnRednServBook).State = System.Data.Entity.EntityState.Deleted;
                            db.SaveChanges();
                            db.Entry(ExtnRednServBook).State = System.Data.Entity.EntityState.Detached;
                            ts.Complete();
                            return this.Json(new { status = true, responseText = "Data removed successfully.", JsonRequestBehavior.AllowGet });
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
                            // List<string> Msg = new List<string>();
                            Msg.Add(ex.Message);
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
}