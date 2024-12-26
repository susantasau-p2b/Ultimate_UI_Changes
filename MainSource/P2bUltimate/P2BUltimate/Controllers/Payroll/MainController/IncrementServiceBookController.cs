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
using Newtonsoft.Json;
using P2BUltimate.Process;
using System.Diagnostics;
using P2BUltimate.Security;
namespace P2BUltimate.Controllers.Payroll.MainController
{
    [AuthoriseManger]
    public class IncrementServiceBookController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/IncrementServiceBook/Index.cshtml");
        }

        public ActionResult PopulateDropDownActivityList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //var qurey = db.IncrActivity.Include(e => e.IncrList).ToList();
                int OEmp_Id = Convert.ToInt32(data2);
                var query = db.EmployeePolicyStruct.AsNoTracking().Where(e => e.EmployeePayroll.Employee.Id == OEmp_Id && e.EndDate == null).Include(e => e.EmployeePayroll).Include(e => e.EmployeePayroll.Employee).Include(e => e.EmployeePolicyStructDetails)
                    .Include(e => e.EmployeePolicyStructDetails.Select(r => r.PolicyFormula)).Include(e => e.EmployeePolicyStructDetails.
                    Select(r => r.PolicyFormula.IncrActivity)).FirstOrDefault();
                var IncrActList = query.EmployeePolicyStructDetails.Select(r => r.PolicyFormula.IncrActivity);

                var selected = (Object)null;
                //if (data2 != "" && data != "0" && data2 != "0")
                //{
                //    selected = Convert.ToInt32(data2);
                //}

                List<IncrActivity> IncrAct = new List<IncrActivity>();
                if (IncrActList.Count() > 0)
                {
                    foreach (var item in IncrActList)
                    {
                        if (item.FirstOrDefault() != null)
                        {
                            foreach (var item1 in item)
                            {
                                IncrAct.Add(item1);
                            }
                        }

                    }
                }
                SelectList s = new SelectList(IncrAct, "Id", "Name", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetActivityPolicy()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var qurey = db.IncrActivity.Where(e => e.IncrList.LookupVal.ToUpper() == "REGULAR")
                    .Select(e => new { Name = e.Name, activity_id = e.Id, IncrPolicy_id = e.IncrPolicy.Id, IncrPolicy = e.IncrPolicy.Name }).SingleOrDefault();
                var yr = DateTime.Now.Year;
                var month = DateTime.Now.Month < 10 ? "0" + DateTime.Now.Month : DateTime.Now.Month.ToString();
                var date = "01/" + month + "/" + yr;
                var returndata = new
                {
                    activity = qurey.Name,
                    activity_id = qurey.activity_id,
                    IncrPolicy = qurey.IncrPolicy,
                    IncrPolicy_id = qurey.IncrPolicy_id,
                    date = date
                };
                return Json(returndata, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult getpolicy(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (data != null || data != "")
                {
                    var id = Convert.ToInt32(data);
                    var query = db.IncrActivity.Include(e => e.IncrList).Where(e => e.IncrList.Id == id).ToList();
                    var selected = "";
                    if (data2 != "")
                    {
                        selected = data2;
                    }
                    SelectList s = new SelectList(query, "Id", "Name", selected);
                    return Json(s, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return null;
                }
            }
        }

        //public ActionResult ValidateForm(FormCollection form)
        //{
        //    string IncrementActivity = form["IncrActivitylist"] == "0" ? "" : form["IncrActivitylist"];
        //    string IncrementPolicy = form["incrpolicy"] == "0" ? "" : form["incrpolicy"];
        //    string ProcessIncrDate = form["ProcessIncrDate"] == "0" ? "" : form["ProcessIncrDate"];
        //    int Emp = form["Employee-Table"] == "0" ? 0 : Convert.ToInt32(form["Employee-Table"]);
        //    List<string> Msg = new List<string>();


        //    if (IncrementActivity == null)
        //    {
        //        Msg.Add("Please Select Increment Activity");
        //    }
        //    if (IncrementPolicy == null)
        //    {
        //        Msg.Add("Please Select Increment Policy");
        //    }
        //    if (ProcessIncrDate == null)
        //    {
        //        Msg.Add("Please Enter Increment Date");
        //    }
        //    if (Emp == null)
        //    {
        //        Msg.Add("Please select employee");
        //    }

        //    if (Msg.Count > 0)
        //    {
        //        return Json(new { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //    }
        //    else
        //    {
        //        return Json(new { success = true }, JsonRequestBehavior.AllowGet);

        //    }

        //}

        #region Create
        public ActionResult Create(IncrementServiceBook IncrementServiceBook, FormCollection form, String forwarddata) //Create submit
        {
            List<string> Msg = new List<string>();
            try
            {
                using (DataBaseContext db = new DataBaseContext())
                {
                    try
                    {
                        //   var Emp = form["Employee-Table"] == "0" ? 0 : Convert.ToInt32(form["Employee-Table"]);
                        var Emp = form["Employee-Table"] == "0" ? "" : form["Employee-Table"];

                        string IncrementActivity = form["IncrActivitylist"] == "0" ? "" : form["IncrActivitylist"];
                        string IncrementPolicy = form["incrpolicy"] == "0" ? "" : form["incrpolicy"];
                        string ProcessIncrDate = form["ProcessIncrDate"] == "0" ? "" : form["ProcessIncrDate"];

                        var date = Convert.ToDateTime(ProcessIncrDate).ToString("MM/yyyy");

                        int CompId = 0;
                        if (SessionManager.UserName != null)
                        {
                            CompId = Convert.ToInt32(Session["CompId"]);
                        }

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

                        ////////new 21/08/2019
                        var check = db.CPIEntryT.Where(e => e.PayMonth == date).ToList();

                        if (check.Count() == 0)
                        {
                            Msg.Add("Kindly run CPI first and then try again");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                        ///////////

                        Employee OEmployee = null;
                        EmployeePayroll OEmployeePayroll = null;

                        List<string> MsgCheck = new List<string>();
                        foreach (var i in ids)
                        {
                            OEmployee = db.Employee.Include(q => q.EmpName).Where(r => r.Id == i).AsNoTracking().AsParallel().SingleOrDefault();
                            OEmployeePayroll = db.EmployeePayroll.Include(e => e.IncrementServiceBook).Include(e => e.IncrementServiceBook.Select(r => r.IncrActivity)).Where(e => e.Employee.Id == OEmployee.Id).AsNoTracking().AsParallel().SingleOrDefault();

                            //if (OEmployeePayroll.IncrementServiceBook.Any(d => d.ProcessIncrDate.Value.ToShortDateString() == ProcessIncrDate.ToString())
                            //{
                            //    {
                            //        MsgCheck.Add("Already increment done for employee " + OEmployee.EmpCode + " " + OEmployee.EmpName.FullNameFML + " on date= " + ProcessIncrDate);
                            //        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //    }
                            //}


                            if (OEmployeePayroll.IncrementServiceBook.Any(d => d.ProcessIncrDate.Value.ToShortDateString() == ProcessIncrDate.ToString() && d.ReleaseFlag == false))
                            {
                                {
                                    Msg.Add("Please Release The Activity and Try Again:" + OEmployee.EmpCode + " " + OEmployee.EmpName.FullNameFML + " on date= " + ProcessIncrDate);
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }
                            }

                            int IncrActivityIds = 0;

                            if (IncrementActivity != null && IncrementActivity != "")
                            {
                                IncrActivityIds = int.Parse(IncrementActivity);
                            }

                            if (OEmployeePayroll.IncrementServiceBook.Any(d => d.ProcessIncrDate.Value.ToShortDateString() == ProcessIncrDate.ToString() && d.IncrActivity_Id == IncrActivityIds))
                            {
                                {
                                    Msg.Add("Already increment for Date= " + ProcessIncrDate);
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }
                            }


                        }
                        if (MsgCheck.Count > 0)
                        {
                            return Json(new Utility.JsonReturnClass { success = false, responseText = MsgCheck }, JsonRequestBehavior.AllowGet);
                        }
                        foreach (var i in ids)
                        {
                            OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct).Include(e => e.PayStruct.JobStatus)
                                            .Include(e => e.EmpOffInfo)
                                            .Where(r => r.Id == i).SingleOrDefault();

                            OEmployeePayroll = db.EmployeePayroll.Include(e => e.IncrementServiceBook).Where(e => e.Employee.Id == i).SingleOrDefault();


                            //if (OEmployeePayroll.IncrementServiceBook.Any(d => d.ProcessIncrDate.Value.ToShortDateString() == ProcessIncrDate.ToString()))
                            //{
                            //    {
                            //        Msg.Add("Already increment for Date= " + ProcessIncrDate);
                            //        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //    }
                            //}


                            if (OEmployee.GeoStruct != null)
                                IncrementServiceBook.GeoStruct = db.GeoStruct.Find(OEmployee.GeoStruct.Id);

                            if (OEmployee.FuncStruct != null)
                                IncrementServiceBook.FuncStruct = db.FuncStruct.Find(OEmployee.FuncStruct.Id);

                            if (OEmployee.PayStruct != null)
                                IncrementServiceBook.PayStruct = db.PayStruct.Find(OEmployee.PayStruct.Id);

                            int IncrementPolicyId = 0;
                            int IncrActivityId = 0;
                            string LookupVal = "";
                            if (IncrementActivity != null && IncrementActivity != "")
                            {
                                IncrActivityId = int.Parse(IncrementActivity);
                                //LookupVal = db.LookupValue.Where(e => e.Id == IncrActivityId).Select(e => e.LookupVal.ToUpper()).SingleOrDefault();
                                LookupVal = db.IncrActivity.Where(e => e.Id == IncrActivityId).Select(e => e.IncrList.LookupVal.ToUpper()).SingleOrDefault();
                                IncrementServiceBook.IncrActivity = db.IncrActivity.Where(e => e.Id == IncrActivityId).SingleOrDefault();
                            }

                            if (IncrementPolicy != null && IncrementPolicy != "")
                            {
                                IncrementPolicyId = int.Parse(IncrementPolicy);
                            }


                            using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                              new System.TimeSpan(0, 30, 0)))
                            {
                                if (Session["CompId"] != null)
                                    IncrementServiceBook.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                                else
                                    IncrementServiceBook.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                                if (LookupVal.ToUpper() == "REGULAR")
                                {
                                    Process.ServiceBook.ServiceBookProcess(IncrementServiceBook.Narration, CompId, "INCREMENT_PROCESS", null, null, null, null, OEmployeePayroll.Id, LookupVal, IncrementServiceBook.ProcessIncrDate, false, true, 0, null);

                                }
                                else
                                {
                                    Process.ServiceBook.ServiceBookProcess(IncrementServiceBook.Narration, CompId, "INCREMENT_PROCESS", null, null, null, null, OEmployeePayroll.Id, LookupVal, IncrementServiceBook.ProcessIncrDate, false, false, 0, null);

                                }



                                ts.Complete();


                                // return Json(new Object[] { "", "", "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
                                //    List<string> Msgs = new List<string>();
                            }
                        }
                        Msg.Add("Data Saved successfully");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    catch (Exception ex)
                    {
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

                        //     List<string> Msg = new List<string>();
                        Msg.Add(ex.Message);

                        //return Json(new Object[] { "", "", Msg }, JsonRequestBehavior.AllowGet);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
        #endregion

        public class EditData
        {
            public int Id { get; set; }
            public SalaryHead SalaryHead { get; set; }
            public bool Editable { get; set; }
            public double Amount { get; set; }
            public string FormulaEditable { get; set; }
            public string FormulaType { get; set; }
        }

        public ActionResult GetNonstdData(string data)
        {
            string Msg = "";
            List<EditData> model = new List<EditData>();
            var view = new EditData();

            using (DataBaseContext db = new DataBaseContext())
            {
                bool EditAppl = true;
                string FormulaActive = "Y";
                string FormulaType = "";
                List<int> extraeditdata = one_ids(data);
                if (extraeditdata.Count() == 1)
                {
                    foreach (int i in extraeditdata)
                    {
                        var OEmployeeSalStruct = db.EmployeePayroll.Where(e => e.Id == i).Include(e => e.Employee).Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails))
                                                .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead)))
                                                .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.SalHeadOperationType)))
                                                .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.Frequency)))
                                                .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.Type)))
                                               .SingleOrDefault();

                        var OEmpSalStruct = OEmployeeSalStruct.EmpSalStruct.Where(e => e.EndDate == null).FirstOrDefault();



                        var OEmpSalStructDet = OEmpSalStruct.EmpSalStructDetails;
                        foreach (var SalForAppl in OEmpSalStructDet)
                        {
                            var m = db.EmpSalStructDetails.Include(e => e.SalaryHead).Include(e => e.SalHeadFormula).Include(e => e.SalHeadFormula.FormulaType).Where(e => e.Id == SalForAppl.Id).SingleOrDefault();


                            var SalHeadForm = m.SalHeadFormula; //SalaryHeadGenProcess.SalFormulaFinder(x, m.SalaryHead.Id);


                            if (SalHeadForm != null)
                            {
                                if (SalHeadForm.FormulaType.LookupVal.ToUpper() == "NONSTANDARDFORMULA" && SalForAppl.SalaryHead.SalHeadOperationType.LookupVal.ToString().ToUpper() != "BASIC")
                                {
                                    FormulaActive = "Y";
                                    EditAppl = true;
                                    FormulaType = "NONSTANDARDFORMULA";

                                    view = new EditData()
                                    {
                                        Id = SalForAppl.Id,
                                        SalaryHead = SalForAppl.SalaryHead,
                                        Amount = SalForAppl.Amount,
                                        Editable = EditAppl,
                                        FormulaEditable = FormulaActive,
                                        FormulaType = FormulaType
                                    };

                                    model.Add(view);
                                }
                            }
                        }
                    }
                }

                else
                {
                    //Msg = "Nonstandard heads are defined. So you can't select multiple employees.";
                    Msg = "Please note that in the case of multiple selections of employee releases, the nonstandard salary component value has to be changed manually...";
                }

            }

            var result = new { Sal = model, msg = Msg };

            return Json(result, JsonRequestBehavior.AllowGet);

        }

        public List<int> one_ids(string form)
        {
            string itsec_id = form;
            List<int> ids = itsec_id.Split(',').Select(e => int.Parse(e)).ToList();
            return (ids);
        }

        #region Release
        public ActionResult Release(IncrementServiceBook IncrementServiceBook, FormCollection form, String forwarddata, string param) //Create submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    var Activityid = form["Activity_Id"] == "0" ? "" : form["Activity_Id"];

                    //var Emp = form["emp_Id"] == "0" ? "" : form["emp_Id"];13/07/2023
                    var ReleaseFlag = form["ReleaseFlag"] == "0" ? "" : form["ReleaseFlag"];
                    var ExtendReleaseFlag = form["ExtendReleaseFlag"] == "0" ? "" : form["ExtendReleaseFlag"];
                    var extendreleaseflag = Convert.ToBoolean(ExtendReleaseFlag);
                    var chkrelease = Convert.ToBoolean(ReleaseFlag);
                    var checkHoldFlag = Convert.ToBoolean(param);
                    var idsSalHead = form["input_hidden_field"] != "" ? one_ids(form["input_hidden_field"]) : null;
                    var ReleaseDateStr = form["ReleaseDate"] == "0" ? "" : form["ReleaseDate"];
                    DateTime? ReleaseDate = !string.IsNullOrEmpty(ReleaseDateStr) ? (DateTime?)Convert.ToDateTime(ReleaseDateStr) : null;


                    if (checkHoldFlag == true)
                    {
                        if (chkrelease == true && extendreleaseflag == true)
                        {
                            Msg.Add("Release Flag and Extend Release Flag Both Are Not True");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    if (!extendreleaseflag)
                    {
                        var releasedatemonth = IncrementServiceBook.ReleaseDate.Value.Month.ToString("d2") + "/" + IncrementServiceBook.ReleaseDate.Value.Year;
                        var cpidata = db.CPIEntryT.Any(q => q.PayMonth == releasedatemonth);
                        if (cpidata == false)
                        {
                            Msg.Add("Process the CPI Index for Month " + releasedatemonth + " and then release the activity");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }

                        if (chkrelease == false)
                        {
                            Msg.Add(" Make A Release Flag True");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    int CompId = 0;
                    if (!String.IsNullOrEmpty(Session["CompId"].ToString()))
                    {
                        CompId = Convert.ToInt32(Session["CompId"]);
                    }
                    //13/07/2023
                    //List<int> ids = null;
                    //if (Emp != null && Emp != "0" && Emp != "false")
                    //{
                    //    ids = Utility.StringIdsToListIds(Emp);
                    //}
                    //else
                    //{
                    //    Msg.Add(" Kindly select employee  ");
                    //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //}13/07/2023
                    List<int> ids = null;
                    if (forwarddata != null && forwarddata != "0" && forwarddata != "false")
                    {
                        ids = Utility.StringIdsToListIds(forwarddata);
                    }
                    //  Employee OEmployee = null;
                    EmployeePayroll OEmployeePayroll = null;
                    //int PayScaleAgrId = int.Parse(PayScaleAgr);
                    //var OPayScalAgreement = db.PayScaleAgreement.Where(e => e.Id == PayScaleAgrId).SingleOrDefault();

                    int inccount = -1;

                    List<int> IncrIdList = null;
                    //if (forwarddata != null && forwarddata != "0" && forwarddata != "false")
                    //{
                    //    IncrIdList = Utility.StringIdsToListIds(forwarddata);
                    //}
                    if (Activityid != null && Activityid != "0" && Activityid != "false")
                    {
                        IncrIdList = Utility.StringIdsToListIds(Activityid);
                    }
                    foreach (var Empid in ids)
                    {
                        //OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct).Include(e => e.PayStruct.JobStatus)
                        //                .Include(e => e.EmpOffInfo)
                        //                .Where(r => r.Id == Empid).SingleOrDefault();

                        //lengthy
                        //int IncrId = 0;
                        //if (inccount == 0)
                        //{
                        //    IncrId = IncrIdList[0];
                        //}
                        //else
                        //{
                        //    inccount = inccount + 1;
                        //    IncrId = IncrIdList[inccount];
                        //}
                        //end

                        inccount = inccount + 1;
                        int IncrId = IncrIdList[inccount];
                        OEmployeePayroll = db.EmployeePayroll.Where(e => e.Employee.Id == Empid).AsNoTracking().SingleOrDefault();

                        IncrementServiceBook.ReleaseFlag = true;
                        IncrementServiceBook OIncrementServiceBook = null;
                        Employee OEmployee = null;
                        OEmployee = db.Employee
                                        .Where(r => r.Id == Empid).SingleOrDefault();
                        // IncrementServiceBook.ReleaseFlag = Convert.ToBoolean(ReleaseFlag);
                        using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, new System.TimeSpan(0, 30, 0)))
                        {
                            //if (IncrementServiceBook.ReleaseFlag == true)
                            //{

                          

                            if (extendreleaseflag == true && checkHoldFlag == true)
                            {
                                IncrementServiceBook OIncrementServiceBook1 = db.IncrementServiceBook.Where(e => e.Id == IncrId).SingleOrDefault();
                                OIncrementServiceBook1.ReleaseDate = IncrementServiceBook.ReleaseDate;
                                db.IncrementServiceBook.Attach(OIncrementServiceBook1);
                                db.Entry(OIncrementServiceBook1).State = System.Data.Entity.EntityState.Modified;
                                //await db.SaveChangesAsync();
                                db.SaveChanges();
                            }
                            else
                            {
                                OIncrementServiceBook = db.IncrementServiceBook.Where(e => e.Id == IncrId).SingleOrDefault();
                            }
                            if (ReleaseDate.Value.Date < OIncrementServiceBook.ProcessIncrDate.Value.Date)
                            {
                                Msg.Add(" The " + OEmployee.EmpCode + " has Release date should be greater than the process date.");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                            if (checkHoldFlag == true)
                            {

                                IncrementHoldReleaseDetails Oincrementholdreleasedetails = db.IncrementHoldReleaseDetails.Where(e => e.IncrementServiceBook.Id == IncrId).SingleOrDefault();
                                Oincrementholdreleasedetails.ReleaseDate = IncrementServiceBook.ReleaseDate;
                                Oincrementholdreleasedetails.ReleaseNarration = IncrementServiceBook.Narration;
                                db.IncrementHoldReleaseDetails.Attach(Oincrementholdreleasedetails);
                                db.Entry(Oincrementholdreleasedetails).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                            }

                            if (!extendreleaseflag)
                            {
                                Process.ServiceBook.ServiceBookProcess("", CompId, "INCREMENT_RELEASE", OIncrementServiceBook.Id, null, null, null, OEmployeePayroll.Id, "INCREMENT", IncrementServiceBook.ReleaseDate, false, false, 0, null);
                            }
                            
                            if (idsSalHead != null && idsSalHead.Count() > 0)
                            {
                                List<EmpSalStruct> OEmpSalStruct = db.EmpSalStruct.Include(e => e.EmpSalStructDetails).Include(e => e.EmpSalStructDetails.Select(r => r.SalaryHead)).Include(e => e.EmpSalStructDetails.Select(r => r.SalHeadFormula))
                            .Where(e => e.EmployeePayroll.Id == OEmployeePayroll.Id && e.EffectiveDate >= OIncrementServiceBook.ProcessIncrDate).ToList();
                      
                                if (OEmpSalStruct.Count() > 0)
                                {
                                    foreach (var item in OEmpSalStruct)
                                    {
                                        for (int i = 0; i < idsSalHead.Count(); i++)
                                        {
                                            int SalHeadId = idsSalHead[i];
                                            double amount = Convert.ToDouble(form["Amt" + i]);
                                            string FormulaEdit = form["F" + i];
                                            EmpSalStructDetails OEmpSalStructDet = item.EmpSalStructDetails.Where(e => e.SalaryHead.Id == SalHeadId).FirstOrDefault();


                                            if (FormulaEdit == "N")
                                            {
                                                OEmpSalStructDet.Amount = amount;
                                                OEmpSalStructDet.SalHeadFormula = null;
                                                db.EmpSalStructDetails.Attach(OEmpSalStructDet);
                                                db.Entry(OEmpSalStructDet).State = System.Data.Entity.EntityState.Modified;
                                                db.SaveChanges();
                                                db.Entry(OEmpSalStructDet).State = System.Data.Entity.EntityState.Detached;
                                            }
                                         
                                        }
                                    }
                                }
                            }

                            ts.Complete();
                            // return Json(new { success = true, responseText = "Data Updated Successfully." }, JsonRequestBehavior.AllowGet);

                            //}
                            //else
                            //{
                            //    // return Json(new { success = false, responseText = "Make A Release Flag True" }, JsonRequestBehavior.AllowGet);
                            //    Msg.Add(" Make A Release Flag True");
                            //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            //}
                        }
                    }
                    Msg.Add("  Data Updated successfully  ");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                }
                catch (Exception ex)
                {
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
                    //  List<string> Msg = new List<string>();
                    Msg.Add(ex.Message);
                    return Json(new { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        #endregion

        public ActionResult Hold(IncrementServiceBook IncrementServiceBook, FormCollection form, String forwarddata, string param) //Create submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    // var Emp = form["emp_Id"] == "0" ? "" : form["emp_Id"];
                    //  var ReleaseFlag = form["ReleaseFlag"] == "0" ? "" : form["ReleaseFlag"];
                    var HoldNarration = form["txtHoldNarrationRelease"] == "0" ? "" : form["txtHoldNarrationRelease"];
                    var Activityid = form["Activity_Idh"] == "0" ? "" : form["Activity_Idh"];
                    int CompId = 0;
                    if (!String.IsNullOrEmpty(Session["CompId"].ToString()))
                    {
                        CompId = Convert.ToInt32(Session["CompId"]);
                    }

                    //int Empid = 0;
                    //if (Emp != null)
                    //{
                    //    Empid = int.Parse(Emp);
                    //}

                    //       EmployeePayroll OEmployeePayroll = null;

                    //int PayScaleAgrId = int.Parse(PayScaleAgr);
                    //var OPayScalAgreement = db.PayScaleAgreement.Where(e => e.Id == PayScaleAgrId).SingleOrDefault();
                    //"1464,1465,1466"

                    //     OEmployeePayroll = db.EmployeePayroll.Where(e => e.Employee.Id == Empid).SingleOrDefault();
                    //   IncrementServiceBook.ReleaseFlag = Convert.ToBoolean(ReleaseFlag);
                    //var ids = Utility.StringIdsToListIds(forwarddata);
                    var ids = Utility.StringIdsToListIds(Activityid);
                    DateTime? currentdate = DateTime.Now;
                    using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                      new System.TimeSpan(0, 30, 0)))
                    {

                        foreach (var a in ids)
                        {
                            DBTrack dbt = new DBTrack() { Action = "C", CreatedBy = SessionManager.UserName, CreatedOn = DateTime.Now };
                            IncrementServiceBook OIncrementServiceBook = db.IncrementServiceBook.Where(e => e.Id == a).SingleOrDefault();
                            OIncrementServiceBook.IsHold = true;
                            OIncrementServiceBook.ReleaseDate = IncrementServiceBook.ReleaseDate;
                            db.IncrementServiceBook.Attach(OIncrementServiceBook);
                            db.Entry(OIncrementServiceBook).State = System.Data.Entity.EntityState.Modified;
                            //await db.SaveChangesAsync();
                            db.SaveChanges();
                            TempData["RowVersion"] = OIncrementServiceBook.RowVersion;
                            // db.Entry(OIncrementServiceBook).State = System.Data.Entity.EntityState.Detached;
                            IncrementHoldReleaseDetails incrementhold = new IncrementHoldReleaseDetails
                            {
                                DBTrack = dbt,
                                IncrementServiceBook = OIncrementServiceBook,
                                HoldNarration = HoldNarration,
                                ReleaseDate = IncrementServiceBook.ReleaseDate,
                                HoldDate = currentdate
                            };
                            db.IncrementHoldReleaseDetails.Add(incrementhold);
                            db.SaveChanges();
                            // Process.ServiceBook.ServiceBookProcess(CompId, "INCREMENT_RELEASE", OIncrementServiceBook.Id, null, null, null, OEmployeePayroll.Id, "INCREMENT", IncrementServiceBook.ReleaseDate, false, false);
                            //db.Entry(OIncrementServiceBook).State = System.Data.Entity.EntityState.Modified;
                            //db.SaveChanges();
                            // return Json(new { success = true, responseText = "Data Updated Successfully." }, JsonRequestBehavior.AllowGet);



                        }
                        ts.Complete();
                        Msg.Add("  Data Updated successfully  ");
                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);


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
                        LineNo = Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                        LogTime = DateTime.Now
                    };
                    Logfile.CreateLogFile(Err);
                    //  List<string> Msg = new List<string>();
                    Msg.Add(ex.Message);
                    return Json(new { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
            }
        }


        public JsonResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                var Q = db.IncrementServiceBook
                    .Include(e => e.IncrActivity.IncrList).Include(e => e.GeoStruct)
                    .Include(e => e.FuncStruct).Include(e => e.PayStruct)
                    .Where(e => e.Id == data).SingleOrDefault();

                var returnObj = new
                {
                    IncrActivity = Q.IncrActivity.IncrList.LookupVal,
                    ProcessIncrDate = Q.ProcessIncrDate.Value.ToShortDateString(),
                    GeoStruct_FullDetails = Q.GeoStruct == null ? null : Q.GeoStruct.FullDetails,
                    GeoStruct_Id = Q.GeoStruct.Id == null ? "" : Q.GeoStruct.Id.ToString(),
                    PayStruct_FullDetails = Q.PayStruct == null ? null : Q.PayStruct.FullDetails,
                    PayStruct_Id = Q.PayStruct.Id == null ? "" : Q.PayStruct.Id.ToString(),
                    FuncStruct_FullDetails = Q.FuncStruct == null ? null : Q.FuncStruct.FullDetails,
                    FuncStruct_Id = Q.FuncStruct.Id == null ? "" : Q.FuncStruct.Id.ToString(),
                    Narration = Q.Narration == null ? "" : Q.Narration,
                    Releasedate = Q.ReleaseDate == null ? "" : Q.ReleaseDate.Value.ToShortDateString()
                };

                var IncrServBook = db.IncrementServiceBook.Find(data);
                Session["RowVersion"] = IncrServBook.RowVersion;
                var Auth = IncrServBook.DBTrack.IsModified;
                //return Json(new Object[] { Q, add_data, "", Auth, JsonRequestBehavior.AllowGet });
                return Json(new Object[] { returnObj, Auth }, JsonRequestBehavior.AllowGet);
            }
        }

        public class IncrServBookGridData //releasegrid
        {
            public int Id { get; set; }
            public int EmpId { get; set; }
            public Employee Employee { get; set; }
            public IncrActivity IncrActivity { get; set; }
            public string ProcessDate { get; set; }
            public double OldBasic { get; set; }
            public double NewBasic { get; set; }
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

        public class IncrChildDataClass //childgrid
        {
            public int Id { get; set; }
            public bool Release { get; set; }
            public string ReleaseDate { get; set; }
            public string Activity { get; set; }
            public string ProcessDate { get; set; }
            public string OriginalDate { get; set; }
            public double OldBasic { get; set; }
            public double NewBasic { get; set; }
        }

        public ActionResult P2BGridRelease(P2BGrid_Parameters gp, string param)
        {
            try
            {
                DataBaseContext db = new DataBaseContext();
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                var jsonData = (Object)null;

                IEnumerable<IncrServBookGridData> IncrServBook = null;
                List<IncrServBookGridData> model = new List<IncrServBookGridData>();
                IncrServBookGridData view = null;

                var WEmployee = db.EmployeePayroll
                    .Include(e => e.IncrementServiceBook)
                    .Include(e => e.Employee)
                    .Include(e => e.Employee.EmpName)
                    .Include(e => e.Employee.ServiceBookDates)
                    .Where(q => q.IncrementServiceBook.Count > 0).ToList();

                var OEmployee = WEmployee.Where(e => e.Employee.ServiceBookDates != null && e.Employee.ServiceBookDates.ServiceLastDate == null).ToList();

                string PayMonth = "";
                string Month = "";
                if (gp.filter != null)
                    PayMonth = gp.filter;
                else
                {
                    if (DateTime.Now.Date.Month < 10)
                        Month = "0" + DateTime.Now.Date.Month;
                    else
                        Month = DateTime.Now.Date.Month.ToString();
                    PayMonth = Month + "/" + DateTime.Now.Date.Year;
                }
                foreach (var z in OEmployee)
                {
                    var ObjIncrServBook = db.EmployeePayroll.Where(e => e.Id == z.Id)
                        .Select(e => e.IncrementServiceBook.Where(r => r.ReleaseFlag == false))
                                        .SingleOrDefault();


                    //DateTime? Eff_Date = null;
                    //PayScaleAgreement PayScaleAgr = null;
                    foreach (var a in ObjIncrServBook)
                    {
                        //Eff_Date = Convert.ToDateTime(a.ProcessMonth);
                        IncrementServiceBook aa = new IncrementServiceBook();
                        bool flag = Convert.ToBoolean(param);
                        if (flag == true)
                        {

                            aa = db.IncrementServiceBook
                                //.Include(e => e.PayStruct).Include(e => e.PayStruct.Level).Include(e => e.PayStruct.Grade)
                                //.Include(e => e.PayStruct.JobStatus)
                                //.Include(e => e.FuncStruct).Include(e => e.FuncStruct.Job).Include(e => e.FuncStruct.JobPosition)
                                //.Include(e => e.GeoStruct.Company).Include(e => e.GeoStruct.Corporate).Include(e => e.GeoStruct.Department)
                                //.Include(e => e.GeoStruct.Division).Include(e => e.GeoStruct.Group).Include(e => e.GeoStruct.Location)
                                //.Include(e => e.GeoStruct.Region).Include(e => e.GeoStruct.Unit)
                                 .Include(e => e.IncrActivity).Include(e => e.IncrActivity.IncrList)
                                  .Where(e => e.Id == a.Id && e.IsHold == false)
                              .SingleOrDefault();
                        }
                        else
                        {
                            aa = db.IncrementServiceBook
                               .Include(e => e.IncrActivity).Include(e => e.IncrActivity.IncrList)
                                       .Where(e => e.Id == a.Id && e.IsHold == true)
                  .SingleOrDefault();
                        }
                        if (aa != null)
                        {
                            //if (aa.ProcessIncrDate.Value.ToString("MM/yyyy") == PayMonth)
                            //{
                            view = new IncrServBookGridData()
                            {
                                Id = a.Id,
                                EmpId = z.Employee.Id,
                                Employee = z.Employee,
                                IncrActivity = aa.IncrActivity,
                                ProcessDate = aa.ProcessIncrDate != null ? aa.ProcessIncrDate.Value.ToString("dd/MM/yyyy") : null,
                                OldBasic = aa.OldBasic,
                                NewBasic = aa.NewBasic
                            };

                            model.Add(view);
                            db.Entry(aa).State = System.Data.Entity.EntityState.Detached;
                            //}
                        }
                    }

                }

                IncrServBook = model;

                IEnumerable<IncrServBookGridData> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = IncrServBook;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.EmpId.ToString().Contains(gp.searchString))
                                || (e.Employee.EmpCode.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                                || (e.Employee.EmpName.FullNameFML.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                                || (e.IncrActivity.IncrList.LookupVal.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                                || (e.ProcessDate.ToString().Contains(gp.searchString))
                                || (e.OldBasic.ToString().Contains(gp.searchString))
                                || (e.NewBasic.ToString().Contains(gp.searchString))
                                || (e.Id.ToString().Contains(gp.searchString))
                            ).Select(a => new Object[] { a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.IncrActivity.IncrList.LookupVal, a.ProcessDate, a.OldBasic, a.NewBasic, a.Id, a.EmpId }).ToList();
                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.IncrActivity.IncrList.LookupVal, a.ProcessDate, a.OldBasic, a.NewBasic, a.Id, a.EmpId }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = IncrServBook;
                    Func<IncrServBookGridData, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "EmpId" ? c.EmpId.ToString() :
                                         gp.sidx == "EmpCode" ? c.Employee.EmpCode.ToString() :
                                         gp.sidx == "EmpName" ? c.Employee.EmpName.FullNameFML.ToString() :
                                         gp.sidx == "Activity" ? c.IncrActivity.IncrList.LookupVal :
                                         gp.sidx == "ProcessDate" ? c.ProcessDate.ToString() :
                                         gp.sidx == "OldBasic" ? c.OldBasic.ToString() :
                                         gp.sidx == "NewBasic" ? c.NewBasic.ToString() : ""
                                          );
                    }
                    if (gp.sord == "asc")  //Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : ""
                    {
                        IE = IE.OrderBy(orderfuc);  // a.Id,a.Employeee.EmpCode,a.Employeee.EmpName, a.SalaryHead.Name, a.FromPeriod, a.ToPeriod, a.AmountPaid, a.OtherDeduction, a.ProcessMonth, a.TDSAmount, a.Narration
                        jsonData = IE.Select(a => new Object[] { a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.IncrActivity.IncrList != null ? a.IncrActivity.IncrList.LookupVal : "", a.ProcessDate, a.OldBasic, a.NewBasic, a.Id, a.EmpId }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.IncrActivity.IncrList != null ? a.IncrActivity.IncrList.LookupVal : "", a.ProcessDate, a.OldBasic, a.NewBasic, a.Id, a.EmpId }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.IncrActivity.IncrList != null ? a.IncrActivity.IncrList.LookupVal : "", a.ProcessDate, a.OldBasic, a.NewBasic, a.Id, a.EmpId }).ToList();
                    }
                    totalRecords = IncrServBook.Count();
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

        public ActionResult Get_Employelist_h(string geo_id)
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
                var compid = Convert.ToInt32(Session["CompId"].ToString());

                //var empdata = db.EmployeePayroll
                //   .Include(e => e.IncrDataCalc)
                //   .Include(a => a.Employee)
                //   .Include(a => a.Employee.GeoStruct)
                //   .Include(a => a.Employee.FuncStruct)
                //   .Include(a => a.Employee.PayStruct)
                //   .Include(a => a.Employee.EmpName)
                //   .Include(a => a.Employee.ServiceBookDates)
                //   .Where(e => e.Company.Id == compid).AsNoTracking().AsParallel().SingleOrDefault();

                var empdata = db.CompanyPayroll
                .Include(e => e.EmployeePayroll)
                   .Include(e => e.EmployeePayroll.Select(a => a.IncrDataCalc))
                   .Include(e => e.EmployeePayroll.Select(a => a.Employee))
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee.GeoStruct))
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee.FuncStruct))
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee.PayStruct))
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee.EmpName))
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee.ServiceBookDates))
                    .Where(e => e.Company.Id == compid).AsNoTracking().AsParallel().SingleOrDefault();
                DateTime dateTime = DateTime.UtcNow.Date;
                //string Cmon = dateTime.Month.ToString().Length == 1 ? "0" + dateTime.Month.ToString() : dateTime.Month.ToString();
                //   string CIncrMonYr = Cmon + "/" +dateTime.Year;

                List<EmployeePayroll> AllReq_Emp = new List<EmployeePayroll>();
                foreach (var z in empdata.EmployeePayroll)
                {
                    if (z.IncrDataCalc != null)
                    {
                        //string mon = z.IncrDataCalc.ProcessIncrDate.Value.Month.ToString().Length == 1 ? "0" + z.IncrDataCalc.ProcessIncrDate.Value.Month.ToString() : z.IncrDataCalc.ProcessIncrDate.Value.Month.ToString();
                        //string IncrMonYr = mon + "/" + z.IncrDataCalc.ProcessIncrDate.Value.Year;
                        if (z.IncrDataCalc.ProcessIncrDate <= dateTime)
                        {
                            AllReq_Emp.Add(z);
                        }
                    }
                }
                List<EmployeePayroll> List_all = new List<EmployeePayroll>();
                if (deserialize.GeoStruct != null)
                {
                    var one_id = Utility.StringIdsToListIds(deserialize.GeoStruct);
                    foreach (var ca in one_id)
                    {
                        var id = Convert.ToInt32(ca);
                        var List_all_temp = AllReq_Emp.Where(e => e.Employee.GeoStruct != null && e.Employee.GeoStruct.Id == id).ToList();
                        if (List_all_temp != null && List_all_temp.Count != 0)
                        {
                            List_all.AddRange(List_all_temp);
                        }
                    }
                }
                if (deserialize.PayStruct != null)
                {
                    var one_id = Utility.StringIdsToListIds(deserialize.PayStruct);
                    var List_all_temp = new List<EmployeePayroll>();
                    if (List_all.Count > 0)
                    {
                        List_all_temp = List_all.Where(e => e.Employee.PayStruct != null && one_id.Contains(e.Employee.PayStruct.Id)).ToList();
                    }
                    else
                    {
                        List_all_temp = AllReq_Emp.Where(e => e.Employee.PayStruct != null && one_id.Contains(e.Employee.PayStruct.Id)).ToList();
                    }
                    if (List_all_temp != null && List_all_temp.Count != 0)
                    {
                        List_all = List_all_temp;
                    }

                }
                if (deserialize.FunStruct != null)
                {
                    var one_id = Utility.StringIdsToListIds(deserialize.FunStruct);
                    var List_all_temp = new List<EmployeePayroll>();
                    if (List_all.Count > 0)
                    {
                        List_all_temp = List_all.Where(e => e.Employee.FuncStruct != null && one_id.Contains(e.Employee.FuncStruct.Id)).ToList();
                    }
                    else
                    {
                        List_all_temp = AllReq_Emp.Where(e => e.Employee.FuncStruct != null && one_id.Contains(e.Employee.FuncStruct.Id)).ToList();
                    }
                    //var List_all_temp = List_all.Where(e => e.Employee.FuncStruct != null && one_id.Contains(e.Employee.FuncStruct.Id)).ToList();
                    if (List_all_temp != null && List_all_temp.Count != 0)
                    {
                        List_all = List_all_temp;
                    }

                }

                List<Utility.returndataclass> returndata = new List<Utility.returndataclass>();
                if (List_all != null && List_all.Count != 0)
                {
                    List<Employee> data = new List<Employee>();
                    data = List_all.Select(e => e.Employee).ToList();
                    if (deserialize.CheckAll == "check")
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
                                item.ServiceBookDates.ServiceLastDate.Value >= dtChk))
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
                        tablename = "Employee-Table"
                    };
                    return Json(new List<Object> { returnjson }, JsonRequestBehavior.AllowGet);
                }
                if (List_all.Count == 0)
                {
                    List<Employee> data = new List<Employee>();
                    data = db.Employee.Include(e => e.ServiceBookDates).Include(e => e.EmpName).ToList();
                    if (deserialize.CheckAll == "check")
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
                                item.ServiceBookDates.ServiceLastDate.Value >= dtChk))
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
                        tablename = "Employee-Table"
                    };
                    return Json(new List<Object> { returnjson }, JsonRequestBehavior.AllowGet);
                }

                else
                {
                    return Json("No Record Found", JsonRequestBehavior.AllowGet);
                }
            }
        }


        public ActionResult Emp_Grid(ParamModel param, string y)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var all = db.EmployeePayroll.Include(e => e.Employee)
                        .Include(e => e.Employee.EmpName)
                        .Include(e => e.Employee.ServiceBookDates)
                        //.Include(e => e.Employee.GeoStruct)
                        //.Include(e => e.Employee.GeoStruct.Location.LocationObj)
                        //.Include(e => e.Employee.FuncStruct)
                        //.Include(e => e.Employee.FuncStruct.Job)
                        //.Include(e => e.Employee.PayStruct)
                        //.Include(e => e.Employee.PayStruct.Grade)
                       .ToList();
                    // for searchs
                    IEnumerable<EmployeePayroll> fall;
                    if (param.sSearch == null)
                    {
                        fall = all;
                    }
                    else
                    {
                        //fall = all.Where(e => (e.Employee.EmpCode == param.sSearch) || (e.Employee.EmpName.FullNameFML.ToUpper() == param.sSearch.ToUpper())).ToList();
                        fall = all.Where(e => (e.Id.ToString().Contains(param.sSearch))
                               || (e.Employee.EmpCode.Contains(param.sSearch))
                               || (e.Employee.EmpName.FullNameFML.ToUpper().Contains(param.sSearch.ToUpper()))
                               ).ToList();
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

                    //if (string.IsNullOrEmpty(sortcolumn))
                    //{
                    //    fall = fall.OrderByDescending(c => c.Id);
                    //}
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
                                Code = item.Employee.EmpCode,
                                Name = item.Employee.EmpName != null ? item.Employee.EmpName.FullNameFML : null,
                                //JoiningDate = item.Employee.ServiceBookDates != null && item.Employee.ServiceBookDates.JoiningDate != null ? item.Employee.ServiceBookDates.JoiningDate.Value.ToShortDateString() : null,
                                //Job = item.Employee.FuncStruct != null && item.Employee.FuncStruct.Job != null ? item.Employee.FuncStruct.Job.Name : null,
                                //Grade = item.Employee.PayStruct != null && item.Employee.PayStruct.Grade != null ? item.Employee.PayStruct.Grade.Name : null,
                                //Location = item.Employee.GeoStruct != null && item.Employee.GeoStruct.Location != null && item.Employee.GeoStruct.Location.LocationObj != null ? item.Employee.GeoStruct.Location.LocationObj.LocDesc : null
                            });
                        }
                        //return Json(new

                        var jsonResult = Json(new
                        {
                            sEcho = param.sEcho,
                            iTotalRecords = all.Count(),
                            iTotalDisplayRecords = fall.Count(),
                            data = result
                        }, JsonRequestBehavior.AllowGet);
                        jsonResult.MaxJsonLength = int.MaxValue;
                        return jsonResult;
                    }
                    else
                    {
                        var result = from c in dcompanies

                                     select new[] { null, Convert.ToString(c.Id), c.Employee.EmpCode, };
                        //return Json(new
                        //{
                        var jsonResult = Json(new
                        {
                            sEcho = param.sEcho,
                            iTotalRecords = all.Count(),
                            iTotalDisplayRecords = fall.Count(),
                            data = result
                        }, JsonRequestBehavior.AllowGet);
                        jsonResult.MaxJsonLength = int.MaxValue;
                        return jsonResult;
                    }//for data reterivation
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }


        public ActionResult Get_IncrServBook(int data)
        {
            try
            {
                using (DataBaseContext db = new DataBaseContext())
                {
                    var db_data = db.EmployeePayroll
                        .Include(e => e.IncrementServiceBook.Select(r => r.IncrActivity))
                        .Include(e => e.IncrementServiceBook.Select(r => r.IncrActivity.IncrList))
                        .Include(e => e.IncrementServiceBook.Select(r => r.GeoStruct))
                        .Include(e => e.IncrementServiceBook.Select(r => r.PayStruct))
                        .Include(e => e.IncrementServiceBook.Select(r => r.FuncStruct))
                        //.Include(e => e.IncrementServiceBook.Select(r => r.GeoStruct.Company))
                        //.Include(e => e.IncrementServiceBook.Select(r => r.GeoStruct.Corporate))
                        //.Include(e => e.IncrementServiceBook.Select(r => r.GeoStruct.Department))
                        //.Include(e => e.IncrementServiceBook.Select(r => r.GeoStruct.Division))
                        //.Include(e => e.IncrementServiceBook.Select(r => r.GeoStruct.Group))
                        //.Include(e => e.IncrementServiceBook.Select(r => r.GeoStruct.Location))
                        //.Include(e => e.IncrementServiceBook.Select(r => r.GeoStruct.Region))
                        //.Include(e => e.IncrementServiceBook.Select(r => r.GeoStruct.Unit))
                        //.Include(e => e.IncrementServiceBook.Select(r => r.PayStruct.Company))
                        //.Include(e => e.IncrementServiceBook.Select(r => r.PayStruct.Grade))
                        //.Include(e => e.IncrementServiceBook.Select(r => r.PayStruct.JobStatus))
                        //.Include(e => e.IncrementServiceBook.Select(r => r.PayStruct.Level))
                        //.Include(e => e.IncrementServiceBook.Select(r => r.FuncStruct.Company))
                        //.Include(e => e.IncrementServiceBook.Select(r => r.FuncStruct.Job))
                        //.Include(e => e.IncrementServiceBook.Select(r => r.FuncStruct.JobPosition))
                        .Where(e => e.Id == data).SingleOrDefault();
                    if (db_data != null)
                    {
                        List<IncrChildDataClass> returndata = new List<IncrChildDataClass>();
                        foreach (var item in db_data.IncrementServiceBook)
                        {
                            returndata.Add(new IncrChildDataClass
                            {
                                Id = item.Id,
                                Release = item.ReleaseFlag,
                                ReleaseDate = item.ReleaseDate != null ? item.ReleaseDate.Value.ToString() : null,
                                Activity = item.IncrActivity != null ? item.IncrActivity.IncrList.LookupVal : null,
                                ProcessDate = item.ProcessIncrDate != null ? item.ProcessIncrDate.Value.ToString("dd/MM/yyyy") : null,
                                OriginalDate = item.OrignalIncrDate != null ? item.OrignalIncrDate.Value.ToString("dd/MM/yyyy") : null,
                                OldBasic = item.OldBasic,
                                NewBasic = item.NewBasic
                            });
                        }
                        return Json(returndata, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            List<string> Msg = new List<string>();
            try
            {
                using (DataBaseContext db = new DataBaseContext())
                {
                    IncrementServiceBook IncrServBook = db.IncrementServiceBook.Where(e => e.Id == data).SingleOrDefault();

                    if (IncrServBook.ReleaseFlag == true)
                    {
                        return this.Json(new { status = true, valid = true, responseText = "You cannot delete as increment is already released.", JsonRequestBehavior.AllowGet });
                    }

                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {


                        try
                        {

                            db.IncrementServiceBook.Remove(IncrServBook);
                            await db.SaveChangesAsync();


                            ts.Complete();
                            return this.Json(new { status = true, responseText = "Data removed.", JsonRequestBehavior.AllowGet });

                        }
                        catch (RetryLimitExceededException /* dex */)
                        {
                            //Log the error (uncomment dex variable name and add a line here to write a log.)
                            //ModelState.AddModelError("", "Unable to delete. Try again, and if the problem persists, see your system administrator.");
                            //return RedirectToAction("Delete");
                            //    return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });

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

        [HttpPost]
        public ActionResult GridDelete(int data)
        {
            List<string> Msg = new List<string>();
            try
            {
                using (DataBaseContext db = new DataBaseContext())
                {
                    IncrementServiceBook IncrServBook = db.IncrementServiceBook.Where(e => e.Id == data).SingleOrDefault();

                    if (IncrServBook.ReleaseFlag == true)
                    {
                        return this.Json(new { status = true, valid = true, responseText = "You cannot delete as activity is already released.", JsonRequestBehavior.AllowGet });
                    }

                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        try
                        {
                            db.IncrementServiceBook.Attach(IncrServBook);
                            db.Entry(IncrServBook).State = System.Data.Entity.EntityState.Deleted;
                            db.SaveChanges();
                            db.Entry(IncrServBook).State = System.Data.Entity.EntityState.Detached;
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