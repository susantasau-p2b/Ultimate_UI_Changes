using P2BUltimate.App_Start;
using P2BUltimate.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Data;
using Microsoft.Ajax.Utilities;
using P2B.PFTRUST;
using P2B.API;
using OfficeOpenXml.FormulaParsing.LexicalAnalysis.TokenSeparatorHandlers;
using Leave;
using System.Threading.Tasks;
using P2b.Global;
using P2BUltimate.Security;
using System.Transactions;
using System.Data.Entity.Infrastructure;
using Payroll;
using System.Windows.Interop;
using System.Net.NetworkInformation;
using System.Windows.Media.Animation;
using DocumentFormat.OpenXml.Drawing.ChartDrawing;
using System.Linq.Dynamic;

namespace P2BUltimate.Controllers.PFTrust
{
    public class PFLoanAuthorizationFormController : Controller
    {
        // GET: PFLoanAuthorizationForm
        public ActionResult Index()
        {
            return View("~/Views/PFTrust/PFLoanAuthorizationForm/Index.cshtml");
        }







        public class P2BGridData
        {

            public string EmployeeCode { get; set; }
            public string EmployeeName { get; set; }

            public string PFNumber { get; set; }
            public string LoanType { get; set; }
            public string RequisitionDate { get; set; }
            public string SanctionDate { get; set; }
            public string Status { get; set; }
            public string CloserDate { get; set; }
            public string LoanAppliedAmount { get; set; }

            public int Id { get; set; }


        }

        public ActionResult ChkProcess(string typeofbtn, string month)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //int WageMonth = month == "0" ? 0 : Convert.ToInt32(month);

                // var salt = db.PFECRR.Where(e => e.Id == WageMonth).SingleOrDefault();

                string mPayMonth = month;

                bool selected = false;
                var query = db.EmployeePFTrust
                    .Include(e => e.Employee)
                    .Include(e => e.Employee.ServiceBookDates)
                     .Include(e => e.PFTEmployeeLedger)
                      .Include(e => e.PFTEmployeeLedger.Select(x => x.PassbookActivity))
                      .Where(e => e.Employee.ServiceBookDates.ServiceLastDate == null)
                     .ToList();
                // .Include(e => e.PFTEmployeeLedger.Where(r => r.PassbookActivity.LookupVal.ToUpper() == "MONTHLY PF POSTING")).ToList();

                foreach (var item in query)
                {
                    var a = item.PFTEmployeeLedger.Where(t => t.MonthYear == mPayMonth && t.PassbookActivity.LookupVal.ToUpper() == "LOAN DEBIT POSTING").ToList();
                    if (a.Count > 0)
                    {
                        selected = true;
                        break;
                    }
                }
                //  var a = query.PFTEmployeeLedger.Where(t => t.MonthYear == mPayMonth).ToList();


                var data = new
                {
                    status = selected,
                };
                return Json(data, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult LoanDebitPosting(string typeofbtn, string month, string Loanreqid)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                //int WageMonth = month == "0" ? 0 : Convert.ToInt32(month);
                List<P2BUltimate.Process.GlobalProcess.ReturnData> ReturnDataList = new List<P2BUltimate.Process.GlobalProcess.ReturnData>();
                //var salt = db.PFECRR.Where(e => e.Id == WageMonth).SingleOrDefault();
                List<string> Msg = new List<string>();
                string mPayMonth = month;
                DateTime MonthYearDate = Convert.ToDateTime("01/" + mPayMonth);
                DateTime MonthYearDateTo = MonthYearDate.AddMonths(1).AddDays(-1).Date;

                var LoanAdvRequestPFT = db.LoanAdvRequestPFT.Include(e=>e.LoanWFDetails).Where(e => e.IsActive == true && e.SanctionedDate >= MonthYearDate && e.SanctionedDate <= MonthYearDateTo).ToList();
                    //Loan requests
                if (LoanAdvRequestPFT.Count() == 0)
                {
                    Msg.Add("Not any Loan Sanction For " + mPayMonth+" month");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                }
                ReturnDataList = P2BUltimate.Process.GlobalProcess.LoanDebitPosting(mPayMonth);
                foreach (var item in ReturnDataList)
                {
                    Msg.Add(item.ErrMsg);
                }
                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

            }
        }

        //P2BGRID
        public ActionResult P2BGrid(P2BGrid_Parameters gp)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                    int pageSize = gp.rows;
                    int totalPages = 0;
                    int totalRecords = 0;
                    var jsonData = (Object)null;

                    IEnumerable<P2BGridData> LoanAuthorizationList = null;
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
                    DateTime MonthYearDate = Convert.ToDateTime("01/" + PayMonth);
                    DateTime MonthYearDateTo = MonthYearDate.AddMonths(1).AddDays(-1).Date;

                    var BindEmployeeList = db.EmployeePFTrust.Where(e => e.LoanAdvRequestPFT.Count > 0)
                                                             .Include(e => e.Employee)
                                                             .Include(e => e.Employee.EmpName)
                                                             .Include(e => e.LoanAdvRequestPFT)
                                                             .Include(e => e.LoanAdvRequestPFT.Select(m=>m.LoanWFDetails))
                                                             .Include(e => e.LoanAdvRequestPFT.Select(f => f.LoanAdvanceHeadPFT))
                                                             .Include(e => e.LoanAdvRequestPFT.Select(f => f.LoanAdvanceHeadPFT.LoanAdvancePolicyPFT))
                        // .Include(e => e.LoanAdvRequestPFT.Select(f => f.LoanAdvanceHeadPFT.LoanAdvancePolicyPFT.Select(g => g.PFLoanType)))
                                                             .Include(e => e.LoanAdvRequestPFT.Select(f => f.LoanAdvanceHeadPFT.LoanAdvancePolicyPFT.PFLoanType))
                                                             .ToList();




                    foreach (var EmpRecord in BindEmployeeList)
                    {

                        if (EmpRecord != null)
                        {
                            //var loanAdvReq = EmpRecord.LoanAdvRequestPFT.FirstOrDefault();

                            //var loanhead = loanAdvReq.LoanAdvanceHeadPFT;
                            //var loanadvpolicy=loanhead.LoanAdvancePolicyPFT;
                            //var  pfloantype= loanadvpolicy.Select(e => e.PFLoanType);
                            //var pfId = pfloantype.Select(e=>e.Id);
                            //var lookupval = Convert.ToInt32(pfId);
                            int PFlookid = 0;
                            foreach (var itemLARPFT in EmpRecord.LoanAdvRequestPFT)
                            {
                                var itemLARPFT_dta = itemLARPFT.LoanAdvanceHeadPFT.LoanAdvancePolicyPFT.PFLoanType.Id;
                                PFlookid = Convert.ToInt32(itemLARPFT_dta);
                            }

                            var LoanTypevariable = db.LookupValue.Find(PFlookid);


                            foreach (var itemLARPFTvar in EmpRecord.LoanAdvRequestPFT.Where(e=> e.SanctionedDate >= MonthYearDate && e.SanctionedDate <= MonthYearDateTo).OrderByDescending(x=>x.Id))
                            {
                                var WFstatus = itemLARPFTvar.LoanWFDetails.ToList().OrderByDescending(e => e.Id).Select(r => r.WFStatus).FirstOrDefault();
                                var querylookupvalues = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1005").SingleOrDefault().LookupValues.ToList();
                                var chkwfstatus = querylookupvalues.Where(e => e.LookupVal == WFstatus.ToString()).FirstOrDefault();

                                view = new P2BGridData()
                                {

                                    EmployeeCode = EmpRecord.Employee.EmpCode.ToString(),
                                    EmployeeName = EmpRecord.Employee.EmpName.FullDetails.ToString(),
                                    PFNumber = EmpRecord.TrustPFNo==null?"0":EmpRecord.TrustPFNo.ToString(),
                                    LoanType = LoanTypevariable.LookupVal.ToString(),
                                    RequisitionDate = itemLARPFTvar.RequisitionDate.Value.ToShortDateString(),
                                    SanctionDate = itemLARPFTvar.SanctionedDate.Value.ToShortDateString(),
                                    Status = chkwfstatus.LookupValData==null?"":chkwfstatus.LookupValData.ToUpper(),
                                    CloserDate =itemLARPFTvar.CloserDate==null?"": itemLARPFTvar.CloserDate.Value.ToShortDateString(),
                                    LoanAppliedAmount = itemLARPFTvar.LoanAppliedAmount.ToString(),
                                    Id = itemLARPFTvar.Id
                                };
                                model.Add(view);


                            }

                        }
                    }



                    LoanAuthorizationList = model;

                    IEnumerable<P2BGridData> IL;
                    if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                    {
                        IL = LoanAuthorizationList;
                        if (gp.searchOper.Equals("eq"))
                        {


                            jsonData = IL.Where(e => (e.EmployeeCode.ToString().Contains(gp.searchString))


                              || (e.EmployeeName.ToString().Contains(gp.searchString))
                              || (e.PFNumber.ToString().Contains(gp.searchString))
                              || (e.LoanType.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                              || (e.RequisitionDate.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                               || (e.SanctionDate.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                                || (e.Status.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                                 || (e.CloserDate.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                              || (e.LoanAppliedAmount.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                              || (e.Id.ToString().ToUpper().Contains(gp.searchString.ToUpper()))




                              //jsonData = IL.Where(e => (e.Id.ToString().Contains(gp.searchString))

                              //    || (e.EmployeeCode.ToUpper().Contains(gp.searchString.ToUpper()))
                                //    || (e.EmployeeName.ToString().Contains(gp.searchString))
                                //    || (e.PFNumber.ToString().Contains(gp.searchString))
                                //    || (e.LoanType.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                                //    || (e.RequisitionDate.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                                //    || (e.LoanAppliedAmount.ToString().ToUpper().Contains(gp.searchString.ToUpper()))

                              ).Select(a => new Object[] { a.EmployeeCode, a.EmployeeName, a.PFNumber.ToString(), a.LoanType.ToString(), a.RequisitionDate.ToString(), a.SanctionDate.ToString(),a.Status.ToString(),a.CloserDate.ToString(), a.LoanAppliedAmount.ToString(), a.Id }).ToList();
                            //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IL.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.EmployeeCode), Convert.ToString(a.EmployeeName), Convert.ToString(a.PFNumber), Convert.ToString(a.LoanType), Convert.ToString(a.RequisitionDate), Convert.ToString(a.SanctionDate), Convert.ToString(a.Status), Convert.ToString(a.CloserDate), Convert.ToString(a.LoanAppliedAmount), a.Id }).ToList();
                        }
                        totalRecords = IL.Count();
                    }
                    else
                    {
                        IL = LoanAuthorizationList;
                        Func<P2BGridData, dynamic> orderfuc;
                        if (gp.sidx == "Id")
                        {
                            orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                        }
                        else
                        {
                            orderfuc = (c => gp.sidx == "EmployeeCode" ? c.EmployeeCode.ToString() : "");
                        }
                        if (gp.sord == "asc")
                        {
                            IL = IL.OrderBy(orderfuc);
                            jsonData = IL.Select(a => new Object[] { Convert.ToString(a.EmployeeCode), Convert.ToString(a.EmployeeName), Convert.ToString(a.PFNumber), Convert.ToString(a.LoanType), Convert.ToString(a.RequisitionDate), Convert.ToString(a.SanctionDate), Convert.ToString(a.Status), Convert.ToString(a.CloserDate),Convert.ToString(a.LoanAppliedAmount), a.Id }).ToList();
                        }
                        else if (gp.sord == "desc")
                        {
                            IL = IL = IL.OrderByDescending(orderfuc);
                            jsonData = IL.Select(a => new Object[] { Convert.ToString(a.EmployeeCode), Convert.ToString(a.EmployeeName), Convert.ToString(a.PFNumber), Convert.ToString(a.LoanType), Convert.ToString(a.RequisitionDate), Convert.ToString(a.SanctionDate),Convert.ToString(a.Status), Convert.ToString(a.CloserDate), Convert.ToString(a.LoanAppliedAmount), a.Id }).ToList();
                        }
                        if (pageIndex > 1)
                        {
                            int h = pageIndex * pageSize;
                            jsonData = IL.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { Convert.ToString(a.EmployeeCode), Convert.ToString(a.EmployeeName), Convert.ToString(a.PFNumber), Convert.ToString(a.LoanType), Convert.ToString(a.SanctionDate), Convert.ToString(a.RequisitionDate), Convert.ToString(a.Status), Convert.ToString(a.CloserDate),Convert.ToString(a.LoanAppliedAmount), a.Id }).ToList();
                        }
                        totalRecords = LoanAuthorizationList.Count();
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
        }
        //=============================================P2BGrid========================================================


        //==============================================EDIT=============================================================


        public class returnEditClass
        {
            //public string LoanAdvanceHead_Id { get; set; }

            //public string LoanAdvanceHead_FullDetails { get; set; }

            public int wfStatusids { get; set; }



        }
        [HttpPost]
        public ActionResult SanctionCheck(int reqid)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                var loanAdvVariable = db.LoanAdvRequestPFT.Include(e => e.LoanWFDetails).Where(e => e.Id == reqid).SingleOrDefault();
                var WFstatus = loanAdvVariable.LoanWFDetails.ToList().OrderByDescending(e => e.Id).Select(r => r.WFStatus).FirstOrDefault();

                var loanWFId = loanAdvVariable.LoanWFDetails.ToList().OrderByDescending(e => e.Id).Select(r => r.Id).FirstOrDefault();
                var lookupValueId = db.Lookup.Include(i => i.LookupValues).Where(i => i.Code == "1005").Select(e => e.LookupValues.Where(i => i.LookupVal == WFstatus.ToString()).Select(i => i.Id)).FirstOrDefault();
                var querylookupvalues = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1005").SingleOrDefault().LookupValues.ToList();
                int wfstatusid = querylookupvalues.Where(e => e.LookupVal == WFstatus.ToString()).FirstOrDefault().Id;
                var chkwfstatus = querylookupvalues.Where(e => e.LookupVal == WFstatus.ToString()).FirstOrDefault();
                if (chkwfstatus != null)
                {
                    if (chkwfstatus.LookupValData.ToUpper() != "APPLIED")
                    {
                        Msg.Add("You can not authorise because Loan has " + chkwfstatus.LookupValData.ToUpper());
                        
                    }
                }
                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult Edit(int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {



                var Q = db.LoanAdvRequestPFT
                    .Include(e => e.LoanAdvanceHeadPFT)
                                            .Where(e => e.Id == data).Select(e => new
                                            {
                                                BankBranchDetails = e.LoanAccBranch.FullDetails.ToString() == null ? "" : e.LoanAccBranch.FullDetails.ToString(),
                                                LoanAdvanceHeadPFT = e.LoanAdvanceHeadPFT.Name == null ? "" : e.LoanAdvanceHeadPFT.Name,
                                                LoanAccNo = e.LoanAccNo == null ? "" : e.LoanAccNo,
                                                RequisitionDate = e.RequisitionDate == null ? null : e.RequisitionDate,
                                                SanctionedDate = e.SanctionedDate == null ? null : e.SanctionedDate,
                                                LoanAppliedAmount = e.LoanAppliedAmount == 0 ? 0 : e.LoanAppliedAmount,
                                                LoanSanctionedAmount = e.LoanSanctionedAmount == 0 ? 0 : e.LoanSanctionedAmount,
                                                MonthlyInstallmentAmount = e.MonthlyInstallmentAmount == 0 ? 0 : e.MonthlyInstallmentAmount,
                                                TotalInstall = e.TotalInstall == 0 ? 0 : e.TotalInstall,
                                                MonthlyPricipalAmount = e.MonthlyPricipalAmount == 0 ? 0 : e.MonthlyPricipalAmount,
                                                MonthlyInterest = e.MonthlyInterest == 0 ? 0 : e.MonthlyInterest,
                                                Narration = e.Narration.ToString() == null ? "" : e.Narration.ToString(),
                                                EnforcementDate = e.EnforcementDate == null ? null : e.EnforcementDate,
                                                IsActive = e.IsActive,
                                                OwnPFAmount = e.OwnPFAmount == 0 ? 0 : e.OwnPFAmount,
                                                OwnPFIntAmount = e.OwnPFIntAmount == 0 ? 0 : e.OwnPFIntAmount,
                                                OwnerPFAmount = e.OwnerPFAmount == 0 ? 0 : e.OwnerPFAmount,
                                                OwnerPFIntAmount = e.OwnerPFIntAmount == 0 ? 0 : e.OwnerPFIntAmount,
                                                VPFAmount = e.VPFAmount == 0 ? 0 : e.VPFAmount,
                                                VPFIntAmount = e.VPFIntAmount == 0 ? 0 : e.VPFIntAmount,
                                                Bank = e.BankBranchDetails.ToString() == null ? "" : e.BankBranchDetails.ToString(),
                                                PaymentMode = e.PaymentMode.Id.ToString() == null ? "" : e.PaymentMode.Id.ToString(),
                                                PaymentModeVal = e.PaymentMode.LookupVal == null ? "" : e.PaymentMode.LookupVal,
                                                ChequeNo = e.ChequeNo.ToString() == null ? "" : e.ChequeNo.ToString(),
                                                ChequeDate = e.ChequeDate == null ? DateTime.Now : e.ChequeDate

                                            })
                                            .ToList();

                var loanAdvVariable = db.LoanAdvRequestPFT.Include(e => e.LoanWFDetails).Where(e => e.Id == data).SingleOrDefault();
                var WFstatus = loanAdvVariable.LoanWFDetails.ToList().OrderByDescending(e => e.Id).Select(r => r.WFStatus).FirstOrDefault();

                var loanWFId = loanAdvVariable.LoanWFDetails.ToList().OrderByDescending(e => e.Id).Select(r => r.Id).FirstOrDefault();
                var lookupValueId = db.Lookup.Include(i => i.LookupValues).Where(i => i.Code == "1005").Select(e => e.LookupValues.Where(i => i.LookupVal == WFstatus.ToString()).Select(i => i.Id)).FirstOrDefault();
                var querylookupvalues = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1005").SingleOrDefault().LookupValues.ToList();
                int wfstatusid = querylookupvalues.Where(e => e.LookupVal == WFstatus.ToString()).FirstOrDefault().Id;
              
                List<returnEditClass> oreturnEditClass = new List<returnEditClass>();
                oreturnEditClass.Add(new returnEditClass
                {

                    wfStatusids = wfstatusid

                });

                return Json(new Object[] { Q, oreturnEditClass, JsonRequestBehavior.AllowGet });


            }


        }

        //================================================================================================================================================





        public class rreturnEditClassT
        {
            public int wfStatusviewid { get; set; }
            public Array Comment { get; set; }
        }

        [HttpPost]
        public ActionResult EditView(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                var Q = db.LoanAdvRequestPFT
                    .Include(e => e.LoanAdvanceHeadPFT)
                    //.Include(e=>e.LoanWFDetails)
                                            .Where(e => e.Id == data).Select(e => new
                                            {
                                                BankBranchDetails = e.LoanAccBranch.FullDetails.ToString() == null ? "" : e.LoanAccBranch.FullDetails.ToString(),
                                                LoanAdvanceHeadPFT = e.LoanAdvanceHeadPFT.Name == null ? "" : e.LoanAdvanceHeadPFT.Name,
                                                LoanAccNo = e.LoanAccNo == null ? "" : e.LoanAccNo,
                                                RequisitionDate = e.RequisitionDate == null ? null : e.RequisitionDate,
                                                SanctionedDate = e.SanctionedDate == null ? null : e.SanctionedDate,
                                                LoanAppliedAmount = e.LoanAppliedAmount == 0 ? 0 : e.LoanAppliedAmount,
                                                LoanSanctionedAmount = e.LoanSanctionedAmount == 0 ? 0 : e.LoanSanctionedAmount,
                                                MonthlyInstallmentAmount = e.MonthlyInstallmentAmount == 0 ? 0 : e.MonthlyInstallmentAmount,
                                                TotalInstall = e.TotalInstall.ToString() == null ? "" : e.TotalInstall.ToString(),
                                                MonthlyPricipalAmount = e.MonthlyPricipalAmount.ToString() == null ? "" : e.MonthlyPricipalAmount.ToString(),
                                                MonthlyInterest = e.MonthlyInterest.ToString() == null ? "" : e.MonthlyInterest.ToString(),
                                                Narration = e.Narration.ToString() == null ? "" : e.Narration.ToString(),
                                                EnforcementDate = e.EnforcementDate == null ? null : e.EnforcementDate,
                                                IsActive = e.IsActive,
                                                OwnPFAmount = e.OwnPFAmount == 0 ? 0 : e.OwnPFAmount,
                                                OwnPFIntAmount = e.OwnPFIntAmount == 0 ? 0 : e.OwnPFIntAmount,
                                                OwnerPFAmount = e.OwnerPFAmount == 0 ? 0 : e.OwnerPFAmount,
                                                OwnerPFIntAmount = e.OwnerPFIntAmount == 0 ? 0 : e.OwnerPFIntAmount,
                                                VPFAmount = e.VPFAmount == 0 ? 0 : e.VPFAmount,
                                                VPFIntAmount = e.VPFIntAmount == 0 ? 0 : e.VPFIntAmount,
                                                Bank = e.BankBranchDetails.ToString() == null ? "" : e.BankBranchDetails.ToString(),
                                                PaymentMode = e.PaymentMode.Id.ToString() == null ? "" : e.PaymentMode.Id.ToString(),
                                                PaymentModeVal = e.PaymentMode.LookupVal == null ? "" : e.PaymentMode.LookupVal,
                                                ChequeNo = e.ChequeNo.ToString() == null ? "" : e.ChequeNo.ToString(),
                                                ChequeDate = e.ChequeDate == null ? DateTime.Now : e.ChequeDate

                                            })
                                            .ToList();


                List<rreturnEditClassT> oreturnEditClasss = new List<rreturnEditClassT>();

                var loanAdvVariable = db.LoanAdvRequestPFT.Include(e => e.LoanWFDetails).Where(e => e.Id == data).SingleOrDefault();

                var WFstatus = loanAdvVariable.LoanWFDetails.ToList().OrderByDescending(e => e.Id).Select(r => r.WFStatus).FirstOrDefault();
                var loanWFId = loanAdvVariable.LoanWFDetails.ToList().OrderByDescending(e => e.Id).Select(r => r.Id).FirstOrDefault();

                var lookupValueId = db.Lookup.Include(i => i.LookupValues).Where(i => i.Code == "1005").Select(e => e.LookupValues.Where(i => i.LookupVal == WFstatus.ToString()).Select(i => i.Id)).FirstOrDefault();

                var querylookupvalues = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1005").SingleOrDefault().LookupValues.ToList();

                int wfstatusidviews = querylookupvalues.Where(e => e.LookupVal == WFstatus.ToString()).FirstOrDefault().Id;

                oreturnEditClasss.Add(new rreturnEditClassT
                {

                    wfStatusviewid = wfstatusidviews,

                    //wfStatusDetail = querylookupvalues.Where(e => e.LookupVal == WFstatus.ToString()).Select(e => e.LookupValData).ToArray(),

                    Comment = loanAdvVariable.LoanWFDetails.Select(e => e.Comments).ToArray()
                });
                return Json(new Object[] { Q, oreturnEditClasss, JsonRequestBehavior.AllowGet });


            }

        }



        //================================================================================================================================================




        public ActionResult GetLookupValueDATA(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                SelectList svaldata = (SelectList)null;
                var selected = "";
                List<string> AppStatus = new List<string> { "0", "1", "2" };
                if (data != "" && data != null)
                {
                    var qurey1 = db.Lookup.Where(e => e.Code == data).Select(e => e.LookupValues.Where(r => r.IsActive == true && AppStatus.Contains(r.LookupVal))).SingleOrDefault(); // added

                    if (data2 != "" && data2 != "0")
                    {
                        selected = data2;
                    }
                    if (qurey1 != null)
                    {
                        svaldata = new SelectList(qurey1, "Id", "LookupValData", selected);
                    }

                }
                return Json(svaldata, JsonRequestBehavior.AllowGet);

            }
        }

        //===========================================================================================================



        //=============================================== Edit submit==========================================================

        //[HttpPost]
        //public async Task<ActionResult> EditSave(LoanAdvRequestPFT la, int data, FormCollection form)
        //{

        //}     

        [HttpPost]
        public async Task<ActionResult> EditSave(LoanAdvRequestPFT larp, int data, FormCollection form) // Edit submit
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                double ObjLoanSanctionedAmount = larp.LoanSanctionedAmount;
                double ObjLoanAppliedAmount = larp.LoanAppliedAmount;

                double FindTotalAmount = (larp.OwnPFAmount + larp.OwnerPFAmount)
                                           + (larp.VPFAmount + larp.OwnPFIntAmount)
                                           + (larp.OwnerPFIntAmount + larp.VPFIntAmount);

                if (FindTotalAmount>ObjLoanSanctionedAmount )
                {
                    Msg.Add("PF contribution amount is not greater than LoanSanctionedAmount. ");
                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }

                    //if (ObjLoanAppliedAmount == FindTotalAmount)
                    //{
                    //    Msg.Add(" LoanAppliedAmount Should not be equal PF Loan Balance (OwnPFAmount+OwnerPFAmount+VPFAmount+OwnPFIntAmount+OwnerPFIntAmount+VPFIntAmount");
                    //    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    //}

                var loanObject = db.LoanAdvRequestPFT.Include(e => e.LoanWFDetails).Where(e => e.Id == data).SingleOrDefault();

                var WfId = loanObject.LoanWFDetails.ToList().OrderByDescending(e => e.Id).Select(r => r.WFStatus).FirstOrDefault();

                if (WfId == 1)
                {
                    Msg.Add("Record is already sanctioned");
                    // return Json(new Object[] { new Utility.JsonReturnClass { success = false, responseText=Msg }, JsonRequestBehavior.DenyGet });
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                }

                if (WfId == 2)
                {
                    Msg.Add("Record is already rejected");
                    //return Json(new Object[] { new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.DenyGet });

                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }






                try
                {
                    string LoanWFDetails = form["LoanWFDetailids"] == "0" ? "" : form["LoanWFDetailids"];
                    string Comment = form["comment"] == "0" ? "" : form["comment"];

                    larp.LoanWFDetails = null;
                    List<LoanWFDetails> loanWFDetails = new List<LoanWFDetails>();
                    //String fromValues = form["LoanWFDetails"];



                    if (LoanWFDetails != null)
                    {
                        var ids = Utility.StringIdsToListIds(LoanWFDetails);
                        foreach (var item in ids)
                        {
                            var a = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1005").FirstOrDefault().LookupValues.Where(e => e.Id == item).FirstOrDefault().LookupValData;  //db.LookupValue.Find(int.Parse(BusinessCategory));

                            if (a != null)

                            {
                                if (a == "Sanctioned" || a == "Sanction")
                                {
                                    LoanWFDetails loanWF = new LoanWFDetails
                                    {
                                        WFStatus = 1,
                                        Comments = Comment,
                                        DBTrack = new DBTrack { Action = "M", ModifiedBy = SessionManager.UserName, IsModified = true }
                                    };

                                    loanWFDetails.Add(loanWF);
                                }

                                if (a == "Rejected By Sanction")
                                {

                                    LoanWFDetails loanWF = new LoanWFDetails
                                    {
                                        WFStatus = 2,
                                        Comments = Comment,
                                        DBTrack = new DBTrack { Action = "M", ModifiedBy = SessionManager.UserName, IsModified = true }
                                    };

                                    loanWFDetails.Add(loanWF);

                                }
                                loanWFDetails.AddRange(loanObject.LoanWFDetails);
                                larp.LoanWFDetails = loanWFDetails;
                            }

                            //var LoanWF_Object = db.LoanWFDetails.Where(e => e.Id == val.).ToList();

                            //loanWFDetails.Add(val.sel);
                            //larp.LoanWFDetails = loanWFDetails;
                        }
                    }




                    try
                    {


                        // larp.DBTrack = new DBTrack { Action = "M", ModifiedBy = SessionManager.UserName, IsModified = true };

                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {


                            var db_data = db.LoanAdvRequestPFT.Include(e => e.LoanAdvanceHeadPFT)
                                                               .Include(e => e.LoanAdvRepaymentTPFT)
                                                               .Include(e => e.LoanWFDetails)
                                                               .Where(e => e.Id == data).SingleOrDefault();
                            TempData["RowVersion"] = db_data.RowVersion;
                            TempData["CurrRowVersion"] = db_data.RowVersion;
                            if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                            {
                                larp.DBTrack = new DBTrack
                                {
                                    CreatedBy = db_data.DBTrack.CreatedBy == null ? null : db_data.DBTrack.CreatedBy,
                                    CreatedOn = db_data.DBTrack.CreatedOn == null ? null : db_data.DBTrack.CreatedOn,
                                    Action = "M",
                                    ModifiedBy = SessionManager.UserName,
                                    ModifiedOn = DateTime.Now,
                                    IsModified = true
                                };
                                db_data.OwnPFAmount = larp.OwnPFAmount;
                                db_data.OwnerPFAmount = larp.OwnerPFAmount;
                                db_data.VPFAmount = larp.VPFAmount;
                                db_data.OwnPFIntAmount = larp.OwnPFIntAmount;
                                db_data.OwnerPFIntAmount = larp.OwnerPFIntAmount;
                                db_data.VPFIntAmount = larp.VPFIntAmount;

                                db_data.Id = data;
                                db_data.LoanWFDetails = larp.LoanWFDetails;
                                db_data.DBTrack = larp.DBTrack;





                                try
                                {
                                    var loanAdvRequestObject = db.LoanAdvRequestPFT.Where(e => e.Id == data)
                                                                                    .Include(e => e.LoanAdvanceHeadPFT)
                                                                                    .Include(e => e.LoanAdvanceHeadPFT.LoanAdvancePolicyPFT)
                                                                                   // .Include(e => e.LoanAdvanceHeadPFT.LoanAdvancePolicyPFT.Select(f => f.PFLoanType))
                                                                                     .Include(e => e.LoanAdvanceHeadPFT.LoanAdvancePolicyPFT.PFLoanType)
                                                                                    .ToList();

                                    int PFlookid = 0;
                                    foreach (var itemLARPFT in loanAdvRequestObject)
                                    {
                                        var itemLARPFT_dta = itemLARPFT.LoanAdvanceHeadPFT.LoanAdvancePolicyPFT.PFLoanType.Id;
                                        PFlookid = Convert.ToInt32(itemLARPFT_dta);
                                    }

                                    var loanTypeVariable = db.LookupValue.Find(PFlookid);

                                    string loantype = loanTypeVariable.LookupVal.ToString();

                                    List<LoanAdvRepaymentTPFT> LoanAdvRepaymentTPFTDetails = new List<LoanAdvRepaymentTPFT>();





                                    var wfSVariable = db_data.LoanWFDetails.FirstOrDefault().WFStatus;


                                    if (wfSVariable == 1)
                                    {

                                        if (loantype == "Refundable")
                                        {

                                            List<LoanAdvRepaymentTPFT> LoanAdvRepaymentTPFTdet = new List<LoanAdvRepaymentTPFT>();

                                            if (db_data.EnforcementDate != null && db_data.TotalInstall != 0)
                                            {

                                                for (int i = 0; i <= db_data.TotalInstall - 1; i++)
                                                {

                                                    double TotalLoanPaid = 0;
                                                    string Month = db_data.EnforcementDate.Value.AddMonths(i).Month.ToString().Length == 1 ? "0" + db_data.EnforcementDate.Value.AddMonths(i).Month.ToString() : db_data.EnforcementDate.Value.AddMonths(i).Month.ToString();
                                                    string PayMonth = Month + "/" + db_data.EnforcementDate.Value.AddMonths(i).Year;
                                                    LoanAdvRepaymentTPFT LoanAdvRepayT = new LoanAdvRepaymentTPFT()
                                                    {
                                                        InstallementDate = db_data.EnforcementDate.Value.AddMonths(i),
                                                        InstallmentAmount = db_data.MonthlyInstallmentAmount,
                                                        InstallmentCount = i + 1,
                                                        InstallmentPaid = 0,
                                                        PayMonth = PayMonth,
                                                        RepaymentDate = null,
                                                        TotalLoanBalance = 0,//LoanAdvRequest.LoanSanctionedAmount - installment_Paid,
                                                        TotalLoanPaid = 0,//installment_Paid
                                                        DBTrack = db_data.DBTrack
                                                    };

                                                    TotalLoanPaid = TotalLoanPaid + LoanAdvRepayT.InstallmentPaid;
                                                    LoanAdvRepaymentTPFTdet.Add(LoanAdvRepayT);
                                                }

                                            }
                                            db_data.LoanAdvRepaymentTPFT = LoanAdvRepaymentTPFTdet;
                                            db.LoanAdvRequestPFT.Attach(db_data);
                                            db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();
                                          //  db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;
                                            ts.Complete();
                                            Msg.Add("Record updated.");
                                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);


                                        }
                                        else
                                        {
                                            db.LoanAdvRequestPFT.Attach(db_data);
                                            db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();
                                           // db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;
                                            ts.Complete();
                                            Msg.Add("Record updated.");
                                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                        }
                                    }

                                    else
                                    {
                                        db.LoanAdvRequestPFT.Attach(db_data);
                                        db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();
                                        db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;
                                        ts.Complete();
                                        Msg.Add("Record updated.");
                                        return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                    }

                                }


                                catch (DbUpdateConcurrencyException)
                                {
                                    return RedirectToAction("Create", new { concurrencyError = true, id = db_data.Id });
                                }
                                catch (DataException /* dex */)
                                {
                                    return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
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
                                    //    List<string> Msg = new List<string>();
                                    Msg.Add(ex.Message);
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }







                            }
                        }
                    }


                    catch (DbUpdateConcurrencyException ex)
                    {
                        var entry = ex.Entries.Single();
                        var clientValues = (InterestRate)entry.Entity;
                        var databaseEntry = entry.GetDatabaseValues();
                        if (databaseEntry == null)
                        {
                            Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {

                            // var databaseValues = ()databaseEntry.ToObject();
                            //c.RowVersion = databaseValues.RowVersion;

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
                    Msg.Add("Record modified by another user.So refresh it and try to save again.");
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

            }
        }


        //=============================================== Edit submit==========================================================

    }
}