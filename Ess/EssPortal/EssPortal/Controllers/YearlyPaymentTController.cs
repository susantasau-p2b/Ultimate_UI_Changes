using P2b.Global;
using EssPortal.App_Start;
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
using EssPortal.Models;
using System.Web.Script.Serialization;
using System.Threading.Tasks;
using System.Collections;
using System.Data.Entity.Core.Objects;
using System.Reflection;
using Payroll;
using Leave;
using EssPortal.Process;
using System.Diagnostics;
using EssPortal.Security;


namespace P2BUltimate.Controllers.Payroll.MainController
{
    public class YearlyPaymentTController : Controller
    {
        //private DataBaseContext db = new DataBaseContext();

        public ActionResult Index()
        {
            return View("~/Views/YearlyPaymentT/Index.cshtml");
        }

        public List<int> one_ids(string form)
        {
            string itsec_id = form;
            List<int> ids = itsec_id.Split(',').Select(e => int.Parse(e)).ToList();
            return (ids);
        }
        public ActionResult partial()
        {
            return View("~/Views/Shared/_YearlypaymentView.cshtml");

        }

        //public ActionResult EmpLoad(ParamModel pm)
        //{
        //    return View();
        //}
        #region Create

        public ActionResult Create(YearlyPaymentT Y, FormCollection form, String forwarddata) //Create submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string SalaryHeadlist = form["SalaryHeadlist"] == "0" ? "" : form["SalaryHeadlist"];
                    string FromPeriod = form["FromPeriod"] == "0" ? "" : form["FromPeriod"];
                    string ToPeriod = form["ToPeriod"] == "0" ? "" : form["ToPeriod"];
                    string ProcessMonth = form["ProcessMonth"] == "0" ? "" : form["ProcessMonth"];
                    string AmountPaid = form["AmountPaid"] == "0" ? "" : form["AmountPaid"];
                    string TDSAmount = form["TDSAmount"] == "0" ? "" : form["TDSAmount"];
                    string OtherDeduction = form["OtherDeduction"] == "0" ? "" : form["OtherDeduction"];
                    string LvEncashReq = form["LvEncashReq"] == "0" ? "" : form["LvEncashReq"];
                    string Narration = form["Narration"] == "0" ? "" : form["Narration"];
                    var Emp = Convert.ToInt32(SessionManager.EmpId);

                    var fyyr = db.Calendar.Include(a => a.Name).Where(a => a.Name.LookupVal.ToUpper() == "FINANCIALYEAR" && a.Default == true).SingleOrDefault();
                    Y.FinancialYear = fyyr;

                    if (SalaryHeadlist != null && SalaryHeadlist != "")
                    {
                        var Val = db.SalaryHead.Find(int.Parse(SalaryHeadlist));
                        Y.SalaryHead = Val;
                    }
                    if (FromPeriod != null && FromPeriod != "")
                    {
                        var Val = DateTime.Parse(FromPeriod);
                        Y.FromPeriod = Val;
                    }
                    if (ToPeriod != null && ToPeriod != "")
                    {
                        var Val = DateTime.Parse(ToPeriod);
                        Y.ToPeriod = Val;
                    }
                    if (ProcessMonth != null && ProcessMonth != "")
                    {
                        var Val = ProcessMonth;
                        Y.ProcessMonth = Val;
                    }
                    if (AmountPaid != null && AmountPaid != "")
                    {
                        var Val = double.Parse(AmountPaid);
                        Y.AmountPaid = Val;
                    }
                    if (TDSAmount != null && TDSAmount != "")
                    {
                        var Val = double.Parse(TDSAmount);
                        Y.TDSAmount = Val;
                    }
                    if (OtherDeduction != null && OtherDeduction != "")
                    {
                        var Val = double.Parse(OtherDeduction);
                        Y.OtherDeduction = Val;
                    }
                    if (LvEncashReq != null && LvEncashReq != "")
                    {
                        var Val = db.LvEncashReq.Find(int.Parse(LvEncashReq));
                        Y.LvEncashReq = Val;
                    }
                    if (Narration != null && Narration != "")
                    {

                        var val = Narration;
                        Y.Narration = Narration;
                    }

                    //else
                    //{
                    //    return Json(new { sucess = false, responseText = "Kindly select employee" }, JsonRequestBehavior.AllowGet);
                    //}

                    Employee OEmployee = null;
                    EmployeePayroll OEmployeePayroll = null;

                    Y.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                    YearlyPaymentT ObjYPT = new YearlyPaymentT();
                    {
                        ObjYPT.SalaryHead = Y.SalaryHead;
                        ObjYPT.AmountPaid = Y.AmountPaid;
                        ObjYPT.FromPeriod = Y.FromPeriod;
                        ObjYPT.ToPeriod = Y.ToPeriod;
                        ObjYPT.ProcessMonth = Y.ProcessMonth;
                        ObjYPT.TDSAmount = Y.TDSAmount;
                        ObjYPT.LvEncashReq = Y.LvEncashReq;
                        ObjYPT.OtherDeduction = Y.OtherDeduction;
                        ObjYPT.Narration = Y.Narration;
                        ObjYPT.FinancialYear = Y.FinancialYear;
                        ObjYPT.DBTrack = Y.DBTrack;

                    }
                    if (ModelState.IsValid)
                    {
                        OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct)
                                    .Where(r => r.Id == Emp).SingleOrDefault();

                        OEmployeePayroll
                        = db.EmployeePayroll
                      .Where(e => e.Employee.Id == OEmployee.Id).SingleOrDefault();


                        ObjYPT.GeoStruct = db.GeoStruct.Find(OEmployee.GeoStruct.Id);


                        ObjYPT.FuncStruct = db.FuncStruct.Find(OEmployee.FuncStruct.Id);


                        ObjYPT.PayStruct = db.PayStruct.Find(OEmployee.PayStruct.Id == null ? 0 : OEmployee.PayStruct.Id);

                        using (TransactionScope ts = new TransactionScope())
                        {
                            try
                            {
                                db.YearlyPaymentT.Add(ObjYPT);
                                db.SaveChanges();
                                List<YearlyPaymentT> OFAT = new List<YearlyPaymentT>();
                                OFAT.Add(db.YearlyPaymentT.Find(ObjYPT.Id));

                                if (OEmployeePayroll == null)
                                {
                                    EmployeePayroll OTEP = new EmployeePayroll()
                                    {
                                        Employee = db.Employee.Find(OEmployee.Id),
                                        YearlyPaymentT = OFAT,
                                        DBTrack = Y.DBTrack

                                    };


                                    db.EmployeePayroll.Add(OTEP);
                                    db.SaveChanges();
                                }
                                else
                                {
                                    var aa = db.EmployeePayroll.Find(OEmployeePayroll.Id);
                                    aa.YearlyPaymentT = OFAT;
                                    //OEmployeePayroll.DBTrack = dbt;
                                    db.EmployeePayroll.Attach(aa);
                                    db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    db.Entry(aa).State = System.Data.Entity.EntityState.Detached;

                                }
                                ts.Complete();
                                //return Json(new { sucess = true, responseText = "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
                                List<string> Msgs = new List<string>();
                                Msgs.Add("  Data Saved successfully  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msgs }, JsonRequestBehavior.AllowGet);
                            }
                            catch (DataException ex)
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
                                return Json(new { sucess = false, responseText = ex.InnerException }, JsonRequestBehavior.AllowGet);
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
                        List<string> Msgu = new List<string>();
                        Msgu.Add("  Unable to create...  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msgu }, JsonRequestBehavior.AllowGet);
                        // return this.Json(new Object[] { "", "", "Unable to create...", JsonRequestBehavior.AllowGet });
                    }
                    else
                    {
                        StringBuilder sb = new StringBuilder("");
                        foreach (ModelState modelState in ModelState.Values)
                        {
                            foreach (ModelError error in modelState.Errors)
                            {
                                sb.Append(error.ErrorMessage);
                                sb.Append("." + "/n");
                            }
                        }
                        List<string> MsgB = new List<string>();
                        var errorMsg = sb.ToString();
                        MsgB.Add(errorMsg);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = MsgB }, JsonRequestBehavior.AllowGet);

                        //var errorMsg = sb.ToString();
                        //return this.Json(new Object[] { "", "", errorMsg, JsonRequestBehavior.AllowGet });
                        //return this.Json(new { msg = errorMsg });
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

        #endregion

        #region Release Data

        public JsonResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.YearlyPaymentT
                    .Include(e => e.LvEncashReq)
                    .Include(e => e.SalaryHead)
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        FromPeriod = e.FromPeriod,
                        ToPeriod = e.ToPeriod,
                        ProcessMonth = e.ProcessMonth,
                        PayMonth = e.PayMonth,
                        AmountPaid = e.AmountPaid,
                        TDSAmount = e.TDSAmount,
                        OtherDeduction = e.OtherDeduction,
                        ReleaseFlag = e.ReleaseFlag,
                        ReleaseDate = e.ReleaseDate,
                        Narration = e.Narration,
                        Salaryhead_Id = e.SalaryHead.Id == null ? 0 : e.SalaryHead.Id,
                        Action = e.DBTrack.Action
                    }).ToList();

                var add_data = db.YearlyPaymentT
                  .Include(e => e.LvEncashReq)
                    .Where(e => e.Id == data)
                    .Select(e => new
                    {
                        Lvenchreq_FullDetails = e.LvEncashReq.EncashDays == null ? 0 : e.LvEncashReq.EncashDays,
                        Lvenchreq_Id = e.LvEncashReq.Id == null ? "" : e.LvEncashReq.Id.ToString(),

                    }).ToList();


                var yearlypymentT = db.YearlyPaymentT.Find(data);
                Session["RowVersion"] = yearlypymentT.RowVersion;
                var Auth = yearlypymentT.DBTrack.IsModified;
                //return Json(new Object[] { Q, add_data, "", Auth, JsonRequestBehavior.AllowGet });
                return Json(new Object[] { Q, add_data, Auth }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult EditGridDetails(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.YearlyPaymentT
                     .Include(e => e.LvEncashReq)
                     .Include(e => e.SalaryHead)
                     .Where(e => e.Id == data).Select
                     (e => new
                     {
                         FromPeriodP = e.FromPeriod,
                         ToPeriodP = e.ToPeriod,
                         ProcessMonthP = e.ProcessMonth,
                         AmountPaidP = e.AmountPaid,
                         TDSAmountP = e.TDSAmount,
                         OtherDeductionP = e.OtherDeduction,
                         ReleaseFlagP = e.ReleaseFlag,
                         ReleaseDateP = e.ReleaseDate,
                         NarrationP = e.Narration,
                         Salaryhead_IdP = e.SalaryHead.Id == null ? 0 : e.SalaryHead.Id,
                         Action = e.DBTrack.Action
                     }).ToList();
                var yearlypymentT = db.YearlyPaymentT.Find(data);
                Session["RowVersion"] = yearlypymentT.RowVersion;
                var Auth = yearlypymentT.DBTrack.IsModified;
                //return Json(new Object[] { Q, add_data, "", Auth, JsonRequestBehavior.AllowGet });
                return Json(new { data = Q }, JsonRequestBehavior.AllowGet);
            }
        }



        public async Task<ActionResult> EditSave(string forwarddata, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string PayMonth = form["PayMonth"] == "0" ? "" : form["PayMonth"];
                    string ReleaseDate = form["ReleaseDate"] == "0" ? "" : form["ReleaseDate"];
                    string ProcessFlag = form["ProcessFlag"] == "0" ? "" : form["ProcessFlag"];
                    if (ReleaseDate == null)
                    {
                        Msg.Add("Enter Realease Date ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }
                    if (PayMonth == null)
                    {
                        Msg.Add("Enter Pay Month ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }
                    List<int> ids = null;
                    if (forwarddata != null && forwarddata != "0" && forwarddata != "false")
                    {
                        ids = one_ids(forwarddata);
                    }
                    try
                    {

                        //DbContextTransaction transaction = db.Database.BeginTransaction();

                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            foreach (var ca in ids)
                            {
                                YearlyPaymentT YearlyPayT = db.YearlyPaymentT.Find(ca);
                                YearlyPayT.ReleaseDate = Convert.ToDateTime(ReleaseDate);
                                YearlyPayT.ReleaseFlag = Convert.ToBoolean(ProcessFlag);
                                YearlyPayT.PayMonth = PayMonth;
                                db.YearlyPaymentT.Attach(YearlyPayT);
                                db.Entry(YearlyPayT).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(YearlyPayT).State = System.Data.Entity.EntityState.Detached;
                            }

                            ts.Complete();
                            List<string> Msgs = new List<string>();
                            Msgs.Add(" Record Updated  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msgs }, JsonRequestBehavior.AllowGet);

                            // return Json(new Object[] { "", "", "Record Updated", JsonRequestBehavior.AllowGet });

                        }
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        var entry = ex.Entries.Single();
                        var clientValues = (YearlyPaymentT)entry.Entity;
                        var databaseEntry = entry.GetDatabaseValues();
                        if (databaseEntry == null)
                        {
                            Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                        }
                        else
                        {
                            var databaseValues = (YearlyPaymentT)databaseEntry.ToObject();
                            // Y.RowVersion = databaseValues.RowVersion;

                        }
                        List<string> Msgn = new List<string>();
                        Msgn.Add("Record modified by another user.So refresh it and try to save again.");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msgn }, JsonRequestBehavior.AllowGet);
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

        #endregion


        public ActionResult PopulateDropDownList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var qurey = db.SalaryHead.Where(e => e.Frequency.LookupVal.ToUpper() == "YEARLY" && e.SalHeadOperationType.LookupVal.ToUpper() != "PERK").ToList();

                // var qurey = db.SalaryHead.Where(e => e.SalHeadOperationType.LookupVal.ToUpper() == "LVENCASH").ToList();

                var selected = (Object)null;
                if (data2 != "" && data != "0" && data2 != "0")
                {
                    selected = Convert.ToInt32(data2);
                }

                SelectList s = new SelectList(qurey, "Id", "Name", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetlvencashreqDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.LvEncashReq.ToList();
                IEnumerable<LvEncashReq> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.LvEncashReq.ToList().Where(d => d.FullDetails.Contains(data));
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
        //public JsonResult GetSalHead(string data)
        //{
        //    int Id = int.Parse(data);
        //    var qurey = db.SalaryHead.Where(e => e.Frequency.LookupVal.ToUpper() == "YEARLY").Where(e=>e.Id == Id).ToList();
        //    var a = db.SalaryHead.Include(e => e.SalHeadOperationType).Where(e => e.Id == Id).SingleOrDefault();
        //    bool selected = false;
        //    if (a.SalHeadOperationType.LookupVal.ToUpper() == "LVENCASH")
        //    {
        //        selected = true;
        //    }
        //    return Json(selected, JsonRequestBehavior.AllowGet);
        //}
        public JsonResult GetSalHead(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                int Id = int.Parse(data);
                var qurey = db.SalaryHead.Where(e => e.Frequency.LookupVal.ToUpper() == "YEARLY" && e.SalHeadOperationType.LookupVal.ToUpper() != "PERK").Where(e => e.Id == Id).SingleOrDefault();
                var a = db.SalaryHead.Include(e => e.SalHeadOperationType).Where(e => e.Id == Id).SingleOrDefault();
                bool selected = false;
                if (qurey != null && qurey.Name == "LEAVE ENCASH")
                {
                    selected = true;
                }
                return Json(selected, JsonRequestBehavior.AllowGet);
            }
        }

        public class P2bYearlypaymentGridData
        {
            public int Id { get; set; }
            public Employee Employee { get; set; }
            public SalaryHead SalaryHead { get; set; }
            public DateTime? FromPeriod { get; set; }
            public DateTime? ToPeriod { get; set; }
            public string ProcessMonth { get; set; }
            public double AmountPaid { get; set; }
            public double TDSAmount { get; set; }
            public string Narration { get; set; }
            public double OtherDeduction { get; set; }

        }
        public class returndatagridclass
        {
            public string Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
        }


        public ActionResult GetMyYearlyPaymentTData(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string authority = Convert.ToString(Session["auho"]);
                var WfStatus = new List<Int32>();
                var SanctionStatus = new List<Int32>();
                var ApporvalStatus = new List<Int32>();
                if (authority.ToUpper() == "SANCTION")
                {
                    WfStatus.Add(1);
                    WfStatus.Add(2);
                }
                else if (authority.ToUpper() == "APPROVAL")
                {
                    WfStatus.Add(1);
                    WfStatus.Add(2);
                }
                else if (authority.ToUpper() == "MYSELF")
                {
                    SanctionStatus.Add(1);
                    SanctionStatus.Add(2);

                    ApporvalStatus.Add(3);
                    ApporvalStatus.Add(4);
                }
                var Emp = Convert.ToInt32(SessionManager.EmpId);
                var ids = Utility.StringIdsToListString(data);
                var id = Convert.ToInt32(ids[0]);
                var Q = db.YearlyPaymentT
                     .Include(e => e.LvEncashReq)
                     .Include(e => e.SalaryHead)
                     .Where(e => e.Id == id).AsEnumerable().Select
                     (e => new
                     {
                         FromPeriod = e.FromPeriod != null ? e.FromPeriod.Value.ToShortDateString() : null,
                         ToPeriod = e.ToPeriod != null ? e.ToPeriod.Value.ToShortDateString() : null,
                         //FromPeriod = e.FromPeriod.Value.ToShortDateString,
                         //ToPeriod = e.ToPeriod.Value,
                         ProcessMonth = e.ProcessMonth,
                         PayMonth = e.PayMonth,
                         AmountPaid = e.AmountPaid,
                         TDSAmount = e.TDSAmount,
                         OtherDeduction = e.OtherDeduction,
                         ReleaseFlag = e.ReleaseFlag,
                         //ReleaseDate = e.ReleaseDate.Value.ToShortDateString(),
                         Narration = e.Narration,
                         SalaryHead = e.SalaryHead == null ? "" : e.SalaryHead.Name,
                         Action = e.DBTrack.Action
                     }).ToList();


                return Json(Q, JsonRequestBehavior.AllowGet);
            }
        }

       
        public ActionResult Get_YearlyPayment(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var db_data = db.EmployeePayroll
                        .Include(e => e.YearlyPaymentT)
                        .Where(e => e.Id == data).ToList();
                    if (db_data.Count > 0)
                    {
                        List<YearlyPaymentChildDataClass> returndata = new List<YearlyPaymentChildDataClass>();
                        foreach (var item in db_data.SelectMany(e => e.YearlyPaymentT))
                        {
                            returndata.Add(new YearlyPaymentChildDataClass
                            {
                                Id = item.Id.ToString(),
                                AmountPaid = item.AmountPaid.ToString(),
                                OtherDeduction = item.OtherDeduction.ToString(),
                                Narration = item.Narration,
                                TDSAmount = item.TDSAmount.ToString()
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

        public class GetTestItInvestmentClass
        {
            public string SalaryHead { get; set; }
            public string AmountPaid { get; set; }
            public string TDSAmount { get; set; }
            public string OtherDeduction { get; set; }
            public string Net { get; set; }
            public string YearlyPaymentId { get; set; }
            public string FromPeriod { get; set; }
            public string ToPeriod { get; set; }
            public string PayMonth { get; set; }
            public string ReleaseDate { get; set; }
           // public string Status { get; set; }
            public DateTime Paymnthdate { get; set; }
            public ChildGetLvNewReqClass RowData { get; set; }
            public ChildGetLvNewReqClass ResultRowData { get; set; }
        }
        public class GetItInvestmentClass
        {
            public string SalaryHead { get; set; }
            public string AmountPaid { get; set; }
            public string TDSAmount { get; set; }
            public string OtherDeduction { get; set; }
            public string Net { get; set; }
            public string YearlyPaymentId { get; set; }
            public string FromPeriod { get; set; }
            public string ToPeriod { get; set; }
            public string PayMonth { get; set; }
            public string ReleaseDate { get; set; }                     
            public ChildGetLvNewReqClass RowData { get; set; }
            public ChildGetLvNewReqClass ResultRowData { get; set; }
        }

        public class ChildGetLvNewReqClass
        {
            public string LvNewReq { get; set; }
            public string EmpLVid { get; set; }
            public string Status { get; set; }
            public string LvHead_Id { get; set; }
            public string IsClose { get; set; }
        }

        public ActionResult GetMyYearlypayment()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (string.IsNullOrEmpty(Convert.ToString(SessionManager.EmpLvId)))
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                int Emp = Convert.ToInt32(SessionManager.EmpId);
                int OEmployee = db.Employee.AsNoTracking().Where(r => r.Id == Emp).Select(q => q.Id).SingleOrDefault();

                var db_data = db.EmployeePayroll
                          .Include(e => e.YearlyPaymentT)
                          .Include(e => e.YearlyPaymentT.Select(q => q.SalaryHead))
                          .Where(e => e.Employee.Id == OEmployee).SingleOrDefault();
                //.OrderBy(r => r.YearlyPaymentT.Select(s =>s.SalaryHead)).ThenByDescending(r =>r.YearlyPaymentT.Select(s =>s.ReleaseDate))
                List<GetItInvestmentClass> returndata = new List<GetItInvestmentClass>();
                List<GetTestItInvestmentClass> resultdata = new List<GetTestItInvestmentClass>();
                returndata.Add(new GetItInvestmentClass
                {
                   // YearlyPaymentId = "Id",
                    SalaryHead = "Sal Head",
                    PayMonth = "Pay Month",
                    ReleaseDate ="Release Date",
                    AmountPaid = "Amt Paid",
                    TDSAmount = "TDS Amount",
                    OtherDeduction = "OtherDeduction",
                    Net = "Net",
                    FromPeriod = "FromPeriod",
                    ToPeriod = "ToPeriod"
                   // Status = "Status",
                });

                if (db_data != null && db_data.YearlyPaymentT.Count() > 0)
                {                                          //NKGSB requirment orderby salaryhead then OrderByDescending release date.
                    foreach (var item in db_data.YearlyPaymentT.OrderByDescending(r =>r.ReleaseDate))
                    {
                        resultdata.Add(new GetTestItInvestmentClass
                        {
                            ResultRowData = new ChildGetLvNewReqClass
                            {
                                LvNewReq = item.Id.ToString(),
                                EmpLVid = Emp.ToString(),
                                IsClose = "",
                                Status = "",
                                LvHead_Id = "",
                            },
                            SalaryHead = item.SalaryHead != null ? item.SalaryHead.Name.ToString() : "",
                            YearlyPaymentId = item.Id.ToString(),
                            AmountPaid = item.AmountPaid.ToString(),
                            TDSAmount = item.TDSAmount.ToString(),
                            OtherDeduction = item.OtherDeduction.ToString(),
                            Net=Convert.ToString(item.AmountPaid-item.TDSAmount-item.OtherDeduction),
                            FromPeriod = item.FromPeriod.Value.ToShortDateString(),
                            ToPeriod = item.ToPeriod.Value.ToShortDateString(),
                           // OtherDeduction = item.OtherDeduction.ToString(),
                            PayMonth = item.PayMonth == null ? "" : item.PayMonth,
                            ReleaseDate = item.ReleaseDate == null ? "": item.ReleaseDate.Value.ToShortDateString(),
                            //Status = item.WFStatus != null && new[] { "2", "4" }.Contains(item.WFStatus.LookupVal) == true ? "0" : "1",
                            Paymnthdate = item.PayMonth == null ? DateTime.Now : Convert.ToDateTime("01/"+item.PayMonth),
                        });
                    }
                    //var result = resultdata.OrderByDescending(r => r.Paymnthdate).ToList();

                    //NKGSB requirment orderby salaryhead then OrderByDescending release date.
                    var result = resultdata.OrderBy(r => r.SalaryHead).ToList();

                    foreach (var item2 in result)
                    {
                        returndata.Add(new GetItInvestmentClass
                        {
                            RowData = new ChildGetLvNewReqClass
                            {
                                LvNewReq = item2.YearlyPaymentId,
                                EmpLVid = Emp.ToString(),
                                IsClose = "",
                                Status = "",
                                LvHead_Id = "",
                            },
                            SalaryHead = item2.SalaryHead,
                            // YearlyPaymentId = item.Id.ToString(),
                            AmountPaid = item2.AmountPaid.ToString(),
                            TDSAmount = item2.TDSAmount.ToString(),
                            OtherDeduction = item2.OtherDeduction.ToString(),
                            Net=item2.Net,
                            FromPeriod = item2.FromPeriod,
                            ToPeriod = item2.ToPeriod,
                           // OtherDeduction = item.OtherDeduction.ToString(),
                            PayMonth = item2.PayMonth,
                            ReleaseDate = item2.ReleaseDate,
                            //Status = item.WFStatus != null && new[] { "2", "4" }.Contains(item.WFStatus.LookupVal) == true ? "0" : "1",                           
                        });
                    }
                    return Json(new { status = true, data = returndata }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = false, data = returndata }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public class empgriddetails
        {
            public int Id { get; set; }
            public string EmployeeCode { get; set; }
            public string EmployeeName { get; set; }
        }

        public class YearlyPaymentChildDataClass
        {
            public String Id { get; set; }
            public String TDSAmount { get; set; }
            public String AmountPaid { get; set; }
            public String OtherDeduction { get; set; }
            public String Narration { get; set; }
        }

        public ActionResult GridEditSave(YearlyPaymentT ypay, FormCollection form, string data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var AmountPaid = form["yearlypayment-AmountPaid"] == " 0" ? "" : form["yearlypayment-AmountPaid"];
                    ypay.AmountPaid = Convert.ToDouble(AmountPaid);
                    var TDSAmount = form["yearlypayment-TDSAmount"] == " 0" ? "" : form["yearlypayment-TDSAmount"];
                    ypay.TDSAmount = Convert.ToDouble(TDSAmount);
                    var OtherDeduction = form["yearlypayment-OtherDeduction"] == " 0" ? "" : form["yearlypayment-OtherDeduction"];
                    ypay.OtherDeduction = Convert.ToDouble(OtherDeduction);
                    if (data != null)
                    {
                        var id = Convert.ToInt32(data);
                        var db_data = db.YearlyPaymentT.Where(e => e.Id == id).SingleOrDefault();
                        db_data.AmountPaid = ypay.AmountPaid;
                        db_data.TDSAmount = ypay.TDSAmount;
                        db_data.OtherDeduction = ypay.OtherDeduction;
                        try
                        {
                            db.YearlyPaymentT.Attach(db_data);
                            db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;
                            return this.Json(new { status = true, responseText = "Record Updated Successfully.", JsonRequestBehavior.AllowGet });
                            //Msg.Add("  Record Updated");
                            //return Json(new Utility.JsonReturnClass { data = db_data.ToString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            // return Json(new { data = db_data, responseText = "Record Updated..!" }, JsonRequestBehavior.AllowGet);
                        }
                        catch (Exception e)
                        {

                            throw e;
                        }
                    }
                    else
                    {
                        return this.Json(new { status = false, responseText = "  Data Is Null", JsonRequestBehavior.AllowGet });
                        //     Msg.Add("  Data Is Null  ");
                        //return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //return Json(new { responseText = "Data Is Null" }, JsonRequestBehavior.AllowGet);
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


        public double YearlyCal(SalaryHead SalHd, Employee Emp, string PayMonth, DateTime? FromPeriod, DateTime? ToPeriod)
        {
            double TotAmt = 0;
            using (DataBaseContext db = new DataBaseContext())
            {
                SalaryHead SalHead = db.SalaryHead.Include(e => e.ProcessType).Where(e => e.Id == SalHd.Id).SingleOrDefault();

                var OEmployeePayroll = db.EmployeePayroll
                    .Where(e => e.Employee.Id == Emp.Id).Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails))
                    .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead)))
                      .SingleOrDefault();


                var OEmpSalStruct = OEmployeePayroll.EmpSalStruct.OrderByDescending(e => e.EffectiveDate).Select(e => e.EmpSalStructDetails.Where(r => r.SalaryHead.Id == SalHd.Id)).FirstOrDefault();

                var LWPDaysQuery = db.EmployeePayroll
                .Where(e => e.Employee.Id == Emp.Id).Select(e => e.SalAttendance.Where(r => r.PayMonth == PayMonth)).SingleOrDefault();

                double LWPDays = LWPDaysQuery.Select(e => e.LWPDays).SingleOrDefault();

                if (SalHead.ProcessType.LookupVal.ToString().ToUpper() == "REGULAR")
                {
                    if (SalHead.OnAttend == true)
                    {
                        foreach (var s in OEmpSalStruct)
                        {
                            TotAmt = s.Amount;
                        }
                    }
                    else
                    {
                        foreach (var s in OEmpSalStruct)
                        {
                            TotAmt = s.Amount;
                        }
                    }
                }
                else if (SalHead.ProcessType.LookupVal.ToString().ToUpper() == "EARNED")
                {
                    if (SalHead.OnAttend == true)
                    {

                    }
                    else
                    {
                        foreach (var s in OEmpSalStruct)
                        {
                            TotAmt = s.Amount;
                        }
                    }
                }
                else if (SalHead.ProcessType.LookupVal.ToString().ToUpper() == "FIXEDMONTH")
                {
                    if (SalHead.OnAttend == true)
                    {
                        int TotDays = Convert.ToDateTime("01/" + PayMonth).DayOfYear;
                        foreach (var s in OEmpSalStruct)
                        {
                            TotAmt = s.Amount * LWPDays / TotDays;
                        }
                    }
                    else
                    {
                        foreach (var s in OEmpSalStruct)
                        {
                            TotAmt = s.Amount;
                        }
                    }
                }
                else if (SalHead.ProcessType.LookupVal.ToString().ToUpper() == "PRORATA")
                {
                    if (SalHead.OnAttend == true)
                    {
                        while (FromPeriod <= ToPeriod)
                        {
                            PayMonth = FromPeriod.Value.Month + "/" + FromPeriod.Value.Year;
                            LWPDaysQuery = db.EmployeePayroll
                            .Where(e => e.Employee.Id == Emp.Id).Select(e => e.SalAttendance.Where(r => r.PayMonth == PayMonth)).SingleOrDefault();

                            LWPDays = LWPDaysQuery.Select(e => e.LWPDays).SingleOrDefault();

                            int TotDays = DateTime.DaysInMonth(Convert.ToInt32(PayMonth.Split('/')[1]), Convert.ToInt32(PayMonth.Split('/')[0]));
                            OEmpSalStruct = OEmployeePayroll.EmpSalStruct.Select(e => e.EmpSalStructDetails.Where(r => r.SalaryHead.Id == SalHd.Id)).FirstOrDefault();
                            foreach (var s in OEmpSalStruct)
                            {
                                TotAmt = TotAmt + s.Amount * LWPDays / TotDays;
                            }
                            FromPeriod = FromPeriod.Value.AddMonths(1);
                        }

                    }
                    else
                    {
                        while (FromPeriod <= ToPeriod)
                        {
                            PayMonth = FromPeriod.Value.Month + "/" + FromPeriod.Value.Year;

                            OEmpSalStruct = OEmployeePayroll.EmpSalStruct.Select(e => e.EmpSalStructDetails.Where(r => r.SalaryHead.Id == SalHd.Id)).FirstOrDefault();
                            foreach (var s in OEmpSalStruct)
                            {
                                TotAmt = TotAmt + s.Amount;
                            }
                            FromPeriod = FromPeriod.Value.AddMonths(1);
                        }
                    }
                }
                return TotAmt;
            }
        }
    }
}