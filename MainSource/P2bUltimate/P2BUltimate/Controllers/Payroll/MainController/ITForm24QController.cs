using P2b.Global;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using P2BUltimate.Process;
using Payroll;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using P2BUltimate.Security;
using System.Data.Entity.Core.Objects;
using System.Web.UI.WebControls;
using System.IO;
using System.Web.UI;

namespace P2BUltimate.Controllers.Payroll.MainController
{
    public class ITForm24QController : Controller
    {
        private DataBaseContext db = new DataBaseContext();

        public ActionResult Index()
        {
            return View("~/Views/Payroll/MainViews/ITForm24Q/Index.cshtml");
        }

        public ActionResult ChkProcess(string typeofbtn, string month)
        {
            var finanialyear = new P2b.Global.Calendar();
            if (month != null)
            {
                finanialyear = db.Calendar.Find(int.Parse(month));
            }
            bool selected = false;

            var query = db.ITForm24QData.Include(e => e.FinancialYear).ToList();
            //var financialyear = query.Select(e => e.FinancialYear == finanialyear).SingleOrDefault();
            var financialyearR = query.Where(f => f.FinancialYear == finanialyear);
            if (financialyearR.Count() > 0) //chgd by rekha09122017
            {
                selected = true;
            }
            var data = new
            {
                status = selected,
            };
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ChkItprojection(string month)
        {
            // var finanialyear = new P2b.Global.Calendar();
            if (month != null)
            {
                //finanialyear = db.Calendar.Find(int.Parse(month));
                int finid = Convert.ToInt32(month);
                bool selected = false;
                try
                {
                    var query = db.ITProjection.Include(e => e.FinancialYear).Where(e => e.FinancialYear.Id == finid).Select(e => e.ProjectionDate).ToList();
                    if (query.Count() > 0)
                    {

                        DateTime? maxdate = query.OrderByDescending(t => t).First();
                        selected = true;
                        var data = new
                        {
                            status = selected,
                            date = maxdate.Value.ToShortDateString()
                        };
                        return Json(data, JsonRequestBehavior.AllowGet);
                    }

                }
                catch (Exception e)
                {
                    throw e;
                    return null;
                }
            }
            return null;
        }

        public ActionResult GetSignPersonList(string data, string data2)
        {
            var qurey = db.ITForm16SigningPerson.ToList();
            data2 = db.ITForm16SigningPerson.Where(e => e.IsDefault == true).SingleOrDefault() != null ? db.ITForm16SigningPerson.Where(e => e.IsDefault == true).SingleOrDefault().Id.ToString() : "0";
            var selected = (Object)null;
            if (data2 != "" && data != "0" && data2 != "0")
            {
                selected = Convert.ToInt32(data2);
            }

            SelectList s = new SelectList(qurey, "Id", "SigningPersonFullName", selected);
            return Json(s, JsonRequestBehavior.AllowGet);
        }

        public static EmployeePayroll _returnEmployeePayroll_SalaryT24(Int32 OEmployeePayroll)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                return db.EmployeePayroll
                      .Include(e => e.SalaryT)
                      .Include(e => e.YearlyPaymentT)
                      .Where(e => e.Id == OEmployeePayroll).AsParallel()
                      .SingleOrDefault();

            }
        }
        public ActionResult Create(ITForm24QData IT, FormCollection form) //Create submit
        {
            List<string> Msg = new List<string>();
            try
            {
                DateTime processdate = DateTime.Now;
                string Emp = form["Employee-Table"] == "0" ? "" : form["Employee-Table"];
                string FinancialYear = form["txtfinancialyear_id"] == "0" ? "" : form["txtfinancialyear_id"];
                string finance = form["FinancialYearList"] == "0" ? "" : form["FinancialYearList"];
                int SigningPerson_Id = form["SignPersonlist"] == "0" ? 0 : Convert.ToInt32(form["SignPersonlist"]);
                int QuarterName_Id = form["QuarterNamelist"] == "0" ? 0 : Convert.ToInt32(form["QuarterNamelist"]);
                string ItProjectionProcess = form["ItproJectionProcess"];
                Boolean AnnexureII = Convert.ToBoolean(form["IsCancelled1"]);
                Boolean Tax = Convert.ToBoolean(form["IsCancelled2"]);

                int Fiyr = int.Parse(finance);

                var OFinancialYear = db.Calendar.Where(e => e.Id == Fiyr).SingleOrDefault();
                DateTime FromPeriod = Convert.ToDateTime(OFinancialYear.FromDate);
                DateTime ToPeriod = Convert.ToDateTime(OFinancialYear.ToDate);

                if (AnnexureII != null)
                {

                }
                if (finance != null && finance != "")
                {
                    var value = db.Calendar.Find(int.Parse(finance));
                    IT.FinancialYear = value;

                }
                int CompId = 0;
                if (!String.IsNullOrEmpty(Session["CompId"].ToString()))
                {
                    CompId = int.Parse(Session["CompId"].ToString());
                }

                List<int> ids = null;

                //else
                //{
                //    Msg.Add("Kindly select employee");
                //    return Json(new { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                //}

                if (SigningPerson_Id == 0)
                {
                    Msg.Add("Kindly Select Signing Authority.");
                    return Json(new { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }




                //if (db.ITForm16Quarter.Where(e => e.QuarterFromDate >= FromPeriod && e.QuarterToDate <= ToPeriod).Any() == false)
                //{
                //    Msg.Add("Kindly process IT Challan & Quarterly Returns before Form16.");
                //    return Json(new { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                //}


                //Employee OEmployee = null;
                EmployeePayroll OEmployeePayroll = null;
                CompanyPayroll OCompanyPayroll = null;

                Employee OEmployeeIT = null;
                EmployeePayroll OEmployeePayrollIT = null;
                //  EmployeePayroll OEmployeePayrollITchk = null;

                /////new 
                var QuarterNamefff = db.ITForm16Quarter.Include(q => q.QuarterName).Where(e => e.QuarterName.Id == QuarterName_Id && e.QuarterFromDate >= FromPeriod && e.QuarterToDate <= ToPeriod).SingleOrDefault();
                string QuarterName1 = "";
                if (QuarterNamefff != null && QuarterNamefff.QuarterName != null)
                {
                    QuarterName1 = QuarterNamefff.QuarterName.LookupVal.ToString();
                }
                else
                {
                    Msg.Add("Please Define ITForm16Quarter");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }


                DateTime mQfrom = DateTime.Now;
                DateTime mQto = DateTime.Now;



                if (QuarterName1.ToUpper() == "QUARTER1")
                {
                    mQfrom = FromPeriod;
                    mQto = FromPeriod.AddMonths(3).AddDays(-1);
                }
                else if (QuarterName1.ToUpper() == "QUARTER2")
                {
                    mQfrom = FromPeriod.AddMonths(3);
                    mQto = mQfrom.AddMonths(3).AddDays(-1);
                }
                else if (QuarterName1.ToUpper() == "QUARTER3")
                {
                    mQfrom = FromPeriod.AddMonths(6);
                    mQto = mQfrom.AddMonths(3).AddDays(-1);
                }
                else if (QuarterName1.ToUpper() == "QUARTER4")
                {
                    mQfrom = FromPeriod.AddMonths(9);
                    mQto = mQfrom.AddMonths(3).AddDays(-1);
                }

                List<int> idsE = new List<int>();



                ////new added
                if (Tax == true)
                {
                    var OYearlypaymentt = db.EmployeePayroll.Include(t => t.YearlyPaymentT).Include(t => t.ITaxTransT).AsNoTracking().AsParallel().ToList();
                    foreach (var item in OYearlypaymentt)
                    {
                        if (item.YearlyPaymentT.Any(e => e.TDSAmount > 0 && Convert.ToDateTime("01/" + e.PayMonth) >= mQfrom && Convert.ToDateTime("01/" + e.PayMonth) <= mQto))
                        {
                            idsE.Add(item.Id);
                        }
                    }

                    // var OItTranst = db.EmployeePayroll.AsNoTracking().AsParallel().ToList();
                    foreach (var item in OYearlypaymentt)
                    {
                        var test = item.ITaxTransT.Where(e => Convert.ToDateTime("01/" + e.PayMonth) >= mQfrom && Convert.ToDateTime("01/" + e.PayMonth) <= mQto).ToList();
                        foreach (var t1 in test)
                        {
                            if (t1.TaxPaid > 0)
                            {
                                idsE.Add(item.Id);
                            }
                        }
                    }
                    ids = idsE.Distinct().ToList();
                }
                else
                {
                    ids = db.EmployeePayroll.Select(r => r.Id).ToList();
                }
                //List<int> idsEast = new List<int>();
                //idsEast.Add(145);
                //idsEast.Add(96);
                ////idsEast.Add(95);
                ////idsEast.Add(469);
                ////idsEast.Add(1264);
                //ids = db.EmployeePayroll.Where(r => idsEast.Contains(r.Id)).Select(r => r.Id).ToList();


                ITForm16Quarter ITForm16Q = db.ITForm16Quarter.Where(e => e.QuarterName.Id == QuarterName_Id && e.QuarterFromDate >= FromPeriod && e.QuarterToDate <= ToPeriod).SingleOrDefault();
                var OForm24QDel = db.ITForm24QData.Include(e => e.ITForm24QDataDetails).Where(e => e.FinancialYear.Id == IT.FinancialYear.Id && e.ITForm16Quarter.Id == ITForm16Q.Id).Select(e => e.Id).ToList();
                var OForm24QDel1 = db.ITForm24QData.Include(e => e.ITForm24QDataDetails).Where(e => e.FinancialYear.Id == IT.FinancialYear.Id && e.ITForm16Quarter.Id == ITForm16Q.Id).ToList();
                if (OForm24QDel != null && OForm24QDel1 != null)
                {
                    var OForm24QDetails = db.ITForm24QDataDetails.Where(e => OForm24QDel.Contains(e.Id)).ToList();
                    if (OForm24QDetails != null)
                    {
                        db.ITForm24QDataDetails.RemoveRange(OForm24QDetails);
                        db.ITForm24QData.RemoveRange(OForm24QDel1);
                    }
                    db.SaveChanges();
                }

                bool deductor = false;
                bool Challan = false;
                for (DateTime dt = mQfrom; dt <= mQto; dt = dt.AddMonths(1))
                {
                    DateTime dtfrom = dt;
                    DateTime dtTO = dtfrom.AddMonths(1).AddDays(-1);
                    foreach (var i in ids)
                    {
                        OEmployeePayroll = db.EmployeePayroll.Include(e => e.Employee)
                            .Include(e => e.Employee.GeoStruct)
                            .Include(e => e.Employee.GeoStruct.Company).AsNoTracking()
                            .Where(e => e.Id == i).SingleOrDefault();

                      //  OCompanyPayroll = db.CompanyPayroll.Where(e => e.Company.Id == OEmployeePayroll.Employee.GeoStruct.Company.Id).SingleOrDefault();

                        //if (AnnexureII == true)
                        //{
                        //    OEmployeePayrollIT = _returnITInvestmentPayment(i);

                        //}

                        //EmployeePayroll OSalaryTEmp24 = IncomeTaxCalc._returnEmployeePayroll_SalaryT(i);
                        EmployeePayroll OSalaryTEmp24 = _returnEmployeePayroll_SalaryT24(i);

                        var OEmpChksal = OSalaryTEmp24.SalaryT.Where(e => Convert.ToDateTime("01/" + e.PayMonth) >= FromPeriod && Convert.ToDateTime("01/" + e.PayMonth) <= ToPeriod).ToList();
                        var OEmpChkYearly = OSalaryTEmp24.YearlyPaymentT.Where(e => e.FromPeriod >= FromPeriod && e.ToPeriod <= ToPeriod).ToList();

                        if (OEmpChksal.Count > 0 || OEmpChkYearly.Count > 0)
                        {


                            using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                               new System.TimeSpan(0, 30, 0)))
                            {
                                //if (AnnexureII == true)
                                //{
                                //    IncomeTaxCalc.ITCalculation(OEmployeePayrollIT, CompId, IT.FinancialYear.Id, FromPeriod, ToPeriod, processdate, null, null, 1, 0);
                                //}
                                bool AnnexureII1 = false;

                                //IncomeTaxCalc.ITForm16PartB(OEmployeePayroll.Id, CompId, IT.FinancialYear.Id, FromPeriod, ToPeriod, processdate, SigningPerson_Id);

                                IncomeTaxCalc.ITForm24Q(OEmployeePayroll, CompId, IT.FinancialYear.Id, FromPeriod, ToPeriod, processdate, SigningPerson_Id, QuarterName_Id, AnnexureII1, Tax, dtfrom, dtTO, deductor, Challan);
                                deductor = true;
                                Challan = true;
                                ts.Complete();

                            }

                        }
                        //
                        //    }
                        //}

                        //
                    }
                }
                // for anneaxure true
                if (AnnexureII == true)
                {
                    foreach (var i in ids)
                    {
                        OEmployeePayroll = db.EmployeePayroll.Include(e => e.Employee)
                            .Include(e => e.Employee.GeoStruct)
                            .Include(e => e.Employee.GeoStruct.Company).AsNoTracking()
                            .Where(e => e.Id == i).SingleOrDefault();
                       // OCompanyPayroll = db.CompanyPayroll.Where(e => e.Company.Id == OEmployeePayroll.Employee.GeoStruct.Company.Id).SingleOrDefault();
                        if (ItProjectionProcess == "true")
                        {
                            OEmployeePayrollIT = _returnITInvestmentPayment(i);
                            OEmployeePayrollIT.RegimiScheme = OEmployeePayrollIT.RegimiScheme.Where(e => e.FinancialYear_Id == IT.FinancialYear.Id).ToList();
                        }

                        //EmployeePayroll OSalaryTEmp24 = IncomeTaxCalc._returnEmployeePayroll_SalaryT(i);
                        EmployeePayroll OSalaryTEmp24 = _returnEmployeePayroll_SalaryT24(i);

                        var OEmpChksal = OSalaryTEmp24.SalaryT.Where(e => Convert.ToDateTime("01/" + e.PayMonth) >= FromPeriod && Convert.ToDateTime("01/" + e.PayMonth) <= ToPeriod).ToList();
                        var OEmpChkYearly = OSalaryTEmp24.YearlyPaymentT.Where(e => e.FromPeriod >= FromPeriod && e.ToPeriod <= ToPeriod).ToList();

                        if (OEmpChksal.Count > 0 || OEmpChkYearly.Count > 0)
                        {
                            if (ItProjectionProcess == "true")
                            {
                                using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                                   new System.TimeSpan(0, 30, 0)))
                                {

                                    IncomeTaxCalc.ITCalculation(OEmployeePayrollIT, CompId, IT.FinancialYear.Id, FromPeriod, ToPeriod, processdate, null, null, 1, 0);
                                    ts.Complete();

                                }
                            }
                            using (TransactionScope ts1 = new TransactionScope(TransactionScopeOption.Required,
                                               new System.TimeSpan(0, 30, 0)))
                            {
                                //IncomeTaxCalc.ITForm16PartB(OEmployeePayroll.Id, CompId, IT.FinancialYear.Id, FromPeriod, ToPeriod, processdate, SigningPerson_Id);
                                IncomeTaxCalc.ITForm24Q(OEmployeePayroll, CompId, IT.FinancialYear.Id, FromPeriod, ToPeriod, processdate, SigningPerson_Id, QuarterName_Id, AnnexureII, Tax, FromPeriod, ToPeriod, deductor, Challan);
                                ts1.Complete();
                            }

                        }
                    }
                }


                Msg.Add("Data Saved successfully");
                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
            }

            catch (Exception ex)
            {
                // List<string> Msg = new List<string>();
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



        }

        public Employee _returnEmployeePayroll(Int32 Emp)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                return db.Employee.Include(e => e.GeoStruct)
                    .Include(e => e.GeoStruct.Company)
                    .Include(e => e.FuncStruct)
                    .Include(e => e.PayStruct)
                    .Include(e => e.ServiceBookDates)
                    .Where(r => r.Id == Emp).AsParallel().SingleOrDefault();
            }
        }



        public EmployeePayroll _returnITInvestmentPayment(Int32 Emp)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                return db.EmployeePayroll.Where(e => e.Id == Emp).Include(e => e.Employee)
                    //.Include(e => e.EmpSalStruct)
                    //.Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails))
                    // .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead)))
                    // .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.Type)))
                    // .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.SalHeadOperationType)))
                    // .Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.ProcessType)))
                    //.Include(e => e.EmpSalStruct.Select(r => r.EmpSalStructDetails.Select(t => t.SalaryHead.Frequency)))
                         .Include(e => e.Employee.EmpOffInfo)
                         .Include(e => e.Employee.EmpOffInfo.NationalityID)
                         .Include(e => e.Employee.Gender)
                           .Include(e => e.Employee.ServiceBookDates)
                            .Include(e => e.Employee.EmpName)
                            .Include(e => e.ITProjection)
                            .Include(e => e.ITProjection.Select(r => r.FinancialYear))
                             .Include(e => e.RegimiScheme).Include(e => e.RegimiScheme.Select(y => y.Scheme))
                    //    .Include(e => e.ITInvestmentPayment)
                    //    .Include(e => e.ITInvestmentPayment.Select(r => r.FinancialYear))
                    //   .Include(e => e.ITInvestmentPayment.Select(r => r.ITSection))
                    //  .Include(e => e.ITInvestmentPayment.Select(r => r.ITSection.ITSectionList))
                    //  .Include(e => e.ITInvestmentPayment.Select(r => r.ITSection.ITSectionListType))
                    //  .Include(e => e.ITInvestmentPayment.Select(r => r.ITSection.ITSectionList))//added 17042017 by prashant
                    //   .Include(e => e.ITInvestmentPayment.Select(r => r.ITSubInvestmentPayment))
                    //   .Include(e => e.ITInvestmentPayment.Select(r => r.LoanAdvanceHead))
                    //  .Include(e => e.ITInvestmentPayment.Select(r => r.LoanAdvanceHead.ITLoan))
                    //  .Include(e => e.ITInvestmentPayment.Select(r => r.LoanAdvanceHead.ITSection))
                    //  .Include(e => e.ITInvestmentPayment.Select(r => r.LoanAdvanceHead.LoanAdvancePolicy))
                    // .Include(e => e.ITInvestmentPayment.Select(r => r.ITInvestment))
                    //  .Include(e => e.ITInvestmentPayment.Select(r => r.ITInvestment.SalaryHead))
                    //  .Include(e => e.ITReliefPayment)
                    // .Include(e => e.LoanAdvRequest)
                    //  .Include(e => e.LoanAdvRequest.Select(r => r.LoanAdvRepaymentT))
                    // .Include(e => e.LoanAdvRequest.Select(r => r.LoanAdvanceHead))
                    //.Include(e => e.ITReliefPayment.Select(r => r.ITSection))
                         .AsParallel().SingleOrDefault();

            }
        }


        public ActionResult GetCalendarDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "FINANCIALYEAR").ToList();
                IEnumerable<P2b.Global.Calendar> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.Calendar.ToList().Where(d => d.FullDetails.Contains(data));
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


        public class P2BGridData
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
            public string HeaderCol { get; set; }
            public string ActualAmount { get; set; }
            public string QualifyAmount { get; set; }
            public string QuarterName { get; set; }
            public string DeductibleAmount { get; set; }
            public string FinalAmount { get; set; }
            public string Form16Header { get; set; }
            public string Form24Header { get; set; }
            public string FinancialYear { get; set; }
            public int PickupId { get; set; }
            public double ProjectedAmount { get; set; }
            public double ProjectedQualifyingAmount { get; set; }
            public string ReportDate { get; set; }
            public double QualifiedAmount { get; set; }
            public int SalayHead { get; set; }
            public string Section { get; set; }
            public string SectionType { get; set; }
            public string SubChapter { get; set; }
            public double TDSComponents { get; set; }
            public DateTime? FromPeriod { get; set; }
            public DateTime? Toperiod { get; set; }
            public string title { get; set; }
            public bool Islock { get; set; }
            public string Narration { get; set; }
        }

        public ActionResult P2BGrid(P2BGrid_Parameters gp)
        {
            try
            {
                int Id = Convert.ToInt32(gp.id);
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                var jsonData = (Object)null;

                IEnumerable<P2BGridData> ITProjectionList = null;
                List<P2BGridData> model = new List<P2BGridData>();
                P2BGridData view = null;
                string PayMonth = "";

                if (gp.filter != null)
                    PayMonth = gp.filter;
                if (PayMonth != null && PayMonth != "")
                {

                    var financialyear = db.Calendar.Find(int.Parse(PayMonth));

                    //var Sal = db.CompanyPayroll.Where(e => e.Company.Id == id).Include(e => e.SalHeadFormula).Select(e => e.SalHeadFormula).SingleOrDefault();

                    //var all = Sal.GroupBy(e => e.Name).Select(e => e.FirstOrDefault()).ToList();

                    var BindEmpList = db.ITForm24QData.Include(e => e.ITForm16Quarter).Include(e => e.ITForm16Quarter.QuarterName).Distinct().Where(e => e.FinancialYear.Id == financialyear.Id)
                         .ToList();
                    var test = BindEmpList.GroupBy(r => r.ITForm16Quarter.QuarterName.LookupVal).ToList();

                    if (test != null)
                    {
                        foreach (var item in test)
                        {
                            var b = item.FirstOrDefault();
                            if (b != null)
                            {
                                view = new P2BGridData()
                                {
                                    Id = b.ITForm16Quarter.QuarterName.Id,
                                    //Code = item.Employee.EmpCode,
                                    //Name = item.Employee.EmpName.FullNameFML,

                                    //FromPeriod = all.FinancialYear.FromDate,
                                    //Toperiod = all.FinancialYear.ToDate,
                                    //Islock = all.IsLocked,
                                    QuarterName = b.ITForm16Quarter.QuarterName.LookupVal,
                                    ReportDate = b.ReportDate.Value.ToString("dd/MM/yyyy")
                                };
                                model.Add(view);
                            }

                        }
                    }
                }
                else
                {
                    List<string> Msgu = new List<string>();
                    Msgu.Add("  Financial Year Not Selected ");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msgu }, JsonRequestBehavior.AllowGet);
                    //return Json(new Object[] { "", "", "Financial Year Not Selected", JsonRequestBehavior.AllowGet });
                }
                ITProjectionList = model;

                IEnumerable<P2BGridData> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = ITProjectionList;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e => (e.QuarterName.ToUpper().ToString().Contains(gp.searchString.ToUpper()))
                                || (e.ReportDate.ToString().Contains(gp.searchString))
                                || (e.Id.ToString().Contains(gp.searchString))
                                ).Select(a => new Object[] { a.QuarterName, a.ReportDate, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.QuarterName, a.ReportDate, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = ITProjectionList;
                    Func<P2BGridData, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "QuarterName" ? c.QuarterName.ToString() :
                                         gp.sidx == "ReportDate" ? c.ReportDate.ToString() :
                                         "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.QuarterName, a.ReportDate, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.QuarterName, a.ReportDate, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.QuarterName, a.ReportDate, a.Id }).ToList();
                    }
                    totalRecords = ITProjectionList.Count();
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
                List<string> Msg = new List<string>();
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



        public ActionResult ReleaseProcess(string forwardata, string PayMonth)
        {
            List<string> Msg = new List<string>();
            try
            {
                var Ofinanialyear = db.Calendar.Find(int.Parse(PayMonth));
                List<int> ids = null;
                if (forwardata == "false" || forwardata == null || forwardata == "0")
                {
                    Msg.Add(" Unable To Forward Data  ");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    //  return this.Json(new Object[] { "", "", "", JsonRequestBehavior.AllowGet });
                }
                if (forwardata != null && forwardata != "0" && forwardata != "false")
                {
                    ids = Utility.StringIdsToListIds(forwardata);
                }


                foreach (var i in ids)
                {

                    using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required,
                                       new System.TimeSpan(0, 30, 0)))
                    {

                        using (DataBaseContext db2 = new DataBaseContext())
                        {

                            var ITProject = db.ITForm16Data.Where(e => e.FinancialYear.Id == Ofinanialyear.Id).ToList();

                            if (ITProject != null)
                            {
                                foreach (var a in ITProject)
                                {
                                    a.IsLocked = true;
                                    db.ITForm16Data.Attach(a);
                                    db.Entry(a).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    TempData["RowVersion"] = a.RowVersion;

                                }
                                //db.Entry(ITProject).State = System.Data.Entity.EntityState.Detached;
                            }
                        }
                        ts.Complete();

                    }
                    return Json(new { success = true, responseText = "ITForm16 has been Locked." }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
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
            return View();
        }


        public ActionResult Edit(int data)
        {

            var Q = db.ITProjection.Include(e => e.FinancialYear)
                   .Where(e => e.Id == data).Select(e => new
                   {

                       ActualAmount = e.ActualAmount,
                       ActualQualifyingAmount = e.ActualQualifyingAmount,
                       ChapterName = e.ChapterName,
                       FinancialYear_Id = e.FinancialYear.Id,
                       FinancialYear_FullDetails = e.FinancialYear.FullDetails,
                       Form16Header = e.Form16Header,
                       Form24Header = e.Form24Header,
                       FromPeriod = e.FromPeriod,
                       IsLocked = e.IsLocked,
                       Narration = e.Narration,
                       PickupId = e.PickupId,
                       ProjectedAmount = e.ProjectedAmount,
                       ProjectedQualifyingAmount = e.ProjectedQualifyingAmount,
                       ProjectionDate = e.ProjectionDate,
                       QualifiedAmount = e.QualifiedAmount,
                       SalayHead = e.SalayHead,
                       Section = e.Section,
                       SectionType = e.SectionType,
                       SubChapter = e.SubChapter,
                       TDSComponents = e.TDSComponents,
                       Tiltle = e.Tiltle,
                       ToPeriod = e.ToPeriod


                   }).ToList();

            var Corp = db.ITProjection.Find(data);
            //TempData["RowVersion"] = Corp.RowVersion;
            // var Auth = Corp.DBTrack.IsModified;
            return Json(new Object[] { Q, "", "", "", JsonRequestBehavior.AllowGet });


        }


        public ActionResult ViewProjection(P2BGrid_Parameters gp)//, string data
        {
            int Id = Convert.ToInt32(gp.id);
            try
            {
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                var jsonData = (Object)null;

                IEnumerable<P2BGridData> ITProjectionList = null;
                List<P2BGridData> model = new List<P2BGridData>();
                P2BGridData view = null;
                string PayMonth = "";


                var BindEmpList = db.EmployeePayroll.Include(e => e.Employee.EmpName).Include(e => e.ITForm16Data)
                    .Include(e => e.ITForm16Data.Select(t => t.FinancialYear)).Include(e => e.ITForm16Data.Select(r => r.ITForm16DataDetails))

                                  .Where(e => e.ITForm16Data.Any(t => t.Id == Id))
                                  .ToList();

                foreach (var z in BindEmpList)
                {
                    if (z.ITForm16Data != null)
                    {
                        var O12BADet = z.ITForm16Data.Select(r => r.ITForm16DataDetails).ToList();
                        foreach (var Sal in O12BADet)
                        {
                            foreach (var det in Sal)
                            {
                                view = new P2BGridData()
                                {
                                    Id = z.Employee.Id,
                                    Code = z.Employee.EmpCode,
                                    Name = z.Employee.EmpName.FullNameFML,
                                    HeaderCol = det.HeaderCol1,
                                    ActualAmount = det.ActualAmountCol2,
                                    QualifyAmount = det.QualifyAmountCol3,
                                    DeductibleAmount = det.DeductibleAmountCol4,
                                    FinalAmount = det.FinalAmountCol5
                                };
                                model.Add(view);
                            }



                        }
                    }

                }



                ITProjectionList = model;

                IEnumerable<P2BGridData> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = ITProjectionList;
                    if (gp.searchOper.Equals("eq"))
                    {
                        jsonData = IE.Where(e =>  (e.Code.ToString().Contains(gp.searchString))
                                || (e.Name.ToString().Contains(gp.searchString))
                                || (e.Id.ToString().Contains(gp.searchString))
                                ).Select(a => new Object[] { a.Code, a.Name, a.Id }).ToList();
                        //jsonData = IE.Select(a => new { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Code, a.HeaderCol, a.ActualAmount, a.QualifyAmount, a.DeductibleAmount, a.FinalAmount, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = ITProjectionList;
                    Func<P2BGridData, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "Code" ? c.Code.ToString() :
                                         gp.sidx == "Name" ? c.Name.ToString() :
                                         "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Code, a.HeaderCol, a.ActualAmount, a.QualifyAmount, a.DeductibleAmount, a.FinalAmount, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Code, a.HeaderCol, a.ActualAmount, a.QualifyAmount, a.DeductibleAmount, a.FinalAmount, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Code, a.HeaderCol, a.ActualAmount, a.QualifyAmount, a.DeductibleAmount, a.FinalAmount, a.Id }).ToList();
                    }
                    totalRecords = ITProjectionList.Count();
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



        public FileContentResult ExportToExcel(string data, string Financial)
        {
            //List<int> ids = null;
            //ids = Utility.StringIdsToListIds(data);
            //otimize 13/07/2023
         //   List<ITForm24QData> dtEmp = new List<ITForm24QData>();
            //otimize 13/07/2023
            //foreach (var dat in ids)
            //{

            //int dat = Convert.ToInt32(data);
            //var dtDedtr = db.ITForm24QData.Include(e => e.ITForm16Quarter).Select(x => new { x.ReportDate, x.ITForm16Quarter.QuarterAckNo, x.ITForm16Quarter.QuarterName.LookupVal }).ToList();
            //var dtDed = db.ITForm24QData.Include(e => e.ITForm16Quarter).Select(x => new { x.ReportDate, x.ITForm16Quarter.QuarterAckNo, x.ITForm16Quarter.QuarterName.LookupVal }).ToList();
            //var dtChal = db.ITForm24QData.Include(e => e.ITForm16Quarter).Select(x => new { x.ReportDate, x.ITForm16Quarter.QuarterAckNo, x.ITForm16Quarter.QuarterName.LookupVal }).ToList();
            //var dtSal = db.ITForm24QData.Include(e => e.ITForm16Quarter).Select(x => new { x.ReportDate, x.ITForm16Quarter.QuarterAckNo, x.ITForm16Quarter.QuarterName.LookupVal }).ToList();

            //var dtEmp1 = db.EmployeePayroll.Include(e => e.ITForm24QData)
            //    .Include(e => e.ITForm24QData.Select(t => t.ITForm16Quarter.QuarterName))
            //    .Include(e => e.ITForm24QData.Select(t => t.FinancialYear))
            //   .Include(e => e.ITForm24QData.Select(r => r.ITForm24QDataDetails))
            //   .Include(e => e.ITForm24QData.Select(r => r.ITForm24QDataDetails.Select(t => t.ITForm24QFileFormatDefinition)))
            //   .Include(e => e.ITForm24QData.Select(r => r.ITForm24QDataDetails.Select(t => t.ITForm24QFileFormatDefinition.Form24QFileType))).AsNoTracking().AsParallel()
            //   .ToList();
            int finaancialid = Convert.ToInt32(Financial);
            //otimize 13/07/2023 start
            var dtEmp = db.ITForm24QData.Select(r => new
            {
                Id = r.Id,
                FinancialYearId = r.FinancialYear.Id,
                ITForm16Quarter = r.ITForm16Quarter,
                QuarterName = r.ITForm16Quarter.QuarterName,
                ITForm24QDataDetails = r.ITForm24QDataDetails.Select(t => new
                {
                    DataValue = t.DataValue,
                    TaxDepositedDate = t.TaxDepositedDate,
                    ChallanNo = t.ChallanNo,
                    ITForm24QFileFormatDefinition = t.ITForm24QFileFormatDefinition,
                    Form24QFileType=t.ITForm24QFileFormatDefinition.Form24QFileType
                }).ToList(),

            }).Where(r => r.QuarterName.LookupVal.ToUpper() == data.ToUpper() && r.FinancialYearId == finaancialid)
            .OrderBy(e => e.Id).ToList();
            //otimize 13/07/2023 end
            //int quarterid = Convert.ToInt32(data);

            //otimize 13/07/2023
            //if (dtEmp1 != null)
            //{
            //    foreach (var item in dtEmp1)
            //    {
            //        // var emp = item.ITForm24QData.Distinct();
            //        var dtemp = item.ITForm24QData.Where(t => t.ITForm16Quarter.QuarterName.LookupVal.ToUpper() == data.ToUpper() && t.FinancialYear.Id == finaancialid && t.ITForm24QDataDetails.Count() > 0).ToList();
            //        foreach (var item1 in dtemp)
            //        {
            //            if (dtemp != null)
            //            {

            //                dtEmp.Add(item1);
            //            }
            //        }
            //    }
            //}
            //otimize 13/07/2023
            //}
            //var dtDedtr = 
            //var newww = dtEmp.SelectMany(q => q.ITForm24QData);
            //if (newww != null)
            //{

            //}
            //otimize 13/07/2023
          //  List<ITForm24QData> monorder = dtEmp.OrderBy(e => e.Id).ToList();
            //otimize 13/07/2023
            var chhh = dtEmp.FirstOrDefault();
            var dtDedtr = chhh.ITForm24QDataDetails.Where(t => t.ITForm24QFileFormatDefinition.Form24QFileType.LookupVal.ToUpper() == "DEDUCTOR").
               Select(x => new { Header = x.ITForm24QFileFormatDefinition.Field, Value = x.DataValue }).Distinct().ToList();

            var dtDed = dtEmp.SelectMany(r => r.ITForm24QDataDetails.Where(t => t.ITForm24QFileFormatDefinition.Form24QFileType.LookupVal.ToUpper() == "DEDUCTEE"))
                .OrderBy(r => r.ITForm24QFileFormatDefinition.SrNo)
                .ThenBy(x=>x.TaxDepositedDate)
                .ThenBy(x => x.ChallanNo)
               .Select(x => new { Header = x.ITForm24QFileFormatDefinition.Field + "             " + "(" + x.ITForm24QFileFormatDefinition.SrNo + ")", Value = x.DataValue }).ToList();

            var dtChal = chhh.ITForm24QDataDetails.Where(t => t.ITForm24QFileFormatDefinition.Form24QFileType.LookupVal.ToUpper() == "CHALLAN")
                .OrderBy(r => r.ITForm24QFileFormatDefinition.SrNo)
                .ThenBy(x => x.TaxDepositedDate)
                .ThenBy(x => x.ChallanNo)
                .Select(x => new { Header = x.ITForm24QFileFormatDefinition.Field + "             " + "(" + x.ITForm24QFileFormatDefinition.SrNo + ")", Value = x.DataValue }).ToList();

            var dtSal = dtEmp.SelectMany(r => r.ITForm24QDataDetails.Where(t => t.ITForm24QFileFormatDefinition.Form24QFileType.LookupVal.ToUpper() == "SALARY"))
                .OrderBy(r => r.ITForm24QFileFormatDefinition.SrNo)
                .Select(x => new { Header = x.ITForm24QFileFormatDefinition.Field + "             " + "(" + x.ITForm24QFileFormatDefinition.SrNo + ")", Value = x.DataValue }).ToList();



            string[] heading = new string[] { "Deductor", "Deductee", "Challan", "Salary" };

            byte[] filecontent = ExcelExportHelper.ExportExcel(dtDedtr, dtDed, dtChal, dtSal, heading, true, null);
            return File(filecontent, ExcelExportHelper.ExcelContentType, "Form24Q.xlsx");
        }

        ////public ActionResult ExportToExcel()
        ////{
        ////    var gv = new GridView();
        ////    var dt = db.ITForm24QData.ToList();
        ////    gv.DataSource = dt;
        ////    gv.DataBind();
        ////    Response.ClearContent();
        ////    Response.Buffer = true;
        ////    Response.AddHeader("content-disposition", "attachment; filename=DemoExcel.xls");
        ////    Response.ContentType = "application/ms-excel";
        ////    Response.Charset = "";
        ////    StringWriter objStringWriter = new StringWriter();
        ////    HtmlTextWriter objHtmlTextWriter = new HtmlTextWriter(objStringWriter);
        ////    gv.RenderControl(objHtmlTextWriter);
        ////    Response.Output.Write(objStringWriter.ToString());
        ////    Response.Flush();
        ////    Response.End();
        ////    return Json(new { success = true, responseText = "File is created." }, JsonRequestBehavior.AllowGet);
        ////}

        //public IList<EmployeeViewModel> GetEmployeeList()
        //{
        //    DataBaseContext db = new DataBaseContext();
        //    var employeeList = (from e in db.Employees
        //                        join d in db.Departments on e.DepartmentId equals d.DepartmentId
        //                        select new EmployeeViewModel
        //                        {
        //                            Name = e.Name,
        //                            Email = e.Email,
        //                            Age = (int)e.Age,
        //                            Address = e.Address,
        //                            Department = d.DepartmentName
        //                        }).ToList();
        //    return employeeList;
        //}  
    }
}