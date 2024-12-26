using P2b.Global;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Payroll;
using P2BUltimate.Process;
using P2BUltimate.Security;
using System.Data.Entity.Core.Objects;
using System.Diagnostics;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Validation;

namespace P2BUltimate.Controllers.Payroll.MainController
{
    public class CPIEntryFormController : Controller
    {
        //  private DataBaseContext db = new DataBaseContext();

        public ActionResult Index()
        {
            // db.RefreshAllEntites(RefreshMode.StoreWins);//added by prashnat 13042017
            return View("~/Views/Payroll/MainViews/CPIEntryForm/Index.cshtml");
        }

        public ActionResult GetCode(int Id)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    Company OComany = db.Company.FirstOrDefault();
                    string compcode = OComany.Code;
                    return Json(new Object[] { compcode, JsonRequestBehavior.AllowGet });

                }
                catch (Exception ex)
                {

                    string content = string.Empty;
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
                    content = "LogFile Created";
                    //return View("~/Views/Payroll/MainViews/PFECRSummaryR/Index.cshtml");
                    return Json(new Object[] { content, "", JsonRequestBehavior.AllowGet });
                }
            }
        }

        public ActionResult Polulate_payscale_agreement(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var query = db.PayScaleAgreement.ToList();
                var selected = (Object)null;
                data2 = db.PayScaleAgreement.Where(e => e.EndDate == null).SingleOrDefault().Id.ToString();
                if (data2 != "" && data != "0" && data2 != "0")
                {
                    selected = Convert.ToInt32(data2);
                }

                SelectList s = new SelectList(query, "Id", "FullDetails", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }




        public ActionResult getPayscaleDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                int id = int.Parse(data);
                var query = db.PayScaleAgreement.Include(e => e.PayScale).Where(e => e.PayScale.CPIAppl == true && e.Id == id).SingleOrDefault();

                var selected = query.PayScale.ActualIndexAppl;

                return Json(selected, JsonRequestBehavior.AllowGet);
            }
        }
        public List<string> ValidateObj(Object obj)
        {
            var errorList = new List<String>();
            var context = new ValidationContext(obj, serviceProvider: null, items: null);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(obj, context, results, true);
            if (!isValid)
            {
                foreach (var validationResult in results)
                {
                    errorList.Add(validationResult.ErrorMessage);
                }
                return errorList;
            }
            else
            {
                return errorList;
            }
        }
        public EmployeePayroll _returnEmployeePayrollOne(Int32 empid)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                EmployeePayroll oEmployeePayroll = db.EmployeePayroll.Where(e => e.Id == empid).SingleOrDefault();
                EmpSalStruct EmpSalStruct = db.EmpSalStruct.Where(e => e.EmployeePayroll_Id == oEmployeePayroll.Id && e.EndDate == null).SingleOrDefault();
                Employee Employee = db.EmployeePayroll.Where(e => e.Id == oEmployeePayroll.Employee_Id).Select(r => r.Employee).SingleOrDefault();
                NameSingle EmpName = db.NameSingle.Where(e => e.Id == Employee.EmpName_Id).SingleOrDefault();
                EmpOff EmpOffInfo = db.EmpOff.Where(e => e.Id == Employee.EmpOffInfo_Id).SingleOrDefault();
                PayScale PayScale = db.PayScale.Where(e => e.Id == EmpOffInfo.PayScale_Id).SingleOrDefault();
                ServiceBookDates ServiceBookDates = db.ServiceBookDates.Where(e => e.Id == Employee.ServiceBookDates_Id).SingleOrDefault();
                oEmployeePayroll.EmpSalStruct.Add(EmpSalStruct);
                oEmployeePayroll.Employee = Employee;
                Employee.EmpName = EmpName;
                Employee.EmpOffInfo = EmpOffInfo;
                Employee.ServiceBookDates = ServiceBookDates;
                EmpOffInfo.PayScale = PayScale;
                //.Include(e => e.EmpSalStruct)
                //.Include(e => e.Employee.EmpName)
                //.Include(e => e.Employee.EmpOffInfo)
                //.Include(e => e.Employee.ServiceBookDates)
                //.Include(e => e.Employee.EmpOffInfo.PayScale)
                /*.Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails))
                .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.PayScaleAssignment)))
                .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.PayScaleAssignment.PayScaleAgreement)))
                .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead)))
                .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.SalHeadOperationType)))*/

                return oEmployeePayroll;
            }
        }
        public List<EmployeePayroll> _returnEmployeePayrollOneList(Int32 empid, string Paymonth, List<int> EmpList)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                DateTime EffectiveDate = Convert.ToDateTime(Paymonth);
                List<EmployeePayroll> oEmployeePayrollList = db.EmployeePayroll.Where(e => EmpList.Contains(e.Id)).ToList();
                foreach (var oEmployeePayrollListitem in oEmployeePayrollList)
                {

                    List<EmpSalStruct> EmpSalStruct = db.EmpSalStruct.Where(e => e.EmployeePayroll_Id == oEmployeePayrollListitem.Id && e.EffectiveDate >= EffectiveDate).ToList();
                    Employee Employee = db.EmployeePayroll.Where(e => e.Id == oEmployeePayrollListitem.Employee_Id).Select(r => r.Employee).SingleOrDefault();
                    NameSingle EmpName = db.NameSingle.Where(e => e.Id == Employee.EmpName_Id).SingleOrDefault();
                    EmpOff EmpOffInfo = db.EmpOff.Where(e => e.Id == Employee.EmpOffInfo_Id).SingleOrDefault();
                    PayScale PayScale = db.PayScale.Where(e => e.Id == EmpOffInfo.PayScale_Id).SingleOrDefault();
                    ServiceBookDates ServiceBookDates = db.ServiceBookDates.Where(e => e.Id == Employee.ServiceBookDates_Id).SingleOrDefault();
                    oEmployeePayrollListitem.EmpSalStruct = EmpSalStruct;
                    oEmployeePayrollListitem.Employee = Employee;
                    Employee.EmpName = EmpName;
                    Employee.EmpOffInfo = EmpOffInfo;
                    Employee.ServiceBookDates = ServiceBookDates;
                    EmpOffInfo.PayScale = PayScale;
                    //.Include(e => e.EmpSalStruct)
                    //.Include(e => e.Employee.EmpName)
                    //.Include(e => e.Employee.EmpOffInfo)
                    //.Include(e => e.Employee.ServiceBookDates)
                    //.Include(e => e.Employee.EmpOffInfo.PayScale)
                    /*.Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails))
                    .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.PayScaleAssignment)))
                    .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.PayScaleAssignment.PayScaleAgreement)))
                    .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead)))
                    .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.SalHeadOperationType)))*/
                }
                return oEmployeePayrollList;
            }
        }
        public EmployeePayroll _returnEmployeePayrollOneMonth(Int32 empid, string Paymonth)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                DateTime EffectiveDate = Convert.ToDateTime(Paymonth);
                EmployeePayroll oEmployeePayroll = db.EmployeePayroll.Where(e => e.Id == empid).SingleOrDefault();

                List<EmpSalStruct> EmpSalStruct = db.EmpSalStruct.Where(e => e.EmployeePayroll_Id == oEmployeePayroll.Id && e.EffectiveDate >= EffectiveDate).ToList();
                Employee Employee = db.EmployeePayroll.Where(e => e.Id == oEmployeePayroll.Id).Select(r => r.Employee).SingleOrDefault();
                NameSingle EmpName = db.NameSingle.Where(e => e.Id == Employee.EmpName_Id).SingleOrDefault();
                EmpOff EmpOffInfo = db.EmpOff.Where(e => e.Id == Employee.EmpOffInfo_Id).SingleOrDefault();
                PayScale PayScale = db.PayScale.Where(e => e.Id == EmpOffInfo.PayScale_Id).SingleOrDefault();
                ServiceBookDates ServiceBookDates = db.ServiceBookDates.Where(e => e.Id == Employee.ServiceBookDates_Id).SingleOrDefault();
                oEmployeePayroll.EmpSalStruct = EmpSalStruct;
                oEmployeePayroll.Employee = Employee;
                Employee.EmpName = EmpName;
                Employee.EmpOffInfo = EmpOffInfo;
                Employee.ServiceBookDates = ServiceBookDates;
                EmpOffInfo.PayScale = PayScale;
                //.Include(e => e.EmpSalStruct)
                //.Include(e => e.Employee.EmpName)
                //.Include(e => e.Employee.EmpOffInfo)
                //.Include(e => e.Employee.ServiceBookDates)
                //.Include(e => e.Employee.EmpOffInfo.PayScale)
                /*.Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails))
                .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.PayScaleAssignment)))
                .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.PayScaleAssignment.PayScaleAgreement)))
                .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead)))
                .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.SalHeadOperationType)))*/

                return oEmployeePayroll;
            }
        }
        #region CRUD OPERATION
        #region CREATE
        public List<int> one_ids(string form)
        {
            string itsec_id = form;
            List<int> ids = itsec_id.Split(',').Select(e => int.Parse(e)).ToList();
            return (ids);
        }
        [HttpPost]
        public ActionResult Create(CPIEntryT C, FormCollection form, String forwarddata)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            List<string> Msg = new List<string>();
            try
            {
                using (DataBaseContext db = new DataBaseContext())
                {
                    string PayScale = form["payscaleagreement_drop"] == "0" ? "" : form["payscaleagreement_drop"];
                    string Emp = form["Employee-Table"] == "0" ? "" : form["Employee-Table"];
                    string Paymonth = form["PayMonth"] == "0" ? "" : form["PayMonth"];
                    DateTime EffectiveDate = Convert.ToDateTime(Paymonth);


                    List<int> ids = null;
                    if (Emp != null && Emp != "0" && Emp != "false")
                    {
                        ids = one_ids(Emp);
                    }
                    else
                    {
                        Msg.Add(" Kindly select employee ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    int id = 0;
                    if (PayScale != null && PayScale != "")
                    {
                        id = int.Parse(PayScale);
                        C.PayScale = db.PayScaleAgreement.Include(e => e.PayScale).Where(e => e.Id == id).SingleOrDefault().PayScale;

                    }
                    if (ModelState.IsValid)
                    {
                        if (db.CPIEntryT.AsNoTracking().Any(o => o.PayMonth == C.PayMonth))
                        {
                            Msg.Add(" CPIEntry Already Defined For This Month. ");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }

                        string PrevMonth = Convert.ToDateTime("01/" + C.PayMonth).AddMonths(-1).ToString("MM/yyyy");

                        if (db.SalaryT.AsNoTracking().Count() > 0 && db.SalaryT.AsNoTracking().Where(o => o.PayMonth == PrevMonth).Count() <= 0)
                        {
                            Msg.Add(" Kindly generate salary for previous month. ");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }

                        if (db.SalaryT.AsNoTracking().Any(o => o.PayMonth == PrevMonth && o.ReleaseDate == null))
                        {
                            Msg.Add(" Kindly release previous month salary. ");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                        ///////////////////
                        string Prev_Month = Convert.ToDateTime("01/" + C.PayMonth).AddMonths(-1).ToString("MM/yyyy");

                        //List<int> EmployeeServiceBookId_int = db.ServiceBookDates.Where(e => e.ServiceLastDate == null).Select(r => r.Id).ToList();
                        //List<int> Employee_int = new List<int>();
                        //foreach (var EmployeeServiceBookId_intitem in EmployeeServiceBookId_int)
                        //{
                        //    int I = db.Employee.Where(e => e.ServiceBookDates_Id == EmployeeServiceBookId_intitem).Select(e => e.Id).SingleOrDefault();
                        //    if(I>0)
                        //    Employee_int.Add(I);
                        //}
                        List<int> EmployeePayroll_int = new List<int>();
                        // foreach (var Employee_intitem in Employee_int)
                        foreach (var Employee_intitem in ids)
                        {
                            int I = db.EmployeePayroll.Where(e => e.Employee_Id == Employee_intitem).Select(e => e.Id).SingleOrDefault();
                            if (I > 0)
                                EmployeePayroll_int.Add(I);
                        }

                        List<EmployeePayroll> emp_list = new List<EmployeePayroll>();
                        Boolean chkDaperbasiclinkedda = false;
                        //emp_list = _returnEmployeePayrollOneList(0, Paymonth, EmployeePayroll_int);
                        foreach (var EmpId in EmployeePayroll_int)
                        {
                            EmployeePayroll EmpPay = _returnEmployeePayrollOneMonth(EmpId, Paymonth);
                            emp_list.Add(EmpPay);
                            if ((EmpPay.Employee.ServiceBookDates.ServiceLastDate == null) && (EmpPay.Employee.ServiceBookDates.RetirementDate.Value.ToString("MM/yyyy") == Prev_Month))
                            {
                                Msg.Add(EmpPay.Employee.FullDetails + ",");
                            }
                            if ((EmpPay.Employee.EmpOffInfo.PayScale.CPIAppl == false) && (EmpPay.Employee.ServiceBookDates.ServiceLastDate == null))
                            {
                                DateTime effdate = Convert.ToDateTime("01/" + C.PayMonth).Date;
                                var dapermonth = new BasicLinkedDA();
                                dapermonth = db.BasicLinkedDA.Where(e => e.EffectiveFrom <= effdate && effdate <= e.EffectiveTo).OrderByDescending(x => x.Id).FirstOrDefault();
                                if (dapermonth == null)
                                {
                                    chkDaperbasiclinkedda = true;
                                }
                            }

                        }

                        if (Msg.Count() > 0)
                        {
                            Msg.Add("Please Retire this employee or extend the retirement date of this employee :");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                        if (chkDaperbasiclinkedda == true)
                        {
                            Msg.Add(" Kindly Add Basiclinked DA Percent For this month. ");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                        }
                        C.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                        CPIEntryT CPIEntryT = new CPIEntryT()
                        {
                            ActualIndexPoint = C.ActualIndexPoint,
                            CalIndexPoint = C.CalIndexPoint,
                            PayMonth = C.PayMonth,
                            PayScale = C.PayScale,
                            DBTrack = C.DBTrack
                        };

                        //Utility.DumpProcessStatus(Control: "Cpi Strated..!");
                        foreach (EmployeePayroll EmpPay in emp_list)
                        {
                            // Utility.DumpProcessStatus("" + EmpPayOne + "", 163);

                            // EmployeePayroll EmpPay = _returnEmployeePayrollOne(EmpPayOne);
                            var newCpi = CPIEntryT;

                            if (EmpPay.Employee.ServiceBookDates.ServiceLastDate == null || EmpPay.Employee.ServiceBookDates.ServiceLastDate != null &&
                                Convert.ToDateTime("01/" + EmpPay.Employee.ServiceBookDates.ServiceLastDate.Value.ToString("MM/yyyy")) >= Convert.ToDateTime("01/" + C.PayMonth))
                            {
                                //Utility.DumpProcessStatus(LineNo: 169);

                                if (EmpPay.Employee.EmpOffInfo != null)
                                {
                                    //Utility.DumpProcessStatus(LineNo: 175);

                                    if (EmpPay.Employee.EmpOffInfo.PayScale != null)
                                    {
                                        //Utility.DumpProcessStatus(LineNo: 178);

                                        // int Payscale_Id = EmpPay.Employee.EmpOffInfo.PayScale.Id;
                                        int pagree = Convert.ToInt32(PayScale);

                                        int PayScaleAgr_Id = db.PayScaleAgreement.AsNoTracking().Where(e => e.PayScale.Id == EmpPay.Employee.EmpOffInfo.PayScale.Id && e.Id == pagree).Select(e => e.Id).FirstOrDefault();
                                        if (PayScaleAgr_Id == int.Parse(PayScale))
                                        {
                                            // Utility.DumpProcessStatus(LineNo: 184);

                                            //bool CPIApplicable = EmpPay.Employee.EmpOffInfo.PayScale.CPIAppl;
                                            if (EmpPay.Employee.EmpOffInfo.PayScale.CPIAppl == true)
                                            {
                                                //Utility.DumpProcessStatus(LineNo: 189);

                                                var SalFormExist = db.PayScaleAssignment.Include(e => e.SalaryHead).Include(e => e.SalHeadFormula.Select(r => r.VDADependRule)).AsNoTracking()
                                                .Where(e => e.PayScaleAgreement.Id == id && e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "VDA").ToList();

                                                foreach (var SalForm in SalFormExist)
                                                {
                                                    //Utility.DumpProcessStatus(LineNo: 195);

                                                    if (SalForm.SalHeadFormula.Count > 0)
                                                    {
                                                        //Utility.DumpProcessStatus(LineNo: 200);

                                                        //var CPIAppl = db.EmpSalStruct
                                                        //                .Include(q => q.EmployeePayroll)
                                                        //                .Include(q => q.EmpSalStructDetails)
                                                        //                .Include(q => q.EmpSalStructDetails.Select(qa => qa.SalaryHead))
                                                        //                .Include(q => q.EmpSalStructDetails.Select(qa => qa.SalaryHead.SalHeadOperationType))
                                                        //                .Where(e => e.EndDate == null && e.EmployeePayroll.Id == EmpPayOne).SelectMany(e => e.EmpSalStructDetails.Select(r => r.SalaryHead)
                                                        //                .Where(r => r.SalHeadOperationType.LookupVal.ToUpper() == "VDA")).AsNoTracking().AsParallel().SingleOrDefault();

                                                        var CPIAppl = db.EmpSalStruct.AsNoTracking().Where(e => e.EndDate == null && e.EmployeePayroll.Id == EmpPay.Id)
                                                                 .SelectMany(e => e.EmpSalStructDetails.Select(r => r.SalaryHead)
                                                                .Where(r => r.SalHeadOperationType.LookupVal.ToUpper() == "VDA")).FirstOrDefault();

                                                        using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, new System.TimeSpan(1, 30, 0)))
                                                        {
                                                            if (CPIAppl != null)
                                                            {
                                                                // Utility.DumpProcessStatus(LineNo: 209);

                                                                db.CPIEntryT.Add(CPIEntryT);

                                                                List<CPIEntryT> CPIEntry = new List<CPIEntryT>();
                                                                CPIEntry.Add(CPIEntryT);
                                                                db.SaveChanges();

                                                                List<CPIEntryT> CPIEntry_l = new List<CPIEntryT>();
                                                                CPIEntry_l.Add(CPIEntryT);
                                                                var db_EmpData = db.EmployeePayroll.Where(e => e.Id == EmpPay.Id).SingleOrDefault();
                                                                db_EmpData.CPIEntryT = CPIEntry_l;
                                                                db.EmployeePayroll.Attach(db_EmpData);
                                                                db.Entry(db_EmpData).State = System.Data.Entity.EntityState.Modified;
                                                                db.SaveChanges();
                                                                SalaryHeadGenProcess.EmployeeSalaryStructCreationForCPI(EmpPay, EffectiveDate, pagree);
                                                                ts.Complete();

                                                                break;

                                                            }

                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                //Utility.DumpProcessStatus(LineNo: 237);

                                                //List<PayScaleAssignment> SalFormExist = db.PayScaleAssignment
                                                //     .Include(e => e.SalaryHead)
                                                //     .Include(e => e.PayScaleAgreement)
                                                //     .Include(e => e.SalHeadFormula.Select(r => r.VDADependRule)).AsNoTracking()
                                                //.Where(e => e.PayScaleAgreement.Id == id).ToList();

                                                int CompId = Convert.ToInt32(SessionManager.CompanyId);
                                                //var SalFormExist = db.PayScaleAssignment
                                                //              // .Join(db.PayScaleAgreement, p => p.PayScaleAgreement.Id, pc => pc.Id, (p, pc) => new { p, pc })
                                                //              .Where(p => p.PayScaleAgreement.Id == id && p.CompanyPayroll.Id == CompId)
                                                //              .Select(m => new
                                                //              {
                                                //                  //PayScaleAssignment = m.p,
                                                //                 // SalHeadOperationType = m.p.SalaryHead.SalHeadOperationType,
                                                //                  //Id = m.p.Id, // or m.ppc.pc.ProdId
                                                //                  SalHeadFormula = m.SalHeadFormula,
                                                //                  SalaryHead = m.SalaryHead
                                                //                  // other assignments
                                                //              }).ToList();

                                                var SalFormExist = db.PayScaleAssignment.Where(p => p.PayScaleAgreement.Id == id && p.CompanyPayroll.Id == CompId).ToList();

                                                //foreach (var SalForm in SalFormExist)
                                                //{
                                                //Utility.DumpProcessStatus(LineNo: 244);

                                                if (SalFormExist.Count > 0)
                                                {
                                                    //Utility.DumpProcessStatus(LineNo: 249);

                                                    using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                           new System.TimeSpan(1, 30, 0)))
                                                    {


                                                        //var ErrorList = ValidateObj(newCpi);
                                                        //if (ErrorList.Count > 0)
                                                        //{
                                                        //    Msg.Add(string.Join(",", ErrorList));
                                                        //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                                        //}
                                                        //Utility.DumpProcessStatus(LineNo: 260);

                                                        db.CPIEntryT.Add(CPIEntryT);
                                                        db.SaveChanges();

                                                        List<CPIEntryT> CPIEntry_l = new List<CPIEntryT>();
                                                        CPIEntry_l.Add(CPIEntryT);
                                                        var db_EmpData = db.EmployeePayroll.Where(e => e.Id == EmpPay.Id).SingleOrDefault();
                                                        db_EmpData.CPIEntryT = CPIEntry_l;
                                                        db.EmployeePayroll.Attach(db_EmpData);
                                                        db.Entry(db_EmpData).State = System.Data.Entity.EntityState.Modified;
                                                        //ErrorList = ValidateObj(db_EmpData);
                                                        //if (ErrorList.Count > 0)
                                                        //{
                                                        //    Msg.Add(string.Join(",", ErrorList));
                                                        //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                                        //}
                                                        db.SaveChanges();
                                                        //Utility.DumpProcessStatus(LineNo: 278);

                                                        SalaryHeadGenProcess.EmployeeSalaryStructCreationForCPI(EmpPay, EffectiveDate, pagree);
                                                        newCpi = null;
                                                        ts.Complete();

                                                        // break;
                                                    }
                                                }
                                                // }
                                            }
                                        }
                                    }
                                }
                            }
                            //////////////new

                            //else
                            //{
                            //    List<string> Msgs = new List<string>();
                            //    Msgs.Add("  Retire the employee first  ");
                            //    return Json(new Utility.JsonReturnClass { success = true, responseText = Msgs }, JsonRequestBehavior.AllowGet);
                            //}
                            ///////////////

                            //Utility.DumpProcessStatus(Control: "Cpi End..!");

                        }
                        watch.Stop();
                        Utility.DumpProcessStatus(watch.Elapsed.Hours + ":" + watch.Elapsed.Minutes + ":" + watch.Elapsed.Seconds + ":" + watch.Elapsed.Milliseconds);
                        Msg.Add("  Data Created successfully  ");
                        return Json(new Utility.JsonReturnClass { Id = CPIEntryT.Id, Val = CPIEntryT.ActualIndexPoint.ToString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
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
                        // return this.Json(new { msg = errorMsg });
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
        } // [ValidateAntiForgeryToken]
        #endregion

        #region EDIT & EDIT SAVE

        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.CPIEntryT
                    .Include(e => e.PayScale)
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        ActualIndexPoint = e.ActualIndexPoint,
                        CalIndexPoint = e.CalIndexPoint,
                        PayMonth = e.PayMonth, //DateTime.ParseExact(e.PayMonth.Split('/')[0], "MMM", System.Globalization.CultureInfo.InvariantCulture).Month + " " + e.PayMonth.Split('/')[1],
                        Action = e.DBTrack.Action,
                        PayscaleAgr_Id = e.PayScale.Id == 0 ? 0 : db.PayScaleAgreement.Where(r => r.PayScale.Id == e.PayScale.Id).Select(r => r.Id).FirstOrDefault(),
                        ActualIndexAppl = e.PayScale.Id == 0 ? false : db.PayScale.Where(r => r.Id == e.PayScale.Id).Select(r => r.ActualIndexAppl).FirstOrDefault()
                    }).ToList();


                var W = db.DT_CPIEntryT
                     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                     (e => new
                     {
                         DT_Id = e.Id,
                         ActualIndexPoint = e.ActualIndexPoint == null ? 0 : e.ActualIndexPoint,
                         CalIndexPoint = e.CalIndexPoint == null ? 0 : e.CalIndexPoint,
                         PayMonth = e.PayMonth == null ? "" : e.PayMonth,

                         Payscale_Val = e.PayScale_Id == 0 ? "" : db.PayScale.Where(x => x.Id == e.PayScale_Id).Select(x => x.FullDetails).FirstOrDefault(),

                     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var Corp = db.CPIEntryT.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, "", W, Auth, JsonRequestBehavior.AllowGet });
            }
        }

        [HttpPost]
        public ActionResult EditSave(CPIEntryT C, int data, FormCollection form) // Edit submit
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            List<string> Msg = new List<string>();
            try
            {
                using (DataBaseContext db = new DataBaseContext())
                {
                    string PayScale = form["payscaleagreement_drop"] == "0" ? "" : form["payscaleagreement_drop"];
                    bool Auth = form["Autho_Allow"] == "true" ? true : false;
                    string Emp = form["Employee-Table"] == "0" ? "" : form["Employee-Table"];

                    int pagree = Convert.ToInt32(PayScale);

                    List<int> ids = null;
                    if (Emp != null && Emp != "0" && Emp != "false")
                    {
                        ids = one_ids(Emp);
                    }
                    else
                    {
                        Msg.Add(" Kindly select employee ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    if (db.SalaryT.Any(o => o.PayMonth == C.PayMonth && o.ReleaseDate == null))
                    {
                        Msg.Add(" You can't edit CPI as this month salary is Processed. Please delete salary and try again..");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    if (db.SalaryT.Any(o => o.PayMonth == C.PayMonth && o.ReleaseDate != null))
                    {
                        Msg.Add(" You can't edit CPI as this month salary is released. ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    if (Auth == false)
                    {
                        if (ModelState.IsValid)
                        {
                            try
                            {
                                CPIEntryT blog = null; // to retrieve old data
                                DbPropertyValues originalBlogValues = null;
                                PayScaleAgreement PayScaleAgr = null;
                                using (var context = new DataBaseContext())
                                {
                                    blog = context.CPIEntryT.Where(e => e.Id == data).Include(e => e.PayScale)
                                                            .SingleOrDefault();
                                    originalBlogValues = context.Entry(blog).OriginalValues;
                                    PayScaleAgr = db.PayScaleAgreement.Where(e => e.PayScale.Id == blog.PayScale.Id && e.Id == pagree).SingleOrDefault();
                                }

                                C.DBTrack = new DBTrack
                                {
                                    CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                    CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                    Action = "M",
                                    ModifiedBy = SessionManager.UserName,
                                    ModifiedOn = DateTime.Now
                                };


                                var CurCorp = db.CPIEntryT.Find(data);
                                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                {

                                    List<int> EmployeePayroll_int = new List<int>();
                                    // foreach (var Employee_intitem in Employee_int)
                                    foreach (var Employee_intitem in ids)
                                    {
                                        int I = db.EmployeePayroll.Where(e => e.Employee_Id == Employee_intitem).Select(e => e.Id).SingleOrDefault();
                                        if (I > 0)
                                            EmployeePayroll_int.Add(I);
                                    }

                                    var OEmployeePayroll = db.EmployeePayroll
                                      .Include(e => e.Employee)
                                   .Include(e => e.Employee.ServiceBookDates)
                             .Where(x => x.Employee.ServiceBookDates.ServiceLastDate == null && EmployeePayroll_int.Contains(x.Id))
                                    .ToList();

                                    foreach (var EmpPay in OEmployeePayroll)
                                    {

                                        var CPIEntry = db.CPIEntryT.Where(e => e.PayMonth == CurCorp.PayMonth && e.EmployeePayroll_Id == EmpPay.Id).AsParallel().SingleOrDefault();

                                        if (CPIEntry != null)
                                        {
                                            CPIEntry.ActualIndexPoint = C.ActualIndexPoint;
                                            CPIEntry.CalIndexPoint = C.CalIndexPoint;
                                            db.CPIEntryT.Attach(CPIEntry);
                                            db.Entry(CPIEntry).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();


                                            using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                                                              new System.TimeSpan(0, 30, 0)))
                                            {
                                                SalaryHeadGenProcess.EmployeeSalaryStructUpdateForCPI(EmpPay, Convert.ToDateTime(CPIEntry.PayMonth));
                                                ts.Complete();
                                            }
                                        }
                                        else
                                        {

                                            CPIEntryT CPIEntryT_Data = db.CPIEntryT
                                                                .Include(e => e.PayScale)
                                                                .Where(e => e.Id == data).AsParallel().SingleOrDefault();
                                            PayScaleAgreement _Payscale_agreement = db.PayScaleAgreement.Where(e => e.PayScale.Id == CPIEntryT_Data.PayScale.Id && e.Id == pagree).AsParallel().SingleOrDefault();
                                            CPIEntryT CPIEntryT = new CPIEntryT()
                                            {
                                                ActualIndexPoint = CPIEntryT_Data.ActualIndexPoint,
                                                CalIndexPoint = CPIEntryT_Data.CalIndexPoint,
                                                PayMonth = CPIEntryT_Data.PayMonth,
                                                PayScale = CPIEntryT_Data.PayScale,
                                                DBTrack = CPIEntryT_Data.DBTrack
                                            };
                                            DateTime? EffectiveDate = Convert.ToDateTime("01/" + CPIEntryT_Data.PayMonth);
                                            var newCpi = CPIEntryT;
                                            EmployeePayroll EmpPay_Info = _returnEmployeePayrollOneMonth(EmpPay.Id, CPIEntryT_Data.PayMonth);

                                            // 31/07/2023
                                            //EmployeePayroll EmpPay_Info = db.EmployeePayroll.Where(e => e.Id == EmpPay.Id)
                                            //    .FirstOrDefault();
                                            //Employee Employee = db.Employee.Where(e => e.Id == EmpPay_Info.Employee_Id).SingleOrDefault();
                                            //EmpPay_Info.Employee = Employee;
                                            //EmpOff EmpOffInfo = db.EmpOff.Where(e => e.Id == Employee.EmpOffInfo_Id).SingleOrDefault();
                                            //Employee.EmpOffInfo = EmpOffInfo;
                                            //ServiceBookDates ServiceBookDates = db.ServiceBookDates.Where(e => e.Id == Employee.ServiceBookDates_Id).SingleOrDefault();
                                            //Employee.ServiceBookDates = ServiceBookDates;
                                            //PayScale PayScale1 = db.PayScale.Where(e => e.Id == EmpOffInfo.PayScale_Id).SingleOrDefault();
                                            //EmpOffInfo.PayScale = PayScale1;
                                            //EmpSalStruct OEmpSalStruct = db.EmpSalStruct.Where(e => e.EmployeePayroll_Id == EmpPay_Info.Id && e.EndDate == null).SingleOrDefault();
                                            //EmpPay_Info.EmpSalStruct.Add(OEmpSalStruct);

                                            //List<EmpSalStructDetails> EmpSalStructDetails = db.EmpSalStructDetails.Where(e => e.EmpSalStruct_Id == OEmpSalStruct.Id).ToList();
                                            //OEmpSalStruct.EmpSalStructDetails = EmpSalStructDetails;
                                            //foreach (var EmpSalStructDetailsitem in EmpSalStructDetails)
                                            //{
                                            //    SalaryHead SalaryHead = db.SalaryHead.Where(e => e.Id == EmpSalStructDetailsitem.SalaryHead_Id).SingleOrDefault();
                                            //    EmpSalStructDetailsitem.SalaryHead = SalaryHead;
                                            //    LookupValue SalHeadOperationType = db.LookupValue.Where(e => e.Id == SalaryHead.SalHeadOperationType_Id).SingleOrDefault();
                                            //    SalaryHead.SalHeadOperationType = SalHeadOperationType;
                                            //}

                                            // 31/07/2023
                                            //.Include(e => e.EmpSalStruct)
                                            //.Include(e => e.Employee.EmpOffInfo)
                                            //.Include(e => e.Employee.ServiceBookDates)
                                            //.Include(e => e.Employee.EmpOffInfo.PayScale)
                                            //.Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails))
                                            //.Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.PayScaleAssignment)))
                                            //.Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.PayScaleAssignment.PayScaleAgreement)))
                                            //.Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead)))
                                            //.Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.SalHeadOperationType)))

                                            //var OEmpSalStruct = db.EmpSalStruct.Where(e => e.EmployeePayroll.Id == EmpPay.Id )
                                            //    .Include(e => e.EmpSalStructDetails)
                                            //    // .Include(e => e.EmpSalStructDetails.Select(t => t.PayScaleAssignment))
                                            //    // .Include(e => e.EmpSalStructDetails.Select(t => t.PayScaleAssignment.PayScaleAgreement))
                                            //    .Include(e => e.EmpSalStructDetails.Select(t => t.SalaryHead))
                                            //    .Include(e => e.EmpSalStructDetails.Select(t => t.SalaryHead.SalHeadOperationType))
                                            //    .AsNoTracking().AsParallel().SingleOrDefault();

                                            if (EmpPay_Info.Employee.ServiceBookDates.ServiceLastDate == null || EmpPay_Info.Employee.ServiceBookDates.ServiceLastDate != null
                                                && Convert.ToDateTime("01/" + EmpPay_Info.Employee.ServiceBookDates.ServiceLastDate.Value.ToString("MM/yyyy")) >= Convert.ToDateTime("01/" + C.PayMonth))
                                            {
                                                if (EmpPay_Info.Employee.EmpOffInfo != null)
                                                {
                                                    if (EmpPay_Info.Employee.EmpOffInfo.PayScale != null)
                                                    {
                                                        int Payscale_Id = EmpPay_Info.Employee.EmpOffInfo.PayScale.Id;
                                                        var PayScaleAgr_Id = db.PayScaleAgreement.Where(e => e.PayScale.Id == Payscale_Id && e.Id == pagree).SingleOrDefault();
                                                        if (PayScaleAgr_Id.Id == int.Parse(PayScale))
                                                        {
                                                            var CPIApplicable = EmpPay_Info.Employee.EmpOffInfo.PayScale.CPIAppl;
                                                            if (CPIApplicable == true)
                                                            {
                                                                var SalFormExist = db.PayScaleAssignment
                                                                    .Include(e => e.SalaryHead)
                                                                    .Include(e => e.SalHeadFormula.Select(r => r.VDADependRule))
                                                                    .Where(e => e.PayScaleAgreement.Id == _Payscale_agreement.Id
                                                                        && e.SalaryHead.SalHeadOperationType.LookupVal.ToUpper() == "VDA").AsParallel().ToList();

                                                                foreach (var SalForm in SalFormExist)
                                                                {
                                                                    if (SalForm.SalHeadFormula.Count > 0)
                                                                    {
                                                                        //var CPIAppl = OEmpSalStruct.EmpSalStructDetails.Select(r => r.SalaryHead)
                                                                        //           .Where(r => r.SalHeadOperationType.LookupVal.ToUpper() == "VDA").SingleOrDefault();

                                                                        var CPIAppl = db.EmpSalStruct.AsNoTracking().Where(e => e.EndDate == null && e.EmployeePayroll.Id == EmpPay.Id)
                                                               .SelectMany(e => e.EmpSalStructDetails.Select(r => r.SalaryHead)
                                                              .Where(r => r.SalHeadOperationType.LookupVal.ToUpper() == "VDA")).FirstOrDefault();

                                                                        using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                                               new System.TimeSpan(1, 30, 0)))
                                                                        {
                                                                            if (CPIAppl != null)
                                                                            {
                                                                                db.CPIEntryT.Add(CPIEntryT);

                                                                                List<CPIEntryT> CPIEntry_L = new List<CPIEntryT>();
                                                                                CPIEntry_L.Add(CPIEntryT);
                                                                                db.SaveChanges();

                                                                                // 31/07/2023
                                                                                List<CPIEntryT> CPIEntry_l = new List<CPIEntryT>();
                                                                                CPIEntry_l.Add(CPIEntryT);
                                                                                var db_EmpData = db.EmployeePayroll.Where(e => e.Id == EmpPay.Id).SingleOrDefault();
                                                                                db_EmpData.CPIEntryT = CPIEntry_l;
                                                                                db.EmployeePayroll.Attach(db_EmpData);
                                                                                db.Entry(db_EmpData).State = System.Data.Entity.EntityState.Modified;

                                                                                //EmpPay_Info.CPIEntryT = CPIEntry_L;
                                                                                //db.EmployeePayroll.Attach(EmpPay_Info);
                                                                                //db.Entry(EmpPay_Info).State = System.Data.Entity.EntityState.Modified;
                                                                                db.SaveChanges();
                                                                                // 31/07/2023
                                                                                SalaryHeadGenProcess.EmployeeSalaryStructCreationForCPI(EmpPay_Info, EffectiveDate, pagree);

                                                                                db.Entry(EmpPay_Info).State = System.Data.Entity.EntityState.Detached;
                                                                                ts.Complete();

                                                                                break;

                                                                            }

                                                                        }
                                                                    }
                                                                }
                                                            }
                                                            else
                                                            {
                                                                // var SalFormExist = db.PayScaleAssignment.Include(e => e.SalaryHead)
                                                                //     .Include(e => e.SalHeadFormula.Select(r => r.VDADependRule))
                                                                //.Where(e => e.PayScaleAgreement.Id == _Payscale_agreement.Id).AsNoTracking().AsParallel().ToList();

                                                                List<PayScaleAssignment> SalFormExist = db.PayScaleAssignment.Where(e => e.PayScaleAgreement.Id == PayScaleAgr_Id.Id).ToList();
                                                                foreach (var SalFormExistitem in SalFormExist)
                                                                {
                                                                    SalaryHead SalaryHead = db.SalaryHead.Where(e => e.Id == SalFormExistitem.SalaryHead_Id).SingleOrDefault();
                                                                    SalFormExistitem.SalaryHead = SalaryHead;
                                                                    PayScaleAgreement PayScaleAgreement = db.PayScaleAgreement.Where(e => e.Id == SalFormExistitem.PayScaleAgreement_Id).SingleOrDefault();
                                                                    SalFormExistitem.PayScaleAgreement = PayScaleAgreement;
                                                                    List<SalHeadFormula> SalHeadFormula = db.PayScaleAssignment.Where(e => e.Id == SalFormExistitem.Id).Select(e => e.SalHeadFormula.ToList()).SingleOrDefault();
                                                                    foreach (var SalHeadFormulaitem in SalHeadFormula)
                                                                    {
                                                                        VDADependRule VDADependRule = db.VDADependRule.Where(e => e.Id == SalHeadFormulaitem.VDADependRule_Id).SingleOrDefault();
                                                                        if (VDADependRule != null)
                                                                            SalHeadFormulaitem.VDADependRule = VDADependRule;
                                                                    }
                                                                  //  SalFormExistitem.SalHeadFormula = SalHeadFormula;

                                                                }

                                                                //.Include(e => e.SalaryHead)
                                                                //.Include(e => e.PayScaleAgreement)
                                                                //.Include(e => e.SalHeadFormula.Select(r => r.VDADependRule)).AsNoTracking()


                                                                //foreach (var SalForm in SalFormExist)
                                                                //{
                                                                // if (SalForm.SalHeadFormula.Count > 0)
                                                                if (SalFormExist.Count > 0)
                                                                {
                                                                    using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                                           new System.TimeSpan(1, 30, 0)))
                                                                    {
                                                                        //var ErrorList = ValidateObj(newCpi);
                                                                        //if (ErrorList.Count > 0)
                                                                        //{
                                                                        //    Msg.Add(string.Join(",", ErrorList));
                                                                        //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                                                        //}

                                                                        db.CPIEntryT.Add(CPIEntryT);

                                                                        List<CPIEntryT> CPIEntry_l = new List<CPIEntryT>();
                                                                        CPIEntry_l.Add(CPIEntryT);
                                                                        db.SaveChanges();
                                                                        var db_EmpData = db.EmployeePayroll.Where(e => e.Id == EmpPay.Id).SingleOrDefault();
                                                                        db_EmpData.CPIEntryT = CPIEntry_l;
                                                                        db.EmployeePayroll.Attach(db_EmpData);
                                                                        db.Entry(db_EmpData).State = System.Data.Entity.EntityState.Modified;



                                                                        //db.CPIEntryT.Add(newCpi);
                                                                        //db.SaveChanges();

                                                                        //List<CPIEntryT> CPIEntry_L = new List<CPIEntryT>();
                                                                        //CPIEntry_L.Add(CPIEntryT);

                                                                        //EmpPay_Info.CPIEntryT = CPIEntry_L;
                                                                        //db.EmployeePayroll.Attach(EmpPay_Info);
                                                                        //db.Entry(EmpPay_Info).State = System.Data.Entity.EntityState.Modified;
                                                                        //ErrorList = ValidateObj(EmpPay_Info);
                                                                        //if (ErrorList.Count > 0)
                                                                        //{
                                                                        //    Msg.Add(string.Join(",", ErrorList));
                                                                        //    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                                                        //}
                                                                        db.SaveChanges();
                                                                        db.Entry(db_EmpData).State = System.Data.Entity.EntityState.Detached;
                                                                        SalaryHeadGenProcess.EmployeeSalaryStructCreationForCPI(EmpPay_Info, EffectiveDate, pagree);
                                                                        newCpi = null;
                                                                        ts.Complete();
                                                                        //break;
                                                                    }
                                                                }
                                                                //  }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    watch.Stop();
                                    Utility.DumpProcessStatus(watch.Elapsed.Hours + ":" + watch.Elapsed.Minutes + ":" + watch.Elapsed.Seconds + ":" + watch.Elapsed.Milliseconds);
                                    Msg.Add("  Data Updated successfully  ");
                                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
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
                            List<string> Msgn = new List<string>();
                            Msgn.Add("Record modified by another user.So refresh it and try to save again.");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msgn }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                        }
                    }

                    return View();
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

        #region DELETE

        [HttpPost]
        public ActionResult Delete(int data)
        {
            List<string> Msg = new List<string>();
            try
            {
                using (DataBaseContext db = new DataBaseContext())
                {

                    CPIEntryT CPIEntryT = db.CPIEntryT.Include(e => e.PayScale)
                                                       .Where(e => e.Id == data).SingleOrDefault();

                    string PrevMonth = Convert.ToDateTime("01/" + CPIEntryT.PayMonth).AddMonths(-1).ToString("MM/yyyy");
                    if (db.SalaryT.Any(o => o.PayMonth == CPIEntryT.PayMonth && o.ReleaseDate != null))
                    {
                        List<string> Msgu = new List<string>();
                        Msgu.Add(" You can't delete CPI as this month salary is released. ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msgu }, JsonRequestBehavior.AllowGet);
                    }


                    var CPIMonthData = db.CPIEntryT.Where(e => e.PayMonth == CPIEntryT.PayMonth).ToList();

                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {

                        try
                        {
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now,
                                CreatedBy = CPIEntryT.DBTrack.CreatedBy != null ? CPIEntryT.DBTrack.CreatedBy : null,
                                CreatedOn = CPIEntryT.DBTrack.CreatedOn != null ? CPIEntryT.DBTrack.CreatedOn : null,
                                IsModified = CPIEntryT.DBTrack.IsModified == true ? false : false//,
                            };


                            foreach (var a in CPIMonthData.ToList())
                            {
                                db.Entry(a).State = System.Data.Entity.EntityState.Deleted;
                            }


                            db.SaveChanges();
                            ts.Complete();
                            // Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                        catch (RetryLimitExceededException /* dex */)
                        {
                            Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
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

        #region AUTH SAVE
        #endregion
        #endregion

        #region LookupDetails
        [HttpPost]
        public ActionResult GetPayscaleDetails(List<int> SkipIds)
        {

            using (DataBaseContext db = new DataBaseContext())
            {
                //List<string> Ids = SkipIds.ToString();
                var fall = db.PayScale.Include(e => e.PayScaleArea).Include(e => e.PayScaleType).ToList();



                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.PayScale.Include(e => e.PayScaleArea).Include(e => e.PayScaleType).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }

                var list1 = db.CPIEntryT.ToList().Select(e => e.PayScale);
                var list2 = fall.Except(list1);

                //  var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.LookupVal }).Distinct();
                var r = (from ca in list2 select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region P2BGRID

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
                IEnumerable<CPIEntryT> CPIEntryT = null;
                if (gp.IsAutho == true)
                {
                    CPIEntryT = db.CPIEntryT.Where(e => e.DBTrack.IsModified == true).GroupBy(e => e.PayMonth, (key, e) => e.FirstOrDefault());
                }
                else
                {
                    CPIEntryT = db.CPIEntryT.GroupBy(e => e.PayMonth, (key, e) => e.FirstOrDefault());
                }

                IEnumerable<CPIEntryT> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = CPIEntryT;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Select(a => new { a.ActualIndexPoint, a.PayMonth, a.Id }).Where(e => (e.Id.ToString().Contains(gp.searchString)) || (e.ActualIndexPoint.ToString().Contains(gp.searchString)) || (e.PayMonth.ToString().Contains(gp.searchString))).ToList();
                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        //jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.ActualIndexPoint, a.PayMonth, a.PayScale != null ? Convert.ToString(a.PayScale.FullDetails) : "" }).ToList();
                        jsonData = IE.Where(e => (e.Id.ToString().Contains(gp.searchString))
                                   || (e.PayMonth.ToUpper().Contains(gp.searchString.ToUpper()))
                                   || (e.ActualIndexPoint.ToString().Contains(gp.searchString))
                            //|| (e.PayScale.FullDetails.ToUpper().Contains(gp.searchString.ToUpper()))
                                   ).Select(a => new Object[] { Convert.ToString(a.ActualIndexPoint), Convert.ToString(a.PayMonth), a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = CPIEntryT;
                    Func<CPIEntryT, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "ActualIndexPoint" ? c.ActualIndexPoint.ToString() :
                                         gp.sidx == "PayMonth" ? c.PayMonth :
                                         gp.sidx == "PayScale" ? c.PayScale.FullDetails : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.ActualIndexPoint), Convert.ToString(a.PayMonth), a.Id, a.PayScale != null ? Convert.ToString(a.PayScale.FullDetails) : "" }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { Convert.ToString(a.ActualIndexPoint), Convert.ToString(a.PayMonth), a.Id, a.PayScale != null ? Convert.ToString(a.PayScale.FullDetails) : "" }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.ActualIndexPoint), Convert.ToString(a.PayMonth), a.Id, a.PayScale != null ? Convert.ToString(a.PayScale.FullDetails) : "" }).ToList();
                    }
                    totalRecords = CPIEntryT.Count();
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

        #endregion

    }
}