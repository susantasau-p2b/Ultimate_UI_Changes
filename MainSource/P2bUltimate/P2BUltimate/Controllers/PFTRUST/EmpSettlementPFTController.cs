using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using P2b.Global;
using P2B.PFTRUST;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using P2BUltimate.Process;
using P2BUltimate.Security;
using System.Data;


namespace P2BUltimate.Controllers.PFTRUST
{
    public class EmpSettlementPFTController : Controller
    {
        //
        // GET: /EmpSettlementPFT/
        public ActionResult Index()
        {
            return View("~/Views/PFTrust/EmpSettlementPFT/index.cshtml");
        }

        public class returnP2BClass
        {
            public int Id { get; set; }
            public string EmpCode { get; set; }
            public string EmpName { get; set; }
            public string TotalGross { get; set; }
            public string Deductions { get; set; }
            public string ActualFundGross { get; set; }
            public string SettlementDate { get; set; }
            public string IsPaymentLock { get; set; }
            public string ChequeIssueDate { get; set; }
        }

        public class returnDataClass
        {
            public int Id { get; set; }
            public string PFTCalendar { get; set; }
            public string SeperationDate { get; set; }
            public string SettlementDate { get; set; }
            public string OwnIntCloseBal { get; set; }
            public string OwnerIntCloseBal { get; set; }
            public string PFIntCloseBal { get; set; }
            public string VPFCloseBal { get; set; }
            public string PFCloseBal { get; set; }
            public string OwnCloseBal { get; set; }
            public string OwnerCloseBal { get; set; }
            public string VPFIntCloseBal { get; set; }
            public string Actualtax { get; set; }
            public string TaxableIncome { get; set; }
            public string Actualsurchar { get; set; }
            public string DeductTax { get; set; }
            public string DeductSurchar { get; set; }
            public string TotalGross { get; set; }
            public string Deductions { get; set; }
            public string PaymentDate { get; set; }
            public string Cheque_no { get; set; }
            public string BankBranch { get; set; }
            public string ChequeIssueDate { get; set; }
            public bool IsPaymentLock { get; set; }
            public string OwnPfInterest { get; set; }
            public string OwnerPfInterest { get; set; }
            public string VpfInt { get; set; }
            public string PfInterest { get; set; }
        }
        public class returnCalendarClass
        {
            public int Id { get; set; }
            public string FulDetails { get; set; }
        }
        public ActionResult PFCalenderDropdownlist(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                SelectList s = (SelectList)null;
                var selected = "";
                List<returnCalendarClass> returndata = new List<returnCalendarClass>();
                if (data2 != "" && data2 != "0")
                {
                    int calid = Convert.ToInt32(data2);
                    var qurey = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "PFCalendar".ToUpper() && e.Id == calid).AsEnumerable().ToList();
                    foreach (var item in qurey)
                    {
                        returndata.Add(new returnCalendarClass
                        {
                            Id = item.Id,
                            FulDetails = "From Date:" + " " + item.FromDate.Value.ToShortDateString() + "  ," + "To Date:" + " " + item.ToDate.Value.ToShortDateString()

                        });
                    }
                }
                else
                {
                    var qurey = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "PFCalendar".ToUpper() && e.Default == true).AsEnumerable().ToList();
                    foreach (var item in qurey)
                    {
                        returndata.Add(new returnCalendarClass
                        {
                            Id = item.Id,
                            FulDetails = "From Date:" + " " + item.FromDate.Value.ToShortDateString() + "  ," + "To Date:" + " " + item.ToDate.Value.ToShortDateString()

                        });
                    }
                }



                if (data2 != "" && data2 != "0")
                {
                    selected = data2;
                }
                if (returndata != null)
                {
                    s = new SelectList(returndata, "Id", "FulDetails", selected);
                }
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult Process(EmpSettlementPFT LvReq, FormCollection form)
        {
            List<string> Msg = new List<string>();
            try
            {
                using (DataBaseContext db = new DataBaseContext())
                {


                    string Emp = form["Employee-Table"] == "0" ? "" : form["Employee-Table"];
                    string SettlementDatef = form["SettlementDate"] == "0" ? "" : form["SettlementDate"];
                    DateTime SettlementDate = Convert.ToDateTime(SettlementDatef);
                    DateTime intPostingdate = SettlementDate;


                    int ids = 0;
                    if (Emp != null && Emp != "0" && Emp != "false")
                    {
                        //ids = one_ids(Emp);
                        ids = Convert.ToInt32(Emp);
                    }
                    else
                    {
                        Msg.Add("  Please Select Employee  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    }

                    //var Cal = db.Calendar.Where(e => e.Default == true && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();
                    //LvReq.LeaveCalendar = Cal;

                    //if (FrmStat != null && FrmStat != "")
                    //{
                    //    var id = Convert.ToInt32(FrmStat);
                    //    var value = db.LookupValue.Where(e => e.Id == id).FirstOrDefault();
                    //    LvReq.FromStat = value;
                    //}
                    //if (Tostat != null && Tostat != "")
                    //{
                    //    var id = Convert.ToInt32(Tostat);
                    //    var value = db.LookupValue.Where(e => e.Id == id).FirstOrDefault();
                    //    LvReq.ToStat = value;
                    //}
                    //if (LvHeadList != null && LvHeadList != "")
                    //{
                    //    var id = Convert.ToInt32(LvHeadList);
                    //    var value = db.LvHead.Where(e => e.Id == id).FirstOrDefault();
                    //    LvReq.LeaveHead = value;
                    //}



                    var Comp_Id = 0;
                    Comp_Id = Convert.ToInt32(Session["CompId"]);
                    //  var Z = db.CompanyPayroll.Where(e => e.Company.Id == Comp_Id).Include(e => e.EmployeePayroll.Select(f => f.Employee)).SingleOrDefault();
                    Employee OEmployee = null;
                    int OEmployeeLvId = 0;




                    if (ids != 0)
                    {
                        //foreach (var i in ids)
                        //{
                        OEmployee = db.Employee.Where(r => r.Id == ids)
                                .Include(e => e.GeoStruct)
                                .Include(e => e.GeoStruct.Location)
                                .Include(e => e.FuncStruct)
                                .Include(e => e.PayStruct)

                                .SingleOrDefault();


                        var qurey = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "PFCALENDAR".ToUpper() && e.Default == true).AsEnumerable()
                 .Select(e => new
                 {
                     Id = e.Id,
                     Lvcalendardesc = "FromDate :" + e.FromDate.Value.ToShortDateString() + " ToDate :" + e.ToDate.Value.ToShortDateString(),
                     FromDate = e.FromDate.Value.ToShortDateString(),
                     ToDate = e.ToDate.Value.ToShortDateString()

                 }).SingleOrDefault();

                        var CompantPFTrust_Idc = db.CompanyPFTrust.Where(e => e.Company_Id == Comp_Id).Select(e => e.Id).SingleOrDefault();
                        var IntPolicyIdc = db.PFTACCalendar.Include(e => e.CompanyPFTrust).Include(e => e.InterestPolicies).Where(e => e.CompanyPFTrust.Id == CompantPFTrust_Idc && e.PFTCalendar.Id == qurey.Id).Select(e => e.InterestPolicies.Id).SingleOrDefault();
                        if (IntPolicyIdc == 0)
                        {
                            Msg.Add(" Interest Policy Not Available for " + qurey.Lvcalendardesc);
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }

                        var InterestPolicies = db.InterestPolicies.Include(e => e.InterestRate).Where(e => e.Id == IntPolicyIdc).SingleOrDefault();

                        var IntRate = InterestPolicies.InterestRate.Where(e => intPostingdate >= e.EffectiveFrom && intPostingdate <= e.EffectiveTo)
                                                         .SingleOrDefault();
                        if (IntRate == null)
                        {
                            Msg.Add(" Settlement Posting date not between rate period of current PF calendar Policy");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }

                        var EmployeePFTpaid = db.EmployeePFTrust.Include(e => e.Employee).Include(e => e.EmpSettlementPFT).Include(e => e.Employee.ServiceBookDates).Where(e => e.Employee_Id == OEmployee.Id).SingleOrDefault();//pfexit date checked for employee live in PF trust
                        if (EmployeePFTpaid.EmpSettlementPFT.Count()>0)
                        {
                            var checkpayment = EmployeePFTpaid.EmpSettlementPFT.OrderByDescending(e=>e.Id).FirstOrDefault();

                            if (checkpayment.IsPaymentLock == true)
                            {
                                Msg.Add(" Settlement payment is lock. So can't Reprocess..  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                               
                            }
                        }
                        DBTrack DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                        var CompantPFTrust_Id = db.CompanyPFTrust.Where(e => e.Company_Id == Comp_Id).Select(e => e.Id).SingleOrDefault();
                        var IntPolicy = db.PFTACCalendar.Include(e => e.CompanyPFTrust).Include(e => e.InterestPolicies).Include(e => e.PFTCalendar).Where(e => e.PFTCalendar.Id == qurey.Id && e.CompanyPFTrust.Id == CompantPFTrust_Id).SingleOrDefault();
                        var IntPolicyId = IntPolicy.InterestPolicies.Id;
                        //  var IntPolicyId = db.InterestPolicies.Where(e => e.PFTACCalendar_Id == PFTAcCalendar_Id).Select(e => e.Id).SingleOrDefault();

                        var EmployeePFT_Ids = db.EmployeePFTrust.Include(e => e.Employee).Include(e => e.Employee.ServiceBookDates).Where(e => e.Employee_Id == OEmployee.Id).Select(e => e.Id).SingleOrDefault();//pfexit date checked for employee live in PF trust

                        List<P2BUltimate.Process.GlobalProcess.ReturnDataintp> ReturnDataList = new List<P2BUltimate.Process.GlobalProcess.ReturnDataintp>();


                       
                            try
                            {
                                ReturnDataList = P2BUltimate.Process.GlobalProcess.InterestPostingEmp(IntPolicyId, EmployeePFT_Ids,
                                      intPostingdate, SettlementDate, DBTrack);


                                foreach (var item in ReturnDataList)
                                {
                                    Msg.Add(item.ErrMsg);
                                }



                            }
                            catch (DataException ex)
                            {

                            }
                           
                       
                        var PassbookLoanIDValue = new List<string>();
                        PassbookLoanIDValue.Add("SETTLEMENT POSTING");
                        PassbookLoanIDValue.Add("SETTLEMENT BALANCE");

                        List<int> PassbookID = new List<int>();
                        PassbookID = db.LookupValue.Where(e => PassbookLoanIDValue.Contains(e.LookupVal.ToUpper())).Select(e => e.Id).ToList();


                        var PFTEmployeeLedgerData_01 = db.EmployeePFTrust.Where(e => e.Id == EmployeePFT_Ids)
                .Select(e => e.EmpSettlementPFT
                .Where(r =>
                PassbookID.Contains(r.PassbookActivity.Id) == true).OrderByDescending(x => x.Id).ToList()).SingleOrDefault();
                        if (PFTEmployeeLedgerData_01.Count() > 0)
                        {
                            return Json(new
                            {
                                PFTEmployeeLedgerData_01.FirstOrDefault().OwnOpenBal,
                                PFTEmployeeLedgerData_01.FirstOrDefault().OwnIntOpenBal,
                                PFTEmployeeLedgerData_01.FirstOrDefault().OwnerOpenBal,
                                PFTEmployeeLedgerData_01.FirstOrDefault().OwnerIntOpenBal,
                                PFTEmployeeLedgerData_01.FirstOrDefault().PFOpenBal,
                                PFTEmployeeLedgerData_01.FirstOrDefault().PFIntOpenBal,
                                PFTEmployeeLedgerData_01.FirstOrDefault().VPFOpenBal,
                                PFTEmployeeLedgerData_01.FirstOrDefault().VPFIntOpenBal,

                                PFTEmployeeLedgerData_01.FirstOrDefault().OwnCloseBal,
                                PFTEmployeeLedgerData_01.FirstOrDefault().OwnerCloseBal,
                                PFTEmployeeLedgerData_01.FirstOrDefault().PFCloseBal,
                                PFTEmployeeLedgerData_01.FirstOrDefault().VPFCloseBal,
                                PFTEmployeeLedgerData_01.FirstOrDefault().OwnIntCloseBal,
                                PFTEmployeeLedgerData_01.FirstOrDefault().OwnerIntCloseBal,
                                PFTEmployeeLedgerData_01.FirstOrDefault().PFIntCloseBal,
                                PFTEmployeeLedgerData_01.FirstOrDefault().VPFIntCloseBal,
                                PFTEmployeeLedgerData_01.FirstOrDefault().SeperationDate,
                                PFTEmployeeLedgerData_01.FirstOrDefault().ChequeIssueDate,
                                PFTEmployeeLedgerData_01.FirstOrDefault().PaymentDate,
                                PFTEmployeeLedgerData_01.FirstOrDefault().TaxableAccountInterest,

                                PFTEmployeeLedgerData_01.LastOrDefault().OwnPfInterest,
                                PFTEmployeeLedgerData_01.LastOrDefault().OwnerPfInterest,
                                PFTEmployeeLedgerData_01.LastOrDefault().VpfInt,
                                PFTEmployeeLedgerData_01.LastOrDefault().PfInterest,
                                PFTEmployeeLedgerData_01.LastOrDefault().OwnIntOnInt,
                                PFTEmployeeLedgerData_01.LastOrDefault().OwnerIntOnInt,
                                PFTEmployeeLedgerData_01.LastOrDefault().VPFIntOnInt,
                                
                                
                            });
                        }


                        //}
                    }
                    Msg.Add("  Date Has Been Process  ");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //return Json(new Object[] { "", "", "Date Has Been Process" }, JsonRequestBehavior.AllowGet);
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
                Msg.Add(ex.ToString());
                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
            }
        }

        //public ActionResult Create(EmpSettlementPFT d, FormCollection form)
        //{

        //    using (DataBaseContext db = new DataBaseContext())
        //    {

        //        List<string> Msg = new List<string>();
        //        string Employee_Id = form["Employee-Table"] == "0" ? "0" : form["Employee-Table"];
        //        string calendar = form["PFTACCalendar"] == "0" ? "" : form["PFTACCalendar"];
        //        int holiid = Convert.ToInt32(calendar);
        //        int EmpId = Convert.ToInt32(Employee_Id);
        //        Calendar cal = null;
        //        if (calendar != null)
        //        {
        //            int hdid = Convert.ToInt16(calendar);
        //            cal = db.Calendar.Include(a => a.Name).Where(q => q.Id == hdid).SingleOrDefault();//.Find(int.Parse(HoliCalendarDDL));
        //            d.PFTCalendar = cal;
        //        }
        //        if (ModelState.IsValid)
        //        {
        //            using (TransactionScope ts = new TransactionScope())
        //            {
        //                d.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
        //                try
        //                {
        //                    EmpSettlementPFT ObjEmpSettlementPFT = new EmpSettlementPFT()
        //                    {
        //                        PFTCalendar = d.PFTCalendar,
        //                        SeperationDate = d.SeperationDate,
        //                        SettlementDate = d.SettlementDate,
        //                        OwnIntCloseBal = d.OwnIntCloseBal,
        //                        OwnerIntCloseBal = d.OwnerIntCloseBal,
        //                        PFIntCloseBal = d.PFIntCloseBal,
        //                        VPFCloseBal = d.VPFCloseBal,
        //                        PFCloseBal = d.PFCloseBal,
        //                        OwnCloseBal = d.OwnCloseBal,
        //                        OwnerCloseBal = d.OwnerCloseBal,
        //                        VPFIntCloseBal = d.VPFIntCloseBal,
        //                        Actualtax = d.Actualtax,
        //                        Actualsurchar = d.Actualsurchar,
        //                        Deductions = d.Deductions,
        //                        TotalGross = d.TotalGross,
        //                        TaxableIncome = d.TaxableIncome,
        //                        DeductTax = d.DeductTax,
        //                        DeductSurchar = d.DeductSurchar,
        //                        PaymentDate = d.PaymentDate,
        //                        Cheque_no = d.Cheque_no,
        //                        ChequeIssueDate = d.ChequeIssueDate,
        //                        BankBranch = d.BankBranch,
        //                        IsPaymentLock = d.IsPaymentLock,
        //                        OwnPfInterest = d.OwnPfInterest,
        //                        OwnerPfInterest = d.OwnerPfInterest,
        //                        VpfInt = d.VpfInt,
        //                        PfInterest = d.PfInterest,
        //                        DBTrack = d.DBTrack

        //                    };
        //                    db.EmpSettlementPFT.Add(ObjEmpSettlementPFT);
        //                    db.SaveChanges();

        //                    var db_data = db.EmployeePFTrust
        //                        .Include(e => e.Employee).Include(e => e.EmpSettlementPFT)
        //                        .Where(e => e.Employee.Id == EmpId).FirstOrDefault();


        //                    db_data.Id = db_data.Id;
        //                    db_data.EmpSettlementPFT = ObjEmpSettlementPFT;
        //                    db_data.DBTrack = d.DBTrack;

        //                    db.EmployeePFTrust.Attach(db_data);
        //                    db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
        //                    db.SaveChanges();
        //                    ts.Complete();
        //                    Msg.Add("  Data Saved successfully  ");
        //                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //                }
        //                catch (Exception ex)
        //                {
        //                    LogFile Logfile = new LogFile();
        //                    ErrorLog Err = new ErrorLog()
        //                    {
        //                        ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
        //                        ExceptionMessage = ex.Message,
        //                        ExceptionStackTrace = ex.StackTrace,
        //                        LineNo = Convert.ToString(new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber()),
        //                        LogTime = DateTime.Now
        //                    };
        //                    Logfile.CreateLogFile(Err);
        //                    Msg.Add(ex.Message);
        //                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //                }

        //            }
        //        }
        //        else
        //        {
        //            StringBuilder sb = new StringBuilder("");
        //            foreach (ModelState modelState in ModelState.Values)
        //            {
        //                foreach (ModelError error in modelState.Errors)
        //                {
        //                    sb.Append(error.ErrorMessage);
        //                    sb.Append("." + "\n");
        //                }
        //            }

        //            List<string> MsgB = new List<string>();
        //            var errorMsg = sb.ToString();
        //            MsgB.Add(errorMsg);
        //            return Json(new Utility.JsonReturnClass { success = false, responseText = MsgB }, JsonRequestBehavior.AllowGet);
        //        }

        //    }




        //}

        public ActionResult P2BGrid(P2BGrid_Parameters gp)
        {
            try
            {
                List<returnP2BClass> IE = new List<returnP2BClass>();
                DataBaseContext db = new DataBaseContext();
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                var jsonData = (Object)null;
                var ObjEmpSettlementPFT = db.EmployeePFTrust
                    .Include(e => e.Employee)
                    .Include(e => e.Employee.EmpName)
                    .Include(e => e.EmpSettlementPFT)
                    .Include(e => e.EmpSettlementPFT.Select(x => x.PassbookActivity))
                    .OrderBy(e => e.Id).ToList();

                var PassbookLoanIDValue = new List<string>();
                PassbookLoanIDValue.Add("SETTLEMENT BALANCE");

                List<int> PassbookID = new List<int>();
                PassbookID = db.LookupValue.Where(e => PassbookLoanIDValue.Contains(e.LookupVal.ToUpper())).Select(e => e.Id).ToList();


                foreach (var item in ObjEmpSettlementPFT)
                {
                    var dbdata = item.EmpSettlementPFT.Where(r =>
                      PassbookID.Contains(r.PassbookActivity.Id) == true).OrderByDescending(x => x.Id).FirstOrDefault();

                    if (dbdata != null)
                    {
                        IE.Add(new returnP2BClass
                        {
                            Id = item.Id,
                            EmpCode = item.Employee.EmpCode,
                            EmpName = item.Employee.EmpName.FullNameFML,
                            TotalGross = dbdata.TotalGross.ToString(),
                            Deductions = dbdata.Deductions.ToString(),
                            ActualFundGross = dbdata.ActualFundGross.ToString(),
                            SettlementDate = dbdata.SettlementDate.ToShortDateString(),
                            IsPaymentLock = dbdata.IsPaymentLock.ToString(),
                            ChequeIssueDate = dbdata.ChequeIssueDate.ToShortDateString(),
                        });
                    }

                }

                if (!string.IsNullOrEmpty(gp.searchField))
                {

                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE
                         .Where((e => (e.TotalGross.ToString().Contains(gp.searchString))
                                   || (e.EmpCode.ToString().Contains(gp.searchString))
                                   || (e.EmpName.ToString().Contains(gp.searchString))
                                   || (e.Deductions.ToString().Contains(gp.searchString))
                                   || (e.ActualFundGross.ToString().Contains(gp.searchString))
                                   || (e.SettlementDate.ToString().Contains(gp.searchString))
                                   || (e.IsPaymentLock.ToString().Contains(gp.searchString))
                                   || (e.ChequeIssueDate.ToString().Contains(gp.searchString))
                                   || (e.Id.ToString().Contains(gp.searchString))
                                 )).ToList()
                        .Select(a => new Object[] { a.EmpCode, a.EmpName, a.TotalGross, a.Deductions, a.ActualFundGross, a.SettlementDate.ToString(), a.IsPaymentLock, a.ChequeIssueDate.ToString(), a.Id });
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.EmpCode, a.EmpName, a.TotalGross, a.Deductions, a.ActualFundGross, a.SettlementDate.ToString(), a.IsPaymentLock, a.ChequeIssueDate.ToString(), a.Id }).ToList();
                    }
                    totalRecords = IE.Count();


                }
                else
                {
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.EmpCode, a.EmpName, a.TotalGross, a.Deductions, a.ActualFundGross, a.SettlementDate.ToString(), a.IsPaymentLock, a.ChequeIssueDate.ToString(), a.Id }).ToList();
                    }
                    jsonData = IE;
                    totalRecords = IE.Count();
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

        //public ActionResult Edit(int data)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        List<returnDataClass> returndata = new List<returnDataClass>();

        //        var db_data = db.EmployeePFTrust
        //             .Include(e => e.Employee)
        //             .Include(e => e.Employee.EmpName)
        //             .Include(e => e.EmpSettlementPFT)
        //             .Include(e => e.EmpSettlementPFT.Select(e=>e.PFTCalendar))
        //             .Where(e => e.Id == data).FirstOrDefault();

        //        var item = db_data.EmpSettlementPFT;

        //        returndata.Add(new returnDataClass
        //        {
        //            Id = item.Id,
        //            //PFTCalendar = item.PFTCalendar.ToShortDateString(),
        //            PFTCalendar = item.PFTCalendar.Id == null ? "0" : item.PFTCalendar.Id.ToString(),
        //            SeperationDate = item.SeperationDate.ToShortDateString(),
        //            SettlementDate = item.SettlementDate.ToShortDateString(),
        //            OwnIntCloseBal = item.OwnIntCloseBal.ToString(),
        //            OwnerIntCloseBal = item.OwnerIntCloseBal.ToString(),
        //            PFIntCloseBal = item.PFIntCloseBal.ToString(),

        //            VPFCloseBal = item.VPFCloseBal.ToString(),
        //            PFCloseBal = item.PFCloseBal.ToString(),
        //            OwnCloseBal = item.OwnCloseBal.ToString(),
        //            OwnerCloseBal = item.OwnerCloseBal.ToString(),
        //            VPFIntCloseBal = item.VPFIntCloseBal.ToString(),

        //            Actualtax = item.Actualtax.ToString(),
        //            TaxableIncome = item.TaxableIncome.ToString(),
        //            Actualsurchar = item.Actualsurchar.ToString(),
        //            DeductTax = item.DeductTax.ToString(),
        //            DeductSurchar = item.DeductSurchar.ToString(),
        //            TotalGross = item.TotalGross.ToString(),
        //            Deductions = item.Deductions.ToString(),
        //            PaymentDate = item.PaymentDate.ToShortDateString(),
        //            Cheque_no = item.Cheque_no.ToString(),
        //            BankBranch = item.BankBranch.ToString(),
        //            ChequeIssueDate = item.ChequeIssueDate.ToShortDateString(),
        //            IsPaymentLock = item.IsPaymentLock,
        //            OwnPfInterest = item.OwnPfInterest.ToString(),
        //            OwnerPfInterest = item.OwnerPfInterest.ToString(),
        //            VpfInt = item.VpfInt.ToString(),
        //            PfInterest = item.PfInterest.ToString(),
        //        });


        //        return Json(new Object[] { returndata, "", "", JsonRequestBehavior.AllowGet });
        //    }
        //}
        [HttpPost]
        public async Task<ActionResult> EditSave(EmpSettlementPFT d, FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                 List<string> Msg1 = new List<string>();
                string calendar = form["PFTACCalendar"] == "0" ? "" : form["PFTACCalendar"];
                string Employee_Id = form["Employee-Table"] == "0" ? "0" : form["Employee-Table"];
                int EmpId = Convert.ToInt32(Employee_Id);
                var PFTID = db.EmployeePFTrust
                                .Include(e => e.Employee).Include(e => e.EmpSettlementPFT)
                                .Where(e => e.Employee.Id == EmpId).FirstOrDefault();
                if (calendar == "")
                {
                    Msg1.Add(" Please Select Calendar.  ");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg1 }, JsonRequestBehavior.AllowGet);
                    // return Json(new Object[] { "", "", "Code Already Exists.", JsonRequestBehavior.AllowGet });
                }

                if (calendar != null && calendar != "")
                {

                    var val = db.Calendar.Find(int.Parse(calendar));
                    d.PFTCalendar = val;
                }
                var PassbookLoanIDValue = new List<string>();
                PassbookLoanIDValue.Add("SETTLEMENT BALANCE");

                List<int> PassbookID = new List<int>();
                PassbookID = db.LookupValue.Where(e => PassbookLoanIDValue.Contains(e.LookupVal.ToUpper())).Select(e => e.Id).ToList();


                List<string> Msg = new List<string>();
                try
                {
                    if (ModelState.IsValid)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            d.DBTrack = new DBTrack { Action = "M", CreatedBy = SessionManager.UserName, IsModified = false };

                            var db_data = db.EmployeePFTrust
                                .Include(e => e.Employee).
                                Include(e => e.Employee.EmpName)
                                .Include(e => e.EmpSettlementPFT)
                                .Include(t => t.EmpSettlementPFT.Select(x=>x.PassbookActivity))
                                .Where(e => e.Id == PFTID.Id).FirstOrDefault();

                            var dbdata = db_data.EmpSettlementPFT.Where(r =>
                           PassbookID.Contains(r.PassbookActivity.Id) == true).SingleOrDefault();

                            dbdata.PFTCalendar = d.PFTCalendar;
                            dbdata.SeperationDate = d.SeperationDate;
                            dbdata.SettlementDate = d.SettlementDate;
                            dbdata.PaymentDate = d.PaymentDate;
                            dbdata.TaxableIncome = d.TaxableIncome;
                            dbdata.Actualtax = d.Actualtax;
                            dbdata.Actualsurchar = d.Actualsurchar;
                            dbdata.DeductTax = d.DeductTax;
                            dbdata.DeductSurchar = d.DeductSurchar;
                            dbdata.Deductions = d.Deductions;
                            dbdata.TotalGross = d.TotalGross;
                            dbdata.Cheque_no = d.Cheque_no;
                            dbdata.ChequeIssueDate = d.ChequeIssueDate;
                            dbdata.BankBranch = d.BankBranch;
                            dbdata.IsPaymentLock = d.IsPaymentLock;

                            dbdata.OwnCloseBal = d.OwnCloseBal;
                            dbdata.OwnIntCloseBal = d.OwnIntCloseBal;
                            dbdata.OwnerCloseBal = d.OwnerCloseBal;
                            dbdata.OwnerIntCloseBal = d.OwnerIntCloseBal;
                            dbdata.PFCloseBal = d.PFCloseBal;
                            dbdata.PFIntCloseBal = d.PFIntCloseBal;
                            dbdata.VPFCloseBal = d.VPFCloseBal;
                            dbdata.VPFIntCloseBal = d.VPFIntCloseBal;
                            dbdata.ActualFundGross = d.ActualFundGross;
                            dbdata.DBTrack = d.DBTrack;
                            dbdata.Id = dbdata.Id;
                           
                          

                            db.EmpSettlementPFT.Attach(dbdata);
                            db.Entry(dbdata).State = System.Data.Entity.EntityState.Modified;
                            await db.SaveChangesAsync();
                            // SETTLEMENT POSTING update start
                            var PassbookLoanIDValuePost = new List<string>();
                            PassbookLoanIDValuePost.Add("SETTLEMENT POSTING");

                            List<int> PassbookIDPost = new List<int>();
                            PassbookIDPost = db.LookupValue.Where(e => PassbookLoanIDValuePost.Contains(e.LookupVal.ToUpper())).Select(e => e.Id).ToList();
                            var dbdataPost = db_data.EmpSettlementPFT.Where(r =>
                            PassbookIDPost.Contains(r.PassbookActivity.Id) == true).SingleOrDefault();
                            dbdataPost.PFTCalendar = d.PFTCalendar;
                            dbdataPost.SeperationDate = d.SeperationDate;
                            dbdataPost.SettlementDate = d.SettlementDate;
                            dbdataPost.PaymentDate = d.PaymentDate;

                            dbdataPost.OwnPfInterest = d.OwnPfInterest;
                            dbdataPost.OwnerPfInterest = d.OwnerPfInterest;
                            dbdataPost.VpfInt = d.VpfInt;
                            dbdataPost.PfInterest = d.PfInterest;

                            dbdataPost.OwnIntOnInt = d.OwnIntOnInt;
                            dbdataPost.OwnerIntOnInt = d.OwnerIntOnInt;
                            dbdataPost.VPFIntOnInt = d.VPFIntOnInt;

                            dbdataPost.DBTrack = d.DBTrack;
                            dbdataPost.Id = dbdataPost.Id;
                            dbdataPost.IsPaymentLock = d.IsPaymentLock;


                            db.EmpSettlementPFT.Attach(dbdataPost);
                            db.Entry(dbdataPost).State = System.Data.Entity.EntityState.Modified;
                            await db.SaveChangesAsync();

                            // SETTLEMENT POSTING update end

                            ts.Complete();

                        }
                       //Final payment will insert in ledger
                        if (d.IsPaymentLock==true)
                        {
                            var PassbookLoanIDValueLedger = new List<string>();
                            PassbookLoanIDValueLedger.Add("SETTLEMENT POSTING");
                            PassbookLoanIDValueLedger.Add("SETTLEMENT BALANCE");

                            List<int> PassbookIDLedger = new List<int>();
                            PassbookIDLedger = db.LookupValue.Where(e => PassbookLoanIDValueLedger.Contains(e.LookupVal.ToUpper())).Select(e => e.Id).ToList();

                            var PFTEmployeeLedgerCurrent = db.EmployeePFTrust.Where(e => e.Id == PFTID.Id)
                              .Select(e => e.EmpSettlementPFT.Where(r => PassbookIDLedger.Contains(r.PassbookActivity.Id)).ToList()).SingleOrDefault();
                        
                            var PFTEmpLedgerOldsett = PFTEmployeeLedgerCurrent.OrderBy(e=>e.Id).ToList();
                            var PFTEmpLedgerListset = new List<PFTEmployeeLedger>();
                            foreach (var PFTEmpLedgerOldset in PFTEmpLedgerOldsett)
                            {
                                var PFTEmpLedgerECRDataBalanceset = new PFTEmployeeLedger()
                                {
                                    GeoStruct = PFTEmpLedgerOldset.GeoStruct,
                                    PayStruct = PFTEmpLedgerOldset.PayStruct,
                                    FuncStruct = PFTEmpLedgerOldset.FuncStruct,
                                    MonthYear = PFTEmpLedgerOldset.MonthYear,
                                    PassbookActivity = PFTEmpLedgerOldset.PassbookActivity,
                                    PostingDate = DateTime.Now.Date,
                                    CalcDate = PFTEmpLedgerOldset.CalcDate.Date,//SalaryTPF.PaymentDate.Value, //SalaryTPF.PaymentDate null in table

                                    OwnPFInt = PFTEmpLedgerOldset.OwnPfInterest,
                                    OwnerPFInt = PFTEmpLedgerOldset.OwnerPfInterest,
                                    VPFInt = PFTEmpLedgerOldset.VpfInt,
                                    PFInt = PFTEmpLedgerOldset.PfInterest,
                                    TotPFInt = PFTEmpLedgerOldset.TotPFInt,
                                    

                                    OwnIntOnInt = PFTEmpLedgerOldset.OwnIntOnInt,
                                    OwnerIntOnInt = PFTEmpLedgerOldset.OwnerIntOnInt,
                                    VPFIntOnInt = PFTEmpLedgerOldset.VPFIntOnInt,


                                    #region Input Data for ledger
                                    //OwnPFMonthly = SalaryTPF.PFECRR.EE_Share + SalaryTPF.PFECRR.Arrear_EE_Share,
                                    //OwnerPFMonthly = SalaryTPF.PFECRR.ER_Share + SalaryTPF.PFECRR.Arrear_ER_Share,
                                    //VPFAmountMonthly = SalaryTPF.PFECRR.EE_VPF_Share,
                                    //PensionAmount = SalaryTPF.PFECRR.EPS_Share + SalaryTPF.PFECRR.Arrear_EPS_Share,
                                    //PFWages = SalaryTPF.PFECRR.EPF_Wages + SalaryTPF.PFECRR.Arrear_EPF_Wages,
                                    LoanAmountCredit = PFTEmpLedgerOldset.LoanAmountCredit,
                                    LoanAmountDebit = PFTEmpLedgerOldset.LoanAmountDebit,
                                    //OwnPFInt = 0,
                                    //OwnerPFInt = 0,
                                    //VPFInt =0,
                                    //PFInt = 0,
                                    #endregion Input Data for ledger

                                    OwnOpenBal = PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.OwnOpenBal : 0,
                                    OwnCloseBal = (PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.OwnCloseBal : 0),
                                    OwnerOpenBal = PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.OwnerOpenBal : 0,
                                    OwnerCloseBal = (PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.OwnerCloseBal : 0),
                                    VPFOpenBal = PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.VPFOpenBal : 0,
                                    VPFCloseBal = (PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.VPFCloseBal : 0),
                                    PFOpenBal = PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.PFOpenBal : 0,
                                    PFCloseBal = (PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.PFCloseBal : 0),

                                    OwnIntOpenBal = PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.OwnIntOpenBal : 0,
                                    OwnIntCloseBal = (PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.OwnIntCloseBal : 0),
                                    OwnerIntOpenBal = PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.OwnerIntOpenBal : 0,
                                    OwnerIntCloseBal = (PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.OwnerIntCloseBal : 0),
                                    VPFIntOpenBal = PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.VPFIntOpenBal : 0,
                                    VPFIntCloseBal = (PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.VPFIntCloseBal : 0),
                                    PFIntOpenBal = PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.PFIntOpenBal : 0,
                                    PFIntCloseBal = PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.PFIntCloseBal : 0,



                                    OwnPFLoan = (PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.OwnPFLoan : 0),
                                    OwnerPFLoan = (PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.OwnerPFLoan : 0),
                                    VPFPFLoan = (PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.VPFPFLoan : 0),

                                    IntOnInt = (PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.IntOnInt : 0),
                                    IntonIntOpenBal = (PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.IntonIntOpenBal : 0),
                                    IntonIntCloseBal = (PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.IntonIntCloseBal : 0),
                                    IntOnIntPFLoan = (PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.IntOnIntPFLoan : 0),
                                    OwnIntPFLoan = (PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.OwnIntPFLoan : 0),
                                    OwnerIntPFLoan = (PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.OwnerIntPFLoan : 0),
                                    VPFIntPFLoan = (PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.VPFIntPFLoan : 0),
                                    TotalIntOpenBal = (PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.TotalIntOpenBal : 0),
                                    TotalIntCloseBal = (PFTEmpLedgerOldset != null ? PFTEmpLedgerOldset.TotalIntCloseBal : 0),


                                     IsPassbookClose = false,
                                    Narration = PFTEmpLedgerOldset.Narration,
                                    InterestFrequency = PFTEmpLedgerOldset.InterestFrequency,
                                    DBTrack = d.DBTrack,
                                    AccuNonTaxableAccountOpeningbalance = PFTEmpLedgerOldset.AccuNonTaxableAccountOpeningbalance,
                                    AccuNonTaxableAccountClosingbalance = PFTEmpLedgerOldset.AccuNonTaxableAccountClosingbalance,
                                    AccTaxableAccountOpeningbalance = PFTEmpLedgerOldset.AccTaxableAccountOpeningbalance,
                                    AccTaxableAccountClosingbalance = PFTEmpLedgerOldset.AccTaxableAccountClosingbalance,

                                    AccuNonTaxableAccountFy_Openingbalance = PFTEmpLedgerOldset.AccuNonTaxableAccountFy_Openingbalance,//PFTEmpLedgerOld != null ? PFTEmpLedgerOld.AccuNonTaxableAccountFy_Closingbalance : 0,
                                    AccuNonTaxableAccountFy_Closingbalance = PFTEmpLedgerOldset.AccuNonTaxableAccountFy_Closingbalance,//check taxableaccpFy
                                    TaxableAccountFy_Openingbalance = PFTEmpLedgerOldset.TaxableAccountFy_Openingbalance,//PFTEmpLedgerOld != null ? PFTEmpLedgerOld.TaxableAccountFy_Closingbalance : 0,
                                    TaxableAccountFy_Closingbalance = PFTEmpLedgerOldset.TaxableAccountFy_Closingbalance,//(PFTEmpLedgerOld != null ? PFTEmpLedgerOld.TaxableAccountFy_Closingbalance : 0) + Taxableaccpfmonthly,

                                    NonTaxableAccountInterest = PFTEmpLedgerOldset.NonTaxableAccountInterest,//(PFTEmpLedgerOld != null ? PFTEmpLedgerOld.NonTaxableAccountInterest : 0) + NontaxInt,
                                    TaxableAccountInterest = PFTEmpLedgerOldset.TaxableAccountInterest,//(PFTEmpLedgerOld != null ? PFTEmpLedgerOld.TaxableAccountInterest : 0) + TaxableaccInt,


                                };
                                PFTEmpLedgerListset.Add(PFTEmpLedgerECRDataBalanceset);//added monthly PF data balance
                            }
                            var EmployeePFTrustset = db.EmployeePFTrust.Include(t => t.PFTEmployeeLedger).Where(t => t.Id == PFTID.Id).SingleOrDefault();
                            // EmployeePFTrust.PFTEmployeeLedger.ToList().AddRange(PFTEmpLedgerList);
                            PFTEmpLedgerListset.AddRange(EmployeePFTrustset.PFTEmployeeLedger);
                            EmployeePFTrustset.PFTEmployeeLedger = PFTEmpLedgerListset;
                            db.EmployeePFTrust.Attach(EmployeePFTrustset);
                            db.Entry(EmployeePFTrustset).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            db.Entry(EmployeePFTrustset).State = System.Data.Entity.EntityState.Detached;

                        }
                        //Final payment will insert in ledger
                        //}
                    }

                    Msg.Add("  Record Updated  ");
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
}