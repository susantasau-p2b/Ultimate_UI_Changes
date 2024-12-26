using Attendance;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Wordprocessing;
using IR;
using Microsoft.Ajax.Utilities;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Logical;
using P2b.Global;
using P2B.PFTRUST;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using P2BUltimate.Security;
using Payroll;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using System.Web.Mvc;

namespace P2BUltimate.Controllers.PtTrust
{
    public class PFTrustMonthlyTransactionController : Controller
    {
        // GET: PtTrustMonthlyTransaction
        public ActionResult Index()
        {
            return View("~/Views/PFTrust/PFTrustMonthlyTransaction/index.cshtml");
        }
        public class ReturnData
        {
            public double ReturnValue { get; set; }
            public int? Errno { get; set; }
            public string ErrMsg { get; set; }
        }


        public ActionResult ChkProcess(string typeofbtn, string month)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                string mPayMonth = month;

                bool selected = false;
                //var query = db.EmployeePFTrust
                //    .Include(e => e.Employee)
                //    .Include(e => e.Employee.ServiceBookDates)
                //     .Include(e => e.PFTEmployeeLedger)
                //      .Include(e => e.PFTEmployeeLedger.Select(x => x.PassbookActivity))
                //      .Where(e => e.Employee.ServiceBookDates.ServiceLastDate == null)
                //     .ToList();


                //foreach (var item in query)
                //{
                var a = db.PFTEmployeeLedger.Where(t => t.MonthYear == mPayMonth && t.PassbookActivity.LookupVal.ToUpper() == "MONTHLY PF POSTING").AsNoTracking().Select(e => e.Id).FirstOrDefault();
                if (a != null)
                {
                    selected = true;
                    //break;
                }
                // }



                var data = new
                {
                    status = selected,
                };
                return Json(data, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult CreatePFMthlyTEmployeeWise(FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string Emp = form["Employee-Table"] == "0" ? "" : form["Employee-Table"];
                string WageMonth = form["UploadMonth"] == "0" ? "" : form["UploadMonth"];
                var salt = db.PFECRR.Where(e => e.Wage_Month == WageMonth).FirstOrDefault();
                List<string> Msg = new List<string>();
                List<P2BUltimate.Process.GlobalProcess.ReturnData> ReturnDataList = new List<P2BUltimate.Process.GlobalProcess.ReturnData>();
                DateTime Dateverifyfinancialyear = Convert.ToDateTime("01/" + WageMonth);
                var Financialyear = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "FINANCIALYEAR".ToUpper() && e.FromDate.Value <= Dateverifyfinancialyear && e.ToDate.Value >= Dateverifyfinancialyear).SingleOrDefault();
                if (Financialyear == null)
                {
                    Msg.Add("For This month please define financial year.");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
                var SalaryTPF = db.SalaryT.Where(e => e.PayMonth == WageMonth && e.ReleaseDate == null).ToList();
                if (SalaryTPF.Count() > 0)
                {
                    Msg.Add(" Please Release the Salary ");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
                if (salt != null)
                {
                    string mPayMonth = salt.Wage_Month;

                    ReturnDataList = P2BUltimate.Process.GlobalProcess.PFMthlyTEmployeeWiseECR(mPayMonth, Emp);
                    foreach (var item in ReturnDataList)
                    {
                        Msg.Add(item.ErrMsg);
                    }
                }
                else
                {
                    Msg.Add(" Salary not Process ");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }


                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

            }
        }

        //public ActionResult UploadPFdataMonthly(string typeofbtn, string month)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        //int WageMonth = month == "0" ? 0 : Convert.ToInt32(month);
        //        string WageMonth = month;
        //        var salt = db.PFECRR.Where(e => e.Wage_Month == WageMonth).FirstOrDefault();
        //        List<string> Msg = new List<string>();
        //        DateTime Dateverifyfinancialyear = Convert.ToDateTime("01/" + WageMonth);
        //        var Financialyear = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "FINANCIALYEAR".ToUpper() && e.FromDate.Value <= Dateverifyfinancialyear && e.ToDate.Value >= Dateverifyfinancialyear).SingleOrDefault();
        //        if (Financialyear == null)
        //        {
        //            Msg.Add("For This month please define financial year.");
        //            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //        }
        //        if (salt != null)
        //        {
        //            string mPayMonth = salt.Wage_Month;

        //            Msg = P2BUltimate.Process.GlobalProcess.UploadECR(mPayMonth);
        //        }
        //        else
        //        {
        //            Msg.Add(" Salary not Process ");
        //            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //        }


        //        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //    }
        //}24022024

        public class P2BGridData
        {
            public int Id { get; set; }
            public string EmpCode { get; set; }
            public string EmpName { get; set; }
            public DateTime PostingDate { get; set; }
            public string MonthYear { get; set; }
            public double OwnPFMonthly { get; set; }
            public double OwnerPFMonthly { get; set; }
            public double VPFAmountMonthly { get; set; }
            public double PensionAmount { get; set; }
            public string PFNO { get; set; }


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

                IEnumerable<P2BGridData> EmployeePFtrust = null;
                List<P2BGridData> model = new List<P2BGridData>();
                P2BGridData view = null;
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

                int company_Id = 0;
                company_Id = Convert.ToInt32(Session["CompId"]);

                // var Empdata1 = db.EmployeePFTrust.Include(e => e.Employee).Include(e => e.Employee.EmpName).Include(e => e.Employee.EmpOffInfo).Include(e => e.Employee.EmpOffInfo.NationalityID).Include(e => e.PFTEmployeeLedger).Include(e => e.PFTEmployeeLedger.Select(x => x.PassbookActivity)).ToList();

                var Empdata = db.EmployeePFTrust
    .Select(e => new
    {
        EmpCode = e.Employee.EmpCode,
        EmpName = e.Employee.EmpName.FullNameFML,
        PFNO = e.Employee.EmpOffInfo.NationalityID.PFNo,
        PFTEmployeeLedger = e.PFTEmployeeLedger.Select(d => new
        {
            Id = d.Id,
            PassbookActivity = d.PassbookActivity.LookupVal,
            MonthYear = d.MonthYear,
            PostingDate = d.PostingDate,
            OwnPFMonthly = d.OwnPFMonthly,
            OwnerPFMonthly = d.OwnerPFMonthly,
            VPFAmountMonthly = d.VPFAmountMonthly,
            PensionAmount = d.PensionAmount,

        }).Where(y => y.PassbookActivity.ToUpper() == "MONTHLY PF POSTING" && y.MonthYear == PayMonth).ToList()
    })

    .ToList();


                //var BindCompList = db.PFTEmployeeLedger.ToList();

                foreach (var z in Empdata)
                {
                    var pftemployeledgerdata = z.PFTEmployeeLedger.Where(e => e.PassbookActivity.ToUpper() == "MONTHLY PF POSTING" && e.MonthYear == PayMonth).OrderBy(e => e.Id).ToList();

                    foreach (var a in pftemployeledgerdata)
                    {

                        //var aa = db.PFTEmployeeLedger.Where(e => e.Id == a.Id).SingleOrDefault();
                        //if (a != null)
                        //{

                        view = new P2BGridData()
                        {
                            Id = a.Id,
                            EmpCode = z.EmpCode,
                            EmpName = z.EmpName,
                            PostingDate = a.PostingDate,
                            MonthYear = a.MonthYear,
                            OwnPFMonthly = a.OwnPFMonthly,
                            OwnerPFMonthly = a.OwnerPFMonthly,
                            VPFAmountMonthly = a.VPFAmountMonthly,
                            PensionAmount = a.PensionAmount,
                            PFNO = z.PFNO == null ? "" : z.PFNO

                        };

                        model.Add(view);
                        // }
                    }

                }

                EmployeePFtrust = model;

                IEnumerable<P2BGridData> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = EmployeePFtrust;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.Id.ToString().Contains(gp.searchString))
                              || (e.EmpCode.ToString().Contains(gp.searchString))
                              || (e.EmpName.ToString().Contains(gp.searchString))
                               || (e.PFNO.ToString().Contains(gp.searchString))
                              || (e.PostingDate.ToString().Contains(gp.searchString))
                               || (e.MonthYear.ToString().Contains(gp.searchString))
                              || (e.OwnPFMonthly.ToString().Contains(gp.searchString))
                              || (e.OwnerPFMonthly.ToString().Contains(gp.searchString))
                              || (e.VPFAmountMonthly.ToString().Contains(gp.searchString))
                              || (e.PensionAmount.ToString().Contains(gp.searchString))

                                ).Select(a => new Object[] { a.EmpCode, a.EmpName, a.PFNO, a.OwnPFMonthly, a.OwnerPFMonthly, a.PensionAmount, a.VPFAmountMonthly, a.MonthYear, a.PostingDate.ToShortDateString(), a.Id }).ToList();
                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.EmpCode, a.EmpName, a.PFNO, a.OwnPFMonthly, a.OwnerPFMonthly, a.PensionAmount, a.VPFAmountMonthly, a.MonthYear, a.PostingDate.ToShortDateString(), a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = EmployeePFtrust;
                    Func<P2BGridData, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id.ToString() :
                                         gp.sidx == "EmpCode" ? c.EmpCode.ToString() :
                                         gp.sidx == "EmpName" ? c.EmpName.ToString() :
                                         gp.sidx == "OwnPFMonthly" ? c.OwnerPFMonthly.ToString() :
                                         gp.sidx == "OwnerPFMonthly" ? c.OwnerPFMonthly.ToString() :
                                         gp.sidx == "PensionAmount" ? c.PensionAmount.ToString() :
                                         gp.sidx == "VPFAMOUNT" ? c.VPFAmountMonthly.ToString() :

                                         "");

                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.EmpCode, a.EmpName, a.PFNO, a.OwnPFMonthly, a.OwnerPFMonthly, a.PensionAmount, a.VPFAmountMonthly, a.MonthYear, a.PostingDate.ToShortDateString(), a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.EmpCode, a.EmpName, a.PFNO, a.OwnPFMonthly, a.OwnerPFMonthly, a.PensionAmount, a.VPFAmountMonthly, a.MonthYear, a.PostingDate.ToShortDateString(), a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.EmpCode, a.EmpName, a.PFNO, a.OwnPFMonthly, a.OwnerPFMonthly, a.PensionAmount, a.VPFAmountMonthly, a.MonthYear, a.PostingDate.ToShortDateString(), a.Id }).ToList();
                    }
                    totalRecords = EmployeePFtrust.Count();
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

        #region ProcessForNewEmpPFTrust

        [HttpPost]
        public ActionResult ProcessForNewEmpPFTrust()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    int? getCompanyId = Convert.ToInt32(SessionManager.CompanyId);

                    var getPFTEmpLedgercount = db.PFTEmployeeLedger.FirstOrDefault();
                    if (getPFTEmpLedgercount == null)
                    {
                        return JavaScript("alert('Please Upload PF Balance and Try Again...!!!')");
                    }

                    var exempted = db.PFMaster.Include(e => e.PFTrustType).Where(x => x.PFTrustType.LookupVal.ToUpper() == "EXEMPTED" && x.EndDate == null).Select(a => a.EstablishmentID).ToList();
                    var getPostingMonth = string.Empty;

                    var getTotalWorkingEmp = db.Employee.Select(a => new
                    {
                        oServiceLastDate = a.ServiceBookDates.ServiceLastDate,
                        oEmpcode = a.EmpCode,
                        oEmpIds = a.Id.ToString(),
                        oPFAppli = a.EmpOffInfo.PFAppl,
                        oPFEstablimentId = a.EmpOffInfo.PFTrust_EstablishmentId,

                    }).Where(e => e.oServiceLastDate == null && e.oPFAppli == true && exempted.Contains(e.oPFEstablimentId)).ToList();
                    var getitemTWEList = new List<int?>();
                    var getEmployeePfTrust = new List<int?>();
                    foreach (var itemTWE in getTotalWorkingEmp)
                    {
                        int? empids = Convert.ToInt32(itemTWE.oEmpIds);
                        getitemTWEList.Add(empids);
                        var getExistEmployeePfTrust = db.EmployeePFTrust.Include(e => e.PFTEmployeeLedger).Where(e => e.Employee_Id == empids && e.PFTEmployeeLedger.Count() > 0).Select(b => b.Employee_Id).ToList();
                        getEmployeePfTrust.AddRange(getExistEmployeePfTrust);

                    }

                    var getNewJoinEmplist = getitemTWEList.Except(getEmployeePfTrust).ToList();

                    EmployeePFTrust oEmployeePFTrustObj = null;
                    List<PFTEmployeeLedger> PFTEmployeeLedgerList = new List<PFTEmployeeLedger>();

                    if (getNewJoinEmplist.Count() > 0)
                    {
                        foreach (var itemNJE in getNewJoinEmplist)
                        {
                            using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(1, 0, 0)))
                            {
                                string empids = itemNJE.ToString();

                                var empPayrollId = db.EmployeePayroll.Where(e => e.Employee_Id == itemNJE).SingleOrDefault().Id;
                                var SalaryTchk = db.SalaryT.Where(e => e.EmployeePayroll_Id == empPayrollId).OrderByDescending(e => e.Id).FirstOrDefault();
                                if (SalaryTchk != null)
                                {
                                    getPostingMonth = db.SalaryT.Select(b => new
                                    {
                                        oSalaryId = b.Id,
                                        oSalaryTEmpid = b.EmployeePayroll.Employee_Id.ToString(),
                                        oSalaryTpaymonth = b.PayMonth,
                                    }).Where(e => e.oSalaryTEmpid == empids).OrderByDescending(e => e.oSalaryId).FirstOrDefault().oSalaryTpaymonth;
                                }
                                getPostingMonth = getPostingMonth != "" ? getPostingMonth : "";

                                if (!string.IsNullOrEmpty(getPostingMonth))
                                {
                                    var PostingandCalcDate = Convert.ToDateTime("01/" + getPostingMonth).AddMonths(-1).Date;
                                    PFTEmployeeLedger oPFTEmployeeLedger = new PFTEmployeeLedger()
                                    {
                                        OwnOpenBal = 0,
                                        OwnerOpenBal = 0,
                                        OwnIntOpenBal = 0,
                                        OwnerIntOpenBal = 0,
                                        VPFOpenBal = 0,
                                        VPFIntOpenBal = 0,
                                        TotalIntOpenBal = 0,
                                        OwnCloseBal = 0,
                                        OwnerCloseBal = 0,
                                        OwnIntCloseBal = 0,
                                        OwnerIntCloseBal = 0,
                                        VPFCloseBal = 0,
                                        VPFIntCloseBal = 0,
                                        TotalIntCloseBal = 0,
                                        PFOpenBal = 0,
                                        PFCloseBal = 0,
                                        PFIntOpenBal = 0,
                                        PFIntCloseBal = 0,
                                        PassbookActivity = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "512").FirstOrDefault().LookupValues.Where(a => a.LookupVal.ToUpper() == "PF Balance".ToUpper()).SingleOrDefault(),
                                        PostingDate = PostingandCalcDate,
                                        CalcDate = PostingandCalcDate,
                                        MonthYear = Convert.ToDateTime(PostingandCalcDate).ToString("MM/yyyy"),
                                        IsTDSAppl = false,
                                        DBTrack = new DBTrack() { Action = "C", CreatedOn = DateTime.Now, CreatedBy = SessionManager.EmpId },
                                        Narration = "Opening Balance"

                                    };
                                    db.PFTEmployeeLedger.Add(oPFTEmployeeLedger);
                                    db.SaveChanges();
                                    PFTEmployeeLedgerList.Add(oPFTEmployeeLedger);
                                    var dbEmployeePFTrust = db.EmployeePFTrust.Where(e => e.Employee_Id == itemNJE).SingleOrDefault();
                                    if (dbEmployeePFTrust == null)
                                    {
                                        oEmployeePFTrustObj = new EmployeePFTrust()
                                        {
                                            Employee = db.Employee.Find(itemNJE),
                                            CompanyPFTrust = db.CompanyPFTrust.Where(e => e.Company_Id == getCompanyId).FirstOrDefault(),
                                            PFTEmployeeLedger = PFTEmployeeLedgerList,
                                            DBTrack = new DBTrack() { Action = "C", CreatedOn = DateTime.Now, CreatedBy = SessionManager.EmpId },
                                        };
                                        db.EmployeePFTrust.Add(oEmployeePFTrustObj);
                                        db.SaveChanges();
                                        db.Entry(oEmployeePFTrustObj).State = System.Data.Entity.EntityState.Detached;
                                        PFTEmployeeLedgerList.Clear();
                                        
                                    }
                                    else
                                    {
                                        dbEmployeePFTrust.Id = dbEmployeePFTrust.Id;
                                        dbEmployeePFTrust.PFTEmployeeLedger = PFTEmployeeLedgerList;
                                        dbEmployeePFTrust.DBTrack = new DBTrack() { Action = "M", CreatedOn = DateTime.Now, CreatedBy = SessionManager.EmpId };
                                        db.EmployeePFTrust.Attach(dbEmployeePFTrust);
                                        db.SaveChanges();
                                        db.Entry(dbEmployeePFTrust).State = System.Data.Entity.EntityState.Modified;
                                        db.Entry(dbEmployeePFTrust).State = System.Data.Entity.EntityState.Detached;
                                        PFTEmployeeLedgerList.Clear();
                                    }                                  
                                    
                                }
                                else
                                {
                                    return Json(new JsonResult { Data = "SalaryT not found." }, JsonRequestBehavior.AllowGet);
                                }
                                ts.Complete();
                            }
                        }

                        return Json(new JsonResult { Data = "Created." }, JsonRequestBehavior.AllowGet);

                    }
                    else
                    {
                        return Json(new JsonResult { Data = "Record not found for Exempted." }, JsonRequestBehavior.AllowGet);
                    }


                }
                catch (Exception Ex)
                {

                    throw Ex;

                }

            }

        }

        #endregion


    }
}