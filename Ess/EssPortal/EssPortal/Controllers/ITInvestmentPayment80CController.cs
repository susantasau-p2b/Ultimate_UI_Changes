using P2b.Global;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using P2BUltimate.Security;
using Payroll;
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

namespace P2BUltimate.Controllers.Payroll.MainController
{
    [AuthoriseManger]
    public class ITInvestmentPayment80CController : Controller
    {
        // private DataBaseContext db = new DataBaseContext();
        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/ITInvestmentPayment80C/Index.cshtml");
        }

        public ActionResult partial()
        {
            return View("~/Views/Shared/Payroll/_ITInvestmentPayment.cshtml");

        }


        [HttpPost]
        public ActionResult GetITInvestmentLKDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.ITSection.Include(e => e.ITSectionList)
                    .Include(e => e.ITSectionListType)
                    .Include(e => e.ITInvestments)
                    .Where(e => e.ITSectionListType.LookupVal.ToUpper() == "REBATE")
                    .SelectMany(e => e.ITInvestments)
                    .ToList();


                //if (SkipIds != null)
                //{
                //    foreach (var a in SkipIds)
                //    {
                //        if (fall == null)
                //            fall = db.ITInvestment.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                //        else
                //            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                //    }
                //}

                var r = (
                    from ca in fall

                    select new
                    {
                        srno = ca.Id,
                        lookupvalue = ca.ITInvestmentName

                    })
                    .ToList().Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetITSectionByDefault()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.ITSection.Include(e => e.ITInvestments)
                    .Include(e => e.ITSectionList)
                    .Include(e => e.ITSectionListType)
                    .Where(e => e.ITSectionListType.LookupVal.ToUpper() == "REBATE").ToList();
                var returnpara = new
                {
                    Id = fall.Select(a => a.Id.ToString()).ToArray(),
                    FullDetails = fall.Select(a => a.FullDetails).ToArray()
                };

                return Json(returnpara, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public ActionResult GetITSectionLKDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.ITSection.Include(e => e.ITInvestments)
                    .Include(e => e.ITSectionList)
                    .Include(e => e.ITSectionListType)
                    .Where(e => e.ITSectionListType.LookupVal.ToUpper() == "REBATE")
                    .ToList();

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.ITSection.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }
                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult GetLoanAdvHeadLKDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.LoanAdvanceHead.ToList();

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.LoanAdvanceHead.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }
                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult GetSubInvPayLKDetails(List<int> SkipIds)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.ITSubInvestmentPayment.ToList();

                if (SkipIds != null)
                {
                    foreach (var a in SkipIds)
                    {
                        if (fall == null)
                            fall = db.ITSubInvestmentPayment.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();
                        else
                            fall = fall.Where(e => !e.Id.ToString().Contains(a.ToString())).ToList();

                    }
                }
                var r = (from ca in fall select new { srno = ca.Id, lookupvalue = ca.FullDetails }).Distinct();

                return Json(r, JsonRequestBehavior.AllowGet);
            }
        }

        public class ITInvestPayment_Val
        {
            public Array ITSubInvestmentPayment_Id { get; set; }
            public Array ITSubInvestmentPayment_Val { get; set; }
            public string ITInvestment_Id { get; set; }
            public string ITInvestment_Val { get; set; }
            public string ITSection_Id { get; set; }
            public string ITSection_Val { get; set; }
            public string LoanAdvHead_Id { get; set; }
            public string LoanAdvHead_Val { get; set; }

        }


        //string Emp = form["employee-table"] == "0" ? "" : form["employee-table"];
        //string ITInvestmentslist = form["ITInvestmentlist"] == "0" ? "" : form["ITInvestmentlist"];
        //string ITSectionlist = form["ITSectionlist"] == "0" ? "" : form["ITSectionlist"];
        //string LoanAdvHeadlist = form["LoanAdvanceHeadlist"] == "0" ? "" : form["LoanAdvanceHeadlist"];
        //string ITSubInvestmentPaymentlist = form["ITSubInvestmentPaymentlist"] == "0" ? "" : form["ITSubInvestmentPaymentlist"];
        // List<ITSubInvestmentPayment> ObjITSubInvestmentPayment = new List<ITSubInvestmentPayment>();
        //    if (ITSubInvestmentPaymentlist != null && ITSubInvestmentPaymentlist != "")
        //    {
        //        var ids = Utility.StringIdsToListIds(ITSubInvestmentPaymentlist);
        //        foreach (var ca in ids)
        //        {
        //            var value = db.ITSubInvestmentPayment.Find(ca);
        //            ObjITSubInvestmentPayment.Add(value);
        //            ITInvest.ITSubInvestmentPayment = ObjITSubInvestmentPayment;
        //        }
        //    }


        //[HttpPost]
        //public ActionResult Create(ITInvestmentPayment ITInvest, FormCollection form)
        //{
        //    string Emp = form["employee-table"] == "0" ? "" : form["employee-table"];
        //    string ITInvestmentslist = form["ITInvestmentlist"] == "0" ? "" : form["ITInvestmentlist"];
        //    string ITSectionlist = form["ITSectionlist"] == "0" ? "" : form["ITSectionlist"];
        //    string LoanAdvHeadlist = form["LoanAdvanceHeadlist"] == "0" ? "" : form["LoanAdvanceHeadlist"];
        //    string ITSubInvestmentPaymentlist = form["ITSubInvestmentPaymentlist"] == "0" ? "" : form["ITSubInvestmentPaymentlist"];

        //    int CompId = 0;
        //    if (!String.IsNullOrEmpty(Session["CompId"].ToString()))
        //    {
        //        CompId = int.Parse(Session["CompId"].ToString());
        //    }


        //    if (Emp != null && Emp != "0" && Emp != "false")
        //    {
        //      var  ids = Utility.StringIdsToListIds(Emp);
        //    }
        //    else
        //    {
        //        return Json(new { success = false, responseText = "Kindly select employee" }, JsonRequestBehavior.AllowGet);
        //    }
        //    if (ITInvestmentslist != null && ITInvestmentslist != "")
        //    {
        //            var value = db.ITInvestment.Find(int.Parse(ITInvestmentslist));
        //            ITInvest.ITInvestment = value;
        //    }


        //    if (ITSectionlist != null && ITSectionlist != "")
        //    {
        //        var value = db.ITSection.Find(int.Parse(ITSectionlist));
        //        ITInvest.ITSection = value;
        //    }
        //    List<ITSubInvestmentPayment> ObjITSubInvestmentPayment = new List<ITSubInvestmentPayment>();
        //    if (ITSubInvestmentPaymentlist != null && ITSubInvestmentPaymentlist != "")
        //    {
        //        var ids = Utility.StringIdsToListIds(ITSubInvestmentPaymentlist);
        //        foreach (var ca in ids)
        //        {
        //            var value = db.ITSubInvestmentPayment.Find(ca);
        //            ObjITSubInvestmentPayment.Add(value);
        //            ITInvest.ITSubInvestmentPayment = ObjITSubInvestmentPayment;
        //        }
        //    }


        //    if (LoanAdvHeadlist != null && LoanAdvHeadlist != "")
        //    {
        //        var value = db.LoanAdvanceHead.Find(int.Parse(LoanAdvHeadlist));
        //        ITInvest.LoanAdvanceHead = value;
        //    }

        //    Employee OEmployee = null;
        //    EmployeePayroll OEmployeePayroll = null;
        //    CompanyPayroll OCompanyPayroll = null;
        //    if (ModelState.IsValid)
        //    {
        //        using (TransactionScope ts = new TransactionScope())
        //        {
        //            ITInvest.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
        //            ITInvestmentPayment ITInvestPayment = new ITInvestmentPayment()
        //            {
        //                ActualInvestment = ITInvest.ActualInvestment,
        //                DeclaredInvestment = ITInvest.DeclaredInvestment,
        //                FinancialYear = ITInvest.FinancialYear,
        //                ITInvestment = ITInvest.ITInvestment,
        //                InvestmentDate = ITInvest.InvestmentDate,
        //              //  ITSection = ITInvest.ITSection,
        //                ITSubInvestmentPayment = ITInvest.ITSubInvestmentPayment,
        //              //  LoanAdvanceHead = ITInvest.LoanAdvanceHead,
        //               // Narration =ITInvest.Narration,  
        //                DBTrack = ITInvest.DBTrack
        //            };
        //            try
        //            {

        //                db.ITInvestmentPayment.Add(ITInvestPayment);
        //                var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, ITInvest.DBTrack);
        //                DT_ITInvestmentPayment DT_Corp = (DT_ITInvestmentPayment)rtn_Obj;
        //                DT_Corp.ITInvestment_Id = ITInvest.ITInvestment == null ? 0 : ITInvest.ITInvestment.Id;
        //                DT_Corp.ITSection_Id = ITInvest.ITSection == null ? 0 : ITInvest.ITSection.Id;
        //                DT_Corp.LoanAdvanceHead_Id = ITInvest.LoanAdvanceHead == null ? 0 : ITInvest.LoanAdvanceHead.Id;
        //                db.Create(DT_Corp);
        //                db.SaveChanges();
        //                ts.Complete();
        //                return this.Json(new Object[] { "", "", "Data Created Successfully.", JsonRequestBehavior.AllowGet });

        //            }
        //            catch (DbUpdateConcurrencyException)
        //            {
        //                return RedirectToAction("create", new { concurrencyError = true, id = ITInvest.Id });
        //            }
        //            catch (DataException /* dex */)
        //            {
        //                return this.Json(new { msg = "Unable to edit. Try again, and if the problem persists contact your system administrator." });
        //            }
        //        }

        //    }
        //    else
        //    {
        //        StringBuilder sb = new StringBuilder("");
        //        foreach (ModelState modelState in ModelState.Values)
        //        {
        //            foreach (ModelError error in modelState.Errors)
        //            {
        //                sb.Append(error.ErrorMessage);
        //                sb.Append("." + "\n");
        //            }
        //        }
        //        var errorMsg = sb.ToString();
        //        Msg.Add(e.InnerException.Message.ToString());                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //        // return this.Json(new { msg = errorMsg });
        //    }

        //}
        public List<int> one_ids(string form)
        {
            string itsec_id = form;
            List<int> ids = itsec_id.Split(',').Select(e => int.Parse(e)).ToList();
            return (ids);
        }

        public ActionResult Create(ITInvestmentPayment ITP, FormCollection form, String forwarddata) //Create submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string Emp = form["employee-table"] == "0" ? "" : form["employee-table"];
                    string ITInvestmentslist = form["ITInvestmentlist"] == "0" ? "" : form["ITInvestmentlist"];
                    string FinancialYearList = form["FinancialYearList"] == "0" ? "" : form["FinancialYearList"];
                    string LoanAdvHeadlist = form["LoanAdvanceHeadlist"] == "0" ? "" : form["LoanAdvanceHeadlist"];
                    string ITSubInvestmentPaymentlist = form["ITSubInvestmentPaymentlist"] == "0" ? "" : form["ITSubInvestmentPaymentlist"];

                    List<int> ids = null;

                    if (Emp != null && Emp != "0" && Emp != "false")
                    {

                        ids = one_ids(Emp);
                    }
                    else
                    {
                        List<string> Msgu = new List<string>();
                        Msgu.Add("  Kindly select employee  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msgu }, JsonRequestBehavior.AllowGet);
                    }
                    if (ITInvestmentslist == null || ITInvestmentslist == "")
                    {
                        Msg.Add(" Select ITInvestment ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    var ITSectionlist = form["ITSectionListL"] == "0" ? "" : form["ITSectionListL"];

                    if (ITSectionlist != null)
                    {
                        var value = db.ITSection.Find(int.Parse(ITSectionlist));
                        ITP.ITSection = value;
                    }
                    else
                    {
                        Msg.Add("  Kindly select ITSectionlist  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    if (ITInvestmentslist != null && ITInvestmentslist != "")
                    {
                        var value = db.ITInvestment.Find(int.Parse(ITInvestmentslist));
                        ITP.ITInvestment = value;
                    }
                    if (FinancialYearList != null && FinancialYearList != "")
                    {
                        var value = db.Calendar.Find(int.Parse(FinancialYearList));
                        ITP.FinancialYear = value;
                    }


                    List<ITSubInvestmentPayment> ObjITSubInvestmentPayment = new List<ITSubInvestmentPayment>();
                    if (ITSubInvestmentPaymentlist != null && ITSubInvestmentPaymentlist != "")
                    {
                        ids = one_ids(ITSubInvestmentPaymentlist);
                        foreach (var ca in ids)
                        {
                            var value = db.ITSubInvestmentPayment.Find(ca);
                            ObjITSubInvestmentPayment.Add(value);
                            ITP.ITSubInvestmentPayment = ObjITSubInvestmentPayment;
                        }
                    }


                    Employee OEmployee = null;
                    EmployeePayroll OEmployeePayroll = null;

                    ITP.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

                    ITInvestmentPayment ObjITP = new ITInvestmentPayment();
                    {
                        ObjITP.ActualInvestment = ITP.ActualInvestment;
                        ObjITP.DeclaredInvestment = ITP.DeclaredInvestment;
                        ObjITP.FinancialYear = ITP.FinancialYear;
                        ObjITP.InvestmentDate = ITP.InvestmentDate;
                        ObjITP.ITInvestment = ITP.ITInvestment;
                        ObjITP.ITSection = ITP.ITSection;
                        ObjITP.ITSubInvestmentPayment = ITP.ITSubInvestmentPayment;
                        ObjITP.DBTrack = ITP.DBTrack;

                    }
                    if (ModelState.IsValid)
                    {

                        foreach (var i in ids)
                        {
                            OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct)
                                        .Where(r => r.Id == i).SingleOrDefault();

                            //  OEmployeePayroll= db.EmployeePayroll.Where(e => e.Employee.Id == i).SingleOrDefault();

                            OEmployeePayroll = db.EmployeePayroll.Include(e => e.ITInvestmentPayment)
                     .Include(e => e.ITInvestmentPayment.Select(r => r.FinancialYear)).Where(e => e.Employee.Id == i).SingleOrDefault();

                            var calf = OEmployeePayroll.ITInvestmentPayment.Any(e => e.FinancialYear.Id == ITP.FinancialYear.Id && e.InvestmentDate == ITP.InvestmentDate);
                            if (calf == true)
                            {
                                Msg.Add("  Data Already Exist For THis Employee.  ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                            using (TransactionScope ts = new TransactionScope())
                            {
                                try
                                {
                                    db.ITInvestmentPayment.Add(ObjITP);
                                    db.SaveChanges();
                                    List<ITInvestmentPayment> OFAT = new List<ITInvestmentPayment>();
                                    OFAT.Add(db.ITInvestmentPayment.Find(ObjITP.Id));

                                    if (OEmployeePayroll == null)
                                    {
                                        EmployeePayroll OTEP = new EmployeePayroll()
                                        {
                                            Employee = db.Employee.Find(OEmployee.Id),
                                            ITInvestmentPayment = OFAT,
                                            DBTrack = ITP.DBTrack

                                        };


                                        db.EmployeePayroll.Add(OTEP);
                                        db.SaveChanges();
                                    }
                                    else
                                    {
                                        var aa = db.EmployeePayroll.Find(OEmployeePayroll.Id);
                                        aa.ITInvestmentPayment = OFAT;
                                        //OEmployeePayroll.DBTrack = dbt;
                                        db.EmployeePayroll.Attach(aa);
                                        db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();
                                        db.Entry(aa).State = System.Data.Entity.EntityState.Detached;

                                    }
                                    ts.Complete();
                                    //return Json(new Object[] { "", "", "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
                                    List<string> Msgs = new List<string>();
                                    Msgs.Add("  Data Saved successfully  ");
                                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msgs }, JsonRequestBehavior.AllowGet);
                                }
                                catch (DataException ex)
                                {
                                    //LogFile Logfile = new LogFile();
                                    //ErrorLog Err = new ErrorLog()
                                    //{
                                    //    ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
                                    //    ExceptionMessage = ex.Message,
                                    //    ExceptionStackTrace = ex.StackTrace,
                                    //    LineNo = Convert.ToString(new StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
                                    //    LogTime = DateTime.Now
                                    //};
                                    //Logfile.CreateLogFile(Err);
                                    //return Json(new { sucess = false, responseText = ex.InnerException }, JsonRequestBehavior.AllowGet);
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
                        List<string> Msgu = new List<string>();
                        Msgu.Add("Unable to create...");
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
                                sb.Append("." + "\n");
                            }
                        }
                        List<string> MsgB = new List<string>();
                        var errorMsg = sb.ToString();
                        MsgB.Add(errorMsg);
                        return Json(new Utility.JsonReturnClass { success = false, responseText = MsgB }, JsonRequestBehavior.AllowGet);
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
        [HttpPost]
        public ActionResult Edit(int data)
        {
            List<ITInvestPayment_Val> return_data = new List<ITInvestPayment_Val>();
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.ITInvestmentPayment
                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        ActualInvestment = e.ActualInvestment,
                        DeclaredInvestment = e.DeclaredInvestment,
                        InvestmentDate = e.InvestmentDate,
                        Narration = e.Narration,
                        Action = e.DBTrack.Action
                    }).ToList();

                var add_data = db.ITInvestmentPayment
                  .Include(e => e.ITSubInvestmentPayment)
                    .Include(e => e.FinancialYear)
                    .Include(e => e.ITInvestment)
                    .Include(e => e.ITSection)
                    .Include(e => e.LoanAdvanceHead)
                    .Where(e => e.Id == data)
                   .ToList();

                foreach (var ca in add_data)
                {
                    return_data.Add(
                    new ITInvestPayment_Val
                    {
                        ITSubInvestmentPayment_Id = ca.ITSubInvestmentPayment == null ? null : ca.ITSubInvestmentPayment.Select(e => e.Id.ToString()).ToArray(),
                        ITSubInvestmentPayment_Val = ca.ITSubInvestmentPayment == null ? null : ca.ITSubInvestmentPayment.Select(e => e.InvestmentDate.ToString()).ToArray(),
                        ITInvestment_Id = ca.ITInvestment == null ? null : ca.ITInvestment.Id.ToString(),
                        ITInvestment_Val = ca.ITInvestment == null ? null : ca.ITInvestment.FullDetails,
                        ITSection_Id = ca.ITSection == null ? null : ca.ITSection.Id.ToString(),
                        ITSection_Val = ca.ITSection == null ? null : ca.ITSection.FullDetails,
                        LoanAdvHead_Id = ca.LoanAdvanceHead == null ? null : ca.LoanAdvanceHead.Id.ToString(),
                        LoanAdvHead_Val = ca.LoanAdvanceHead == null ? null : ca.LoanAdvanceHead.FullDetails

                    });
                }

                var W = db.DT_ITInvestmentPayment
                     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                     (e => new
                     {
                         DT_Id = e.Id,
                         ActualInvestment = e.ActualInvestment,
                         DeclaredInvestment = e.DeclaredInvestment,
                         InvestmentDate = e.InvestmentDate,
                         Narration = e.Narration,
                         ITInvestment_Val = e.ITInvestment_Id == 0 ? "" : db.ITInvestment.Where(x => x.Id == e.ITInvestment_Id).Select(x => x.FullDetails).FirstOrDefault(),
                         ITSection_Val = e.ITSection_Id == 0 ? "" : db.ITSection.Where(x => x.Id == e.ITSection_Id).Select(x => x.FullDetails).FirstOrDefault(),
                         LoanAdvHead_Val = e.LoanAdvanceHead_Id == 0 ? "" : db.LoanAdvanceHead.Where(x => x.Id == e.LoanAdvanceHead_Id).Select(x => x.FullDetails).FirstOrDefault(),
                     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var ITInv = db.ITInvestmentPayment.Find(data);
                TempData["RowVersion"] = ITInv.RowVersion;
                var Auth = ITInv.DBTrack.IsModified;
                return Json(new Object[] { Q, return_data, W, Auth, JsonRequestBehavior.AllowGet });
            }
        }

        [HttpPost]
        public async Task<ActionResult> EditSave(ITInvestmentPayment ITInvest, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    string ITInvestmentslist = form["ITInvestmentlist"] == "0" ? "" : form["ITInvestmentlist"];
                    string ITSectionlist = form["ITSectionlist"] == "0" ? "" : form["ITSectionlist"];
                    string LoanAdvHeadlist = form["LoanAdvanceHeadlist"] == "0" ? "" : form["LoanAdvanceHeadlist"];
                    string ITSubInvestmentPaymentlist = form["ITSubInvestmentPaymentlist"] == "0" ? "" : form["ITSubInvestmentPaymentlist"];
                    bool Auth = form["autho_allow"] == "true" ? true : false;


                    if (ITInvestmentslist != null && ITInvestmentslist != "")
                    {
                        var value = db.ITInvestment.Find(int.Parse(ITInvestmentslist));
                        ITInvest.ITInvestment = value;
                    }

                    if (ITSectionlist != null && ITSectionlist != "")
                    {
                        var value = db.ITSection.Find(int.Parse(ITSectionlist));
                        ITInvest.ITSection = value;
                    }

                    if (LoanAdvHeadlist != null && LoanAdvHeadlist != "")
                    {
                        var value = db.LoanAdvanceHead.Find(int.Parse(LoanAdvHeadlist));
                        ITInvest.LoanAdvanceHead = value;
                    }

                    var db_Data = db.ITInvestmentPayment.Include(e => e.ITSubInvestmentPayment).Where(e => e.Id == data).SingleOrDefault();


                    List<ITSubInvestmentPayment> ObjITSubInvestmentPayment = new List<ITSubInvestmentPayment>();
                    if (ITSubInvestmentPaymentlist != null && ITSubInvestmentPaymentlist != "")
                    {
                        var ids = Utility.StringIdsToListIds(ITSubInvestmentPaymentlist);
                        foreach (var ca in ids)
                        {
                            var value = db.ITSubInvestmentPayment.Find(ca);
                            ObjITSubInvestmentPayment.Add(value);
                            db_Data.ITSubInvestmentPayment = ObjITSubInvestmentPayment;
                        }
                    }
                    else
                        db_Data.ITSubInvestmentPayment = null;


                    if (Auth == false)
                    {
                        if (ModelState.IsValid)
                        {
                            try
                            {
                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {


                                    ITInvestmentPayment blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.ITInvestmentPayment.Include(e => e.ITInvestment)
                                                       .Include(e => e.ITSection).Include(e => e.LoanAdvanceHead)
                                                       .Where(e => e.Id == data).SingleOrDefault();
                                        originalBlogValues = context.Entry(blog).OriginalValues;
                                    }

                                    ITInvest.DBTrack = new DBTrack
                                    {
                                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = SessionManager.UserName,
                                        ModifiedOn = DateTime.Now
                                    };

                                    db.ITInvestmentPayment.Attach(db_Data);
                                    db.Entry(db_Data).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    TempData["RowVersion"] = ITInvest.RowVersion;
                                    db.Entry(db_Data).State = System.Data.Entity.EntityState.Detached;

                                    int a = EditS(ITSectionlist, ITInvestmentslist, LoanAdvHeadlist, data, ITInvest, ITInvest.DBTrack);




                                    using (var context = new DataBaseContext())
                                    {

                                        var obj = DBTrackFile.DBTrackSave("Payroll/Payroll", originalBlogValues, db.ChangeTracker, ITInvest.DBTrack);
                                        DT_ITInvestmentPayment DT_ITInvest = (DT_ITInvestmentPayment)obj;
                                        DT_ITInvest.ITInvestment_Id = blog.ITInvestment == null ? 0 : blog.ITInvestment.Id;
                                        DT_ITInvest.ITSection_Id = blog.ITSection == null ? 0 : blog.ITSection.Id;
                                        DT_ITInvest.LoanAdvanceHead_Id = blog.LoanAdvanceHead == null ? 0 : blog.LoanAdvanceHead.Id;

                                        db.Create(DT_ITInvest);
                                        db.SaveChanges();
                                    }
                                    await db.SaveChangesAsync();
                                    ts.Complete();
                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = ITInvest.Id, Val = ITInvest.InvestmentDate.ToString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    // return Json(new Object[] { ITInvest.Id, ITInvest.InvestmentDate, "Record Updated", JsonRequestBehavior.AllowGet });

                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (ITInvestmentPayment)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user.");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    // return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (ITInvestmentPayment)databaseEntry.ToObject();
                                    ITInvest.RowVersion = databaseValues.RowVersion;

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

                            //return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                            List<string> Msgn = new List<string>();
                            Msgn.Add("Record modified by another user.So refresh it and try to save again.");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msgn }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            ITInvestmentPayment blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            ITInvestmentPayment Old_ITInvest = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.ITInvestmentPayment.Where(e => e.Id == data).SingleOrDefault();
                                originalBlogValues = context.Entry(blog).OriginalValues;
                            }
                            ITInvest.DBTrack = new DBTrack
                            {
                                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                Action = "M",
                                IsModified = blog.DBTrack.IsModified == true ? true : false,
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };
                            ITInvestmentPayment ITInvestPay = new ITInvestmentPayment()
                            {
                                ActualInvestment = ITInvest.ActualInvestment,
                                DeclaredInvestment = ITInvest.DeclaredInvestment,
                                InvestmentDate = ITInvest.InvestmentDate,
                                DBTrack = ITInvest.DBTrack,
                                Id = data
                            };

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Payroll/Payroll", "M", blog, ITInvestPay, "ITInvestmentPayment", ITInvest.DBTrack);

                                Old_ITInvest = context.ITInvestmentPayment.Where(e => e.Id == data).Include(e => e.ITInvestment)
                                                       .Include(e => e.ITSection).Include(e => e.LoanAdvanceHead).SingleOrDefault();
                                DT_ITInvestmentPayment DT_ITInvest = (DT_ITInvestmentPayment)obj;
                                DT_ITInvest.ITInvestment_Id = DBTrackFile.ValCompare(Old_ITInvest.ITInvestment, ITInvest.ITInvestment);
                                DT_ITInvest.ITSection_Id = DBTrackFile.ValCompare(Old_ITInvest.ITSection, ITInvest.ITSection);
                                DT_ITInvest.LoanAdvanceHead_Id = DBTrackFile.ValCompare(Old_ITInvest.LoanAdvanceHead, ITInvest.LoanAdvanceHead);
                                db.Create(DT_ITInvest);
                            }
                            blog.DBTrack = ITInvest.DBTrack;
                            db.ITInvestmentPayment.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = ITInvest.InvestmentDate.ToString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { blog.Id, ITInvest.InvestmentDate, "Record Updated", JsonRequestBehavior.AllowGet });
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
                return View();
            }
        }

        public int EditS(string ITSection, string ITInvestment, string LoanAdvHead, int data, ITInvestmentPayment c, DBTrack dbT)
        {
            ITInvestmentPayment typedetails = null;
            using (DataBaseContext db = new DataBaseContext())
            {
                //ITSection
                if (ITSection != null & ITSection != "")
                {
                    var val = db.ITSection.Find(int.Parse(ITSection));
                    c.ITSection = val;

                    var type = db.ITInvestmentPayment.Include(e => e.ITSection).Where(e => e.Id == data).SingleOrDefault();

                    if (type.ITSection != null)
                    {
                        typedetails = db.ITInvestmentPayment.Where(x => x.ITSection.Id == type.ITSection.Id && x.Id == data).SingleOrDefault();
                    }
                    else
                    {
                        typedetails = db.ITInvestmentPayment.Where(x => x.Id == data).SingleOrDefault();
                    }
                    typedetails.ITSection = c.ITSection;
                }
                else
                {
                    typedetails = db.ITInvestmentPayment.Include(e => e.ITSection).Where(x => x.Id == data).SingleOrDefault();
                    typedetails.ITSection = null;
                }
                /* end */

                //ITInvestment
                if (ITInvestment != null & ITInvestment != "")
                {
                    var val = db.ITInvestment.Find(int.Parse(ITInvestment));
                    c.ITInvestment = val;

                    var type = db.ITInvestmentPayment.Include(e => e.ITInvestment).Where(e => e.Id == data).SingleOrDefault();

                    if (type.ITInvestment != null)
                    {
                        typedetails = db.ITInvestmentPayment.Where(x => x.ITInvestment.Id == type.ITInvestment.Id && x.Id == data).SingleOrDefault();
                    }
                    else
                    {
                        typedetails = db.ITInvestmentPayment.Where(x => x.Id == data).SingleOrDefault();
                    }
                    typedetails.ITInvestment = c.ITInvestment;
                }
                else
                {
                    typedetails = db.ITInvestmentPayment.Include(e => e.ITInvestment).Where(x => x.Id == data).SingleOrDefault();
                    typedetails.ITInvestment = null;
                }
                // ITInvestment end */


                //LoanAdvHead
                if (LoanAdvHead != null & LoanAdvHead != "")
                {
                    var val = db.LoanAdvanceHead.Find(int.Parse(LoanAdvHead));
                    c.LoanAdvanceHead = val;

                    var type = db.ITInvestmentPayment.Include(e => e.LoanAdvanceHead).Where(e => e.Id == data).SingleOrDefault();

                    if (type.ITInvestment != null)
                    {
                        typedetails = db.ITInvestmentPayment.Where(x => x.LoanAdvanceHead.Id == type.LoanAdvanceHead.Id && x.Id == data).SingleOrDefault();
                    }
                    else
                    {
                        typedetails = db.ITInvestmentPayment.Where(x => x.Id == data).SingleOrDefault();
                    }
                    typedetails.LoanAdvanceHead = c.LoanAdvanceHead;
                }
                else
                {
                    typedetails = db.ITInvestmentPayment.Include(e => e.LoanAdvanceHead).Where(x => x.Id == data).SingleOrDefault();
                    typedetails.LoanAdvanceHead = null;
                }
                // ITInvestment end */

                db.ITInvestmentPayment.Attach(typedetails);
                db.Entry(typedetails).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                TempData["RowVersion"] = typedetails.RowVersion;
                db.Entry(typedetails).State = System.Data.Entity.EntityState.Detached;

                var CurIT = db.ITInvestmentPayment.Find(data);
                TempData["CurrRowVersion"] = CurIT.RowVersion;
                db.Entry(CurIT).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    c.DBTrack = dbT;
                    ITInvestmentPayment ITInvPayment = new ITInvestmentPayment()
                    {
                        ActualInvestment = c.ActualInvestment,
                        DeclaredInvestment = c.DeclaredInvestment,
                        InvestmentDate = c.InvestmentDate,
                        Narration = c.Narration,
                        Id = data,
                        DBTrack = c.DBTrack
                    };


                    db.ITInvestmentPayment.Attach(ITInvPayment);
                    db.Entry(ITInvPayment).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(ITInvPayment).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                    return 1;
                }
                return 0;
            }
        }

        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    ITInvestmentPayment ITInvestmentPayments = db.ITInvestmentPayment.Include(e => e.ITInvestment)
                                                       .Include(e => e.ITSection).Include(e => e.LoanAdvanceHead).Where(e => e.Id == data).SingleOrDefault();

                    ITInvestment ITInvest = ITInvestmentPayments.ITInvestment;
                    ITSection ITSec = ITInvestmentPayments.ITSection;
                    LoanAdvanceHead LoanAdvHead = ITInvestmentPayments.LoanAdvanceHead;

                    if (ITInvestmentPayments.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = ITInvestmentPayments.DBTrack.CreatedBy != null ? ITInvestmentPayments.DBTrack.CreatedBy : null,
                                CreatedOn = ITInvestmentPayments.DBTrack.CreatedOn != null ? ITInvestmentPayments.DBTrack.CreatedOn : null,
                                IsModified = ITInvestmentPayments.DBTrack.IsModified == true ? true : false
                            };
                            ITInvestmentPayments.DBTrack = dbT;
                            db.Entry(ITInvestmentPayments).State = System.Data.Entity.EntityState.Modified;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, ITInvestmentPayments.DBTrack);
                            DT_ITInvestmentPayment DT_ITInvest = (DT_ITInvestmentPayment)rtn_Obj;
                            DT_ITInvest.ITInvestment_Id = ITInvestmentPayments.ITInvestment == null ? 0 : ITInvestmentPayments.ITInvestment.Id;
                            DT_ITInvest.ITSection_Id = ITInvestmentPayments.ITSection == null ? 0 : ITInvestmentPayments.ITSection.Id;
                            DT_ITInvest.LoanAdvanceHead_Id = ITInvestmentPayments.LoanAdvanceHead == null ? 0 : ITInvestmentPayments.LoanAdvanceHead.Id;

                            db.Create(DT_ITInvest);

                            await db.SaveChangesAsync();

                            ts.Complete();
                            // return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                            List<string> Msgr = new List<string>();
                            Msgr.Add("Data removed.");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msgr }, JsonRequestBehavior.AllowGet);

                        }
                    }
                    else
                    {
                        var selectedInvs = ITInvestmentPayments.ITInvestment;
                        var selectedITSection = ITInvestmentPayments.ITSection;
                        var selectedITSubInvest = ITInvestmentPayments.ITSubInvestmentPayment;
                        var selectedLoanAdvHead = ITInvestmentPayments.LoanAdvanceHead;

                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            if (selectedInvs != null && selectedITSection != null && selectedITSubInvest != null && selectedLoanAdvHead != null)
                            {
                                List<string> Msge = new List<string>();
                                Msge.Add("Child record exists.Cannot remove it...");

                                var ITSecInv = ITInvestmentPayments.ITInvestment;
                                if (ITSecInv != null)
                                    //return Json(new Object[] { "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msge }, JsonRequestBehavior.AllowGet);

                                var ITSecITSec = ITInvestmentPayments.ITSection;
                                if (ITSecITSec != null)
                                    //return Json(new Object[] { "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msge }, JsonRequestBehavior.AllowGet);

                                var ITSecLoanHead = ITInvestmentPayments.LoanAdvanceHead;
                                if (ITSecLoanHead != null)
                                    //return Json(new Object[] { "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msge }, JsonRequestBehavior.AllowGet);

                                var ITSecSubPay = new HashSet<int>(ITInvestmentPayments.ITSubInvestmentPayment.Select(e => e.Id));
                                if (ITSecSubPay.Count > 0)
                                    //return Json(new Object[] { "", "Child record exists.Cannot remove it..", JsonRequestBehavior.AllowGet });
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msge }, JsonRequestBehavior.AllowGet);
                            }

                            try
                            {
                                DBTrack dbT = new DBTrack
                                {
                                    Action = "D",
                                    ModifiedBy = SessionManager.UserName,
                                    ModifiedOn = DateTime.Now,
                                    CreatedBy = ITInvestmentPayments.DBTrack.CreatedBy != null ? ITInvestmentPayments.DBTrack.CreatedBy : null,
                                    CreatedOn = ITInvestmentPayments.DBTrack.CreatedOn != null ? ITInvestmentPayments.DBTrack.CreatedOn : null,
                                    IsModified = ITInvestmentPayments.DBTrack.IsModified == true ? false : false//,
                                };

                                db.Entry(ITInvestmentPayments).State = System.Data.Entity.EntityState.Deleted;
                                var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, dbT);
                                DT_ITInvestmentPayment DT_Sec = (DT_ITInvestmentPayment)rtn_Obj;
                                DT_Sec.ITInvestment_Id = ITInvest == null ? 0 : ITInvest.Id;
                                DT_Sec.ITSection_Id = ITSec == null ? 0 : ITSec.Id;
                                DT_Sec.LoanAdvanceHead_Id = LoanAdvHead == null ? 0 : LoanAdvHead.Id;
                                db.Create(DT_Sec);

                                await db.SaveChangesAsync();

                                ts.Complete();
                                //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                                List<string> Msgr = new List<string>();
                                Msgr.Add("  Data removed.  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msgr }, JsonRequestBehavior.AllowGet);
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
        }

        [HttpPost]
        public async Task<ActionResult> AuthSave(int auth_id, Boolean isauth, String auth_action) //to save authorised data
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    if (auth_action == "C")
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            ITInvestmentPayment ITInvest = db.ITInvestmentPayment.Include(e => e.ITInvestment)
                                .Include(e => e.ITSection).Include(e => e.LoanAdvanceHead).FirstOrDefault(e => e.Id == auth_id);

                            ITInvest.DBTrack = new DBTrack
                            {
                                Action = "C",
                                ModifiedBy = ITInvest.DBTrack.ModifiedBy != null ? ITInvest.DBTrack.ModifiedBy : null,
                                CreatedBy = ITInvest.DBTrack.CreatedBy != null ? ITInvest.DBTrack.CreatedBy : null,
                                CreatedOn = ITInvest.DBTrack.CreatedOn != null ? ITInvest.DBTrack.CreatedOn : null,
                                IsModified = ITInvest.DBTrack.IsModified == true ? false : false,
                                AuthorizedBy = SessionManager.UserName,
                                AuthorizedOn = DateTime.Now
                            };

                            db.ITInvestmentPayment.Attach(ITInvest);
                            db.Entry(ITInvest).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(ITInvest).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            //db.SaveChanges();
                            var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, ITInvest.DBTrack);
                            DT_ITInvestmentPayment DT_ITInvest = (DT_ITInvestmentPayment)rtn_Obj;
                            DT_ITInvest.ITInvestment_Id = ITInvest.ITInvestment == null ? 0 : ITInvest.ITInvestment.Id;
                            DT_ITInvest.ITSection_Id = ITInvest.ITSection == null ? 0 : ITInvest.ITSection.Id;
                            DT_ITInvest.LoanAdvanceHead_Id = ITInvest.LoanAdvanceHead == null ? 0 : ITInvest.LoanAdvanceHead.Id;
                            db.Create(DT_ITInvest);
                            await db.SaveChangesAsync();

                            ts.Complete();
                            Msg.Add("  Record Authorised");
                            return Json(new Utility.JsonReturnClass { Id = ITInvest.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            // return Json(new Object[] { ITInvest.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else if (auth_action == "M")
                    {

                        ITInvestmentPayment Old_ITInvest = db.ITInvestmentPayment.Include(e => e.ITInvestment)
                                                          .Include(e => e.ITSection).Include(e => e.LoanAdvanceHead).Where(e => e.Id == auth_id).SingleOrDefault();



                        DT_ITInvestmentPayment Curr_ITInvest = db.DT_ITInvestmentPayment
                                                    .Where(e => e.Orig_Id == auth_id && e.DBTrack.Action == "M" && e.DBTrack.IsModified == true)
                                                    .OrderByDescending(e => e.Id)
                                                    .FirstOrDefault();

                        if (Curr_ITInvest != null)
                        {
                            ITInvestmentPayment ITInvest = new ITInvestmentPayment();

                            string LVal1 = Curr_ITInvest.ITInvestment_Id == null ? null : Curr_ITInvest.ITInvestment_Id.ToString();
                            string LVal2 = Curr_ITInvest.ITSection_Id == null ? null : Curr_ITInvest.ITSection_Id.ToString();
                            string LVal3 = Curr_ITInvest.LoanAdvanceHead_Id == null ? null : Curr_ITInvest.LoanAdvanceHead_Id.ToString();
                            ITInvest.ActualInvestment = Curr_ITInvest.ActualInvestment == null ? Curr_ITInvest.ActualInvestment : Curr_ITInvest.ActualInvestment;
                            ITInvest.DeclaredInvestment = Curr_ITInvest.DeclaredInvestment == null ? Curr_ITInvest.DeclaredInvestment : Curr_ITInvest.DeclaredInvestment;
                            ITInvest.InvestmentDate = Curr_ITInvest.InvestmentDate == null ? Curr_ITInvest.InvestmentDate : Curr_ITInvest.InvestmentDate;
                            ITInvest.Narration = Curr_ITInvest.Narration == null ? Curr_ITInvest.Narration : Curr_ITInvest.Narration;

                            if (ModelState.IsValid)
                            {
                                try
                                {
                                    using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                    {
                                        ITInvest.DBTrack = new DBTrack
                                        {
                                            CreatedBy = Old_ITInvest.DBTrack.CreatedBy == null ? null : Old_ITInvest.DBTrack.CreatedBy,
                                            CreatedOn = Old_ITInvest.DBTrack.CreatedOn == null ? null : Old_ITInvest.DBTrack.CreatedOn,
                                            Action = "M",
                                            ModifiedBy = Old_ITInvest.DBTrack.ModifiedBy == null ? null : Old_ITInvest.DBTrack.ModifiedBy,
                                            ModifiedOn = Old_ITInvest.DBTrack.ModifiedOn == null ? null : Old_ITInvest.DBTrack.ModifiedOn,
                                            AuthorizedBy = SessionManager.UserName,
                                            AuthorizedOn = DateTime.Now,
                                            IsModified = false
                                        };

                                        int a = EditS(LVal1, LVal2, LVal3, auth_id, ITInvest, ITInvest.DBTrack);

                                        await db.SaveChangesAsync();

                                        ts.Complete();
                                        Msg.Add("  Record Authorised");
                                        return Json(new Utility.JsonReturnClass { Id = ITInvest.Id, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                        //return Json(new Object[] { ITInvest.Id, "Record Authorised", JsonRequestBehavior.AllowGet });
                                    }
                                }
                                catch (DbUpdateConcurrencyException ex)
                                {
                                    var entry = ex.Entries.Single();
                                    var clientValues = (ITInvestmentPayment)entry.Entity;
                                    var databaseEntry = entry.GetDatabaseValues();
                                    if (databaseEntry == null)
                                    {
                                        //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                        List<string> Msgu = new List<string>();
                                        Msgu.Add("Unable to save changes. The record was deleted by another user.");
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msgu }, JsonRequestBehavior.AllowGet);
                                    }
                                    else
                                    {
                                        var databaseValues = (ITInvestmentPayment)databaseEntry.ToObject();
                                        ITInvest.RowVersion = databaseValues.RowVersion;
                                    }
                                }

                                //return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                                List<string> Msgn = new List<string>();
                                Msgn.Add("Record modified by another user.So refresh it and try to save again.");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msgn }, JsonRequestBehavior.AllowGet);
                            }
                        }
                        else
                        {
                            //return Json(new Object[] { "", "Data removed from history", JsonRequestBehavior.AllowGet });
                            List<string> Msgr = new List<string>();
                            Msgr.Add("Data removed.");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msgr }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else if (auth_action == "D")
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            ITInvestmentPayment ITInvest = db.ITInvestmentPayment.AsNoTracking().Include(e => e.ITSection)
                                                                        .Include(e => e.ITInvestment).Include(e => e.LoanAdvanceHead).FirstOrDefault(e => e.Id == auth_id);

                            ITSection LKVal1 = ITInvest.ITSection;
                            ITInvestment LKVal2 = ITInvest.ITInvestment;
                            LoanAdvanceHead LKVal3 = ITInvest.LoanAdvanceHead;

                            ITInvest.DBTrack = new DBTrack
                            {
                                Action = "D",
                                ModifiedBy = ITInvest.DBTrack.ModifiedBy != null ? ITInvest.DBTrack.ModifiedBy : null,
                                CreatedBy = ITInvest.DBTrack.CreatedBy != null ? ITInvest.DBTrack.CreatedBy : null,
                                CreatedOn = ITInvest.DBTrack.CreatedOn != null ? ITInvest.DBTrack.CreatedOn : null,
                                IsModified = false,
                                AuthorizedBy = SessionManager.UserName,
                                AuthorizedOn = DateTime.Now
                            };

                            db.ITInvestmentPayment.Attach(ITInvest);
                            db.Entry(ITInvest).State = System.Data.Entity.EntityState.Deleted;


                            var rtn_Obj = DBTrackFile.DBTrackSave("Payroll/Payroll", null, db.ChangeTracker, ITInvest.DBTrack);
                            DT_ITInvestmentPayment DT_ITInvest = (DT_ITInvestmentPayment)rtn_Obj;
                            DT_ITInvest.ITInvestment_Id = ITInvest.ITInvestment == null ? 0 : ITInvest.ITInvestment.Id;
                            DT_ITInvest.ITSection_Id = ITInvest.ITSection == null ? 0 : ITInvest.ITSection.Id;
                            DT_ITInvest.LoanAdvanceHead_Id = ITInvest.LoanAdvanceHead == null ? 0 : ITInvest.LoanAdvanceHead.Id;
                            db.Create(DT_ITInvest);
                            await db.SaveChangesAsync();
                            db.Entry(ITInvest).State = System.Data.Entity.EntityState.Detached;
                            ts.Complete();
                            //return Json(new Object[] { "", "Record Authorised", JsonRequestBehavior.AllowGet });
                            List<string> Msgs = new List<string>();
                            Msgs.Add(" Record Authorised ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msgs }, JsonRequestBehavior.AllowGet);
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
                return View();
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
                IEnumerable<ITInvestmentPayment> ITInvestmentPayment = null;
                if (gp.IsAutho == true)
                {
                    ITInvestmentPayment = db.ITInvestmentPayment.Where(e => e.DBTrack.IsModified == true).AsNoTracking().ToList();
                }
                else
                {
                    ITInvestmentPayment = db.ITInvestmentPayment.AsNoTracking().ToList();
                }

                IEnumerable<ITInvestmentPayment> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = ITInvestmentPayment;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.Id.ToString().Contains(gp.searchString))
                            || (e.InvestmentDate.ToString().Contains(gp.searchString))
                            || (e.ActualInvestment.ToString().Contains(gp.searchString))
                            || (e.DeclaredInvestment.ToString().Contains(gp.searchString))
                            ).Select(a => new Object[] { a.Id, a.InvestmentDate, a.ActualInvestment, a.DeclaredInvestment }).ToList();

                        //jsonData = IE.Select(a => new { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.InvestmentDate, a.ActualInvestment, a.DeclaredInvestment }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = ITInvestmentPayment;
                    Func<ITInvestmentPayment, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "InvestmentDate" ? c.InvestmentDate.Value.ToString() :
                                         gp.sidx == "ActualInvestment" ? c.ActualInvestment.ToString() :
                                         gp.sidx == "DeclaredInvestment" ? c.DeclaredInvestment.ToString() : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.InvestmentDate), Convert.ToString(a.ActualInvestment), a.DeclaredInvestment }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.InvestmentDate), Convert.ToString(a.ActualInvestment), a.DeclaredInvestment }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.InvestmentDate, a.ActualInvestment, a.DeclaredInvestment }).ToList();
                    }
                    totalRecords = ITInvestmentPayment.Count();
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

        public class ITinvestmentPaymentChildDataClass
        {
            public string Id { get; set; }
            public string ITInvestmentName { get; set; }
            public string InvestmentDate { get; set; }
            public string DeclaredInvestment { get; set; }
            public string ActualInvestment { get; set; }

        }

        public ActionResult Get_ITinvestmentPaymentDetails(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var db_data = db.EmployeePayroll.Include(e => e.ITInvestmentPayment)
                        .Include(e => e.ITInvestmentPayment.Select(t => t.ITInvestment))
                        .Include(e => e.ITInvestmentPayment.Select(t => t.ITSection))
                         .Include(e => e.ITInvestmentPayment.Select(t => t.ITSection.ITSectionListType))
                        .Where(e => e.Id == data).ToList();
                    if (db_data.Count > 0)
                    {
                        List<ITinvestmentPaymentChildDataClass> returndata = new List<ITinvestmentPaymentChildDataClass>();
                        foreach (var item in db_data.SelectMany(e => e.ITInvestmentPayment))
                        {
                            if (item.ITSection != null && item.ITSection.ITSectionListType.LookupVal.ToUpper() == "REBATE")
                            {
                                returndata.Add(new ITinvestmentPaymentChildDataClass
                                {
                                    Id = item.Id.ToString() != null ? item.Id.ToString() : "",
                                    ITInvestmentName = item.ITInvestment != null ? item.ITInvestment.ITInvestmentName : "",
                                    InvestmentDate = item.InvestmentDate != null ? item.InvestmentDate.Value.ToString("dd/MM/yyyy") : "",
                                    DeclaredInvestment = item.DeclaredInvestment != null ? item.DeclaredInvestment.ToString() : "",
                                    ActualInvestment = item.ActualInvestment != null ? item.ActualInvestment.ToString() : "",

                                });
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

        public JsonResult EditGridDetails(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.ITInvestmentPayment.Include(e => e.ITInvestment)

                     .Where(e => e.Id == data).Select
                     (e => new
                     {
                         ITInvestmentName = e.ITInvestment.ITInvestmentName,
                         InvestmentDate = e.InvestmentDate,
                         DeclaredInvestment = e.DeclaredInvestment,
                         ActualInvestment = e.ActualInvestment,
                         Action = e.DBTrack.Action
                     }).ToList();
                var ITInvestmentPayment = db.ITInvestmentPayment.Find(data);
                Session["RowVersion"] = ITInvestmentPayment.RowVersion;
                var Auth = ITInvestmentPayment.DBTrack.IsModified;
                //return Json(new Object[] { Q, add_data, "", Auth, JsonRequestBehavior.AllowGet });
                return Json(new { data = Q }, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult GridEditSave(ITInvestmentPayment ITP, FormCollection form, string data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var DeclaredInvestment = form["ITInvestmentPayment-DeclaredInvestment"] == " 0" ? "" : form["ITInvestmentPayment-DeclaredInvestment"];
                    var ActualInvestment = form["ITInvestmentPayment-ActualInvestment"] == " 0" ? "" : form["ITInvestmentPayment-ActualInvestment"];
                    ITP.DeclaredInvestment = Convert.ToDouble(DeclaredInvestment);
                    ITP.ActualInvestment = Convert.ToDouble(ActualInvestment);
                    if (data != null)
                    {
                        var id = Convert.ToInt32(data);
                        var db_data = db.ITInvestmentPayment.Where(e => e.Id == id).SingleOrDefault();
                        db_data.ActualInvestment = ITP.ActualInvestment;
                        db_data.DeclaredInvestment = ITP.DeclaredInvestment;
                        try
                        {
                            db.ITInvestmentPayment.Attach(db_data);
                            db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;
                            Msg.Add("  Record Authorised");
                            return Json(new { status = true, responseText = "Record Updated..!" }, JsonRequestBehavior.AllowGet);
                            //return Json(new Utility.JsonReturnClass {data = db_data.ToString() ,success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            // return Json(new { data = db_data, responseText = "Record Updated..!" }, JsonRequestBehavior.AllowGet);
                        }
                        catch (Exception e)
                        {

                            throw e;
                        }
                    }
                    else
                    {
                        Msg.Add("  Data Is Null  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
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

        public ActionResult GridDelete(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var IT80c = db.ITInvestmentPayment.Find(data);
                db.ITInvestmentPayment.Remove(IT80c);
                db.SaveChanges();
                //return Json(new Object[] { "", "", " Record Deleted Successfully " }, JsonRequestBehavior.AllowGet);
                List<string> Msgs = new List<string>();
                Msgs.Add("Record Deleted Successfully ");
                return Json(new Utility.JsonReturnClass { success = true, responseText = Msgs }, JsonRequestBehavior.AllowGet);

            }
        }
    }

}