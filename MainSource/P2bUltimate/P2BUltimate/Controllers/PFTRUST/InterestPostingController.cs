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

namespace P2BUltimate.Controllers.PFTrust
{
    public class InterestPostingController : Controller
    {
        // GET: InterestPosting
        public ActionResult Index()
        {
            return View("~/Views/PFTrust/InterestPosting/index.cshtml");
        }
        public ActionResult getCalendar1()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var qurey = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "PFCALENDAR".ToUpper() && e.Default == true).AsEnumerable()
                    .Select(e => new
                    {
                        Id = e.Id,
                        Lvcalendardesc = "FromDate :" + e.FromDate.Value.ToShortDateString() + " ToDate :" + e.ToDate.Value.ToShortDateString(),
                        FromDate = e.FromDate.Value.ToShortDateString(),
                        ToDate = e.ToDate.Value.ToShortDateString()

                    }).SingleOrDefault();
                return Json(qurey, JsonRequestBehavior.AllowGet);
            }
        }
        public class P2BGridData
        {
            public int Id { get; set; }
            public string EmpCode { get; set; }
            public string EmpName { get; set; }
            public DateTime PostingDate { get; set; }
            public string MonthYear { get; set; }
            public double TotalOwnPF { get; set; }
            public double TotalOwnerPF { get; set; }
            public double TotalVPF { get; set; }
            public double TotalPF { get; set; }
            public double Owninterest { get; set; }
            public double Ownerinterest { get; set; }
            public double Vpfinterest { get; set; }
            public double Totalinterest { get; set; }
            public double InterestOninterest { get; set; }
            public string InterestFreq { get; set; }


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
                string PostingDate = "";
                if (gp.filter != null)
                {
                    PostingDate = gp.filter;
                }
                else
                {
                    PostingDate = DateTime.Now.Date.ToShortDateString();
                }
                //if (gp.filter != null)
                //    PayMonth = gp.filter;
                //else
                //{
                //    if (DateTime.Now.Date.Month < 10)
                //        Month = "0" + DateTime.Now.Date.Month;
                //    else
                //        Month = DateTime.Now.Date.Month.ToString();
                //    PayMonth = Month + "/" + DateTime.Now.Date.Year;
                //}

                int company_Id = 0;
                company_Id = Convert.ToInt32(Session["CompId"]);

                //var Empdata = db.EmployeePFTrust.Include(e => e.Employee)
                //    .Include(e => e.Employee.EmpName)
                //    .Include(e => e.Employee.EmpOffInfo)
                //    .Include(e => e.Employee.EmpOffInfo.NationalityID)
                //    .Include(e => e.PFTEmployeeLedger)
                //    .Include(e => e.PFTEmployeeLedger.Select(x => x.PassbookActivity))
                //    .Include(e => e.PFTEmployeeLedger.Select(y=>y.InterestFrequency))
                //    .ToList();

                var Empdata1 = db.PFTEmployeeLedger.Select(z => new
                {
                    PassbookActivity = z.PassbookActivity.LookupVal,
                    Empcode=z.EmployeePFTrust.Employee.EmpCode,
                    EmpName = z.EmployeePFTrust.Employee.EmpName.FullNameFML,
                    Id = z.EmployeePFTrust.Id,
                    PostingDate = z.PostingDate,
                    MonthYear = z.MonthYear,
                    InterestFrequency = z.InterestFrequency.LookupVal,
                    OwnCloseBal = z.OwnCloseBal,
                    OwnerCloseBal = z.OwnerCloseBal,
                    VPFCloseBal = z.VPFCloseBal,
                    PFCloseBal = z.PFCloseBal,
                    OwnIntCloseBal = z.OwnIntCloseBal,
                    OwnerIntCloseBal = z.OwnerIntCloseBal,
                    VPFIntCloseBal = z.VPFIntCloseBal,
                    TotalIntCloseBal = z.TotalIntCloseBal,
                    IntonIntCloseBal = z.IntonIntCloseBal
                }
            ).Where(e =>e.PassbookActivity.ToUpper() == "INTEREST BALANCE").OrderBy(e => e.Id).ToList();
                //var BindCompList = db.PFTEmployeeLedger.ToList();
                var Empdata = Empdata1.Where(e => e.PostingDate.ToShortDateString() == PostingDate).ToList();
                foreach (var a in Empdata)
                {
                   // var pftemployeledgerdata = z.PFTEmployeeLedger.Where(e => e.PostingDate.ToShortDateString() == PostingDate && e.PassbookActivity.LookupVal.ToUpper() == "INTEREST BALANCE").OrderBy(e => e.Id).ToList();

                    //foreach (var a in pftemployeledgerdata)
                    //{

                        //var aa = db.PFTEmployeeLedger.Where(e => e.Id == a.Id).SingleOrDefault();
                        //if (a != null)
                        //{

                        view = new P2BGridData()
                        {
                            Id = a.Id,
                            EmpCode = a.Empcode,
                            EmpName = a.EmpName,
                            PostingDate = a.PostingDate,
                            MonthYear = a.MonthYear,
                            InterestFreq= a.InterestFrequency,
                            TotalOwnPF = a.OwnCloseBal,
                            TotalOwnerPF = a.OwnerCloseBal,
                            TotalVPF = a.VPFCloseBal,
                            TotalPF = a.PFCloseBal,
                            Owninterest=a.OwnIntCloseBal,
                            Ownerinterest=a.OwnerIntCloseBal,
                            Vpfinterest=a.VPFIntCloseBal,
                            Totalinterest=a.TotalIntCloseBal,
                            InterestOninterest=a.IntonIntCloseBal

                        };

                        model.Add(view);
                        // }
                  //  }

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
                             //  || (e.Employee.EmpOffInfo.NationalityID.PFNo.ToString().Contains(gp.searchString))
                              || (e.PostingDate.ToString().Contains(gp.searchString))
                               || (e.MonthYear.ToString().Contains(gp.searchString))
                              || (e.InterestFreq.ToString().Contains(gp.searchString))
                              || (e.TotalOwnPF.ToString().Contains(gp.searchString))
                              || (e.TotalOwnerPF.ToString().Contains(gp.searchString))
                              || (e.TotalVPF.ToString().Contains(gp.searchString))
                              || (e.TotalPF.ToString().Contains(gp.searchString))
                              || (e.Owninterest.ToString().Contains(gp.searchString))
                              || (e.Ownerinterest.ToString().Contains(gp.searchString))
                              || (e.Vpfinterest.ToString().Contains(gp.searchString))
                              || (e.Totalinterest.ToString().Contains(gp.searchString))
                              || (e.InterestOninterest.ToString().Contains(gp.searchString))

                                ).Select(a => new Object[] { a.EmpCode, a.EmpName, a.PostingDate.ToShortDateString(), a.InterestFreq, a.TotalOwnPF, a.TotalOwnerPF, a.TotalVPF, a.TotalPF, a.Owninterest, a.Ownerinterest, a.Vpfinterest, a.Totalinterest, a.InterestOninterest, a.Id }).ToList();
                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.EmpCode, a.EmpName, a.PostingDate.ToShortDateString(), a.InterestFreq, a.TotalOwnPF, a.TotalOwnerPF, a.TotalVPF, a.TotalPF, a.Owninterest, a.Ownerinterest, a.Vpfinterest, a.Totalinterest, a.InterestOninterest, a.Id }).ToList();
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
                                         //gp.sidx == "OwnPFMonthly" ? c.OwnerPFMonthly.ToString() :
                                         //gp.sidx == "OwnerPFMonthly" ? c.OwnerPFMonthly.ToString() :
                                         //gp.sidx == "PensionAmount" ? c.PensionAmount.ToString() :
                                         //gp.sidx == "VPFAMOUNT" ? c.VPFAmountMonthly.ToString() :

                                         "");

                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.EmpCode, a.EmpName, a.PostingDate.ToShortDateString(), a.InterestFreq, a.TotalOwnPF, a.TotalOwnerPF, a.TotalVPF, a.TotalPF, a.Owninterest, a.Ownerinterest, a.Vpfinterest, a.Totalinterest, a.InterestOninterest, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.EmpCode, a.EmpName, a.PostingDate.ToShortDateString(), a.InterestFreq, a.TotalOwnPF, a.TotalOwnerPF, a.TotalVPF, a.TotalPF, a.Owninterest, a.Ownerinterest, a.Vpfinterest, a.Totalinterest, a.InterestOninterest, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.EmpCode, a.EmpName, a.PostingDate.ToShortDateString(), a.InterestFreq, a.TotalOwnPF, a.TotalOwnerPF, a.TotalVPF, a.TotalPF, a.Owninterest, a.Ownerinterest, a.Vpfinterest, a.Totalinterest, a.InterestOninterest, a.Id }).ToList();
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


        public ActionResult ChkProcess(string typeofbtn, string month)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                if (month==null && month=="")
                {
                     Msg.Add("  Kindly select Interest Posting date  ");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
              
                DateTime Postingdate = Convert.ToDateTime(month);
                string mPayMonth = Postingdate.ToString("mm/YYYY");

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
                    var a = db.PFTEmployeeLedger.Where(t => t.MonthYear == mPayMonth && t.PassbookActivity.LookupVal.ToUpper() == "INTEREST POSTING").ToList();
                    if (a.Count > 0)
                    {
                        selected = true;
                      //  break;
                    }
                //}
               


                var data = new
                {
                    status = selected,
                };
                return Json(data, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult CreateInterestPost(FormCollection form)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                string Emp = form["Employee-Table"] == "0" ? "" : form["Employee-Table"];
                string month = form["CrPostingDate"] == "0" ? "" : form["CrPostingDate"];
                string CrFromDate = form["CrFromDate"] == "0" ? "" : form["CrFromDate"];
                string CrToDate = form["CrToDate"] == "0" ? "" : form["CrToDate"];   
                DateTime Postingdate = Convert.ToDateTime(month);
                string mPayMonth = Postingdate.ToString("mm/YYYY");
                var Id = Convert.ToInt32(SessionManager.CompanyId);
                var CompCodeid = db.Company.Where(e => e.Id == Id).SingleOrDefault();
                 DBTrack DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                  var qurey = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "PFCALENDAR".ToUpper() && e.Default == true).AsEnumerable()
                    .Select(e => new
                    {
                        Id = e.Id,
                        Lvcalendardesc = "FromDate :" + e.FromDate.Value.ToShortDateString() + " ToDate :" + e.ToDate.Value.ToShortDateString(),
                        FromDate = e.FromDate.Value.ToShortDateString(),
                        ToDate = e.ToDate.Value.ToShortDateString()

                    }).Where(e => e.FromDate == CrFromDate && e.ToDate == CrToDate).SingleOrDefault();
                 //Check Interest posting Month start
                  string MonthYearFull = Postingdate.ToString("MMMM");
                  List<string> IntPostMonth = null;
                  var CompantPFTrust_Id = db.CompanyPFTrust.Where(e => e.Company_Id == CompCodeid.Id).Select(e => e.Id).SingleOrDefault();
                  var IntPolicyId = db.PFTACCalendar.Include(e => e.CompanyPFTrust).Include(e => e.InterestPolicies).Where(e => e.CompanyPFTrust.Id == CompantPFTrust_Id && e.PFTCalendar.Id == qurey.Id).Select(e => e.InterestPolicies.Id).SingleOrDefault();
                  if (IntPolicyId==0)
                  {
                     Msg.Add(" Interest Policy Not Available for "+ qurey.Lvcalendardesc);
                      return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);  
                  }

                  var InterestPolicies = db.InterestPolicies.Include(e => e.InterestRate).Where(e => e.Id == IntPolicyId).SingleOrDefault();

                 var IntRate = InterestPolicies.InterestRate.Where(e => Postingdate >= e.EffectiveFrom && Postingdate <= e.EffectiveTo)
                                                  .SingleOrDefault();
                 if (IntRate==null)
                 {
                       Msg.Add(" Interest Posting date not between rate period of current PF calendar Policy");
                      return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);  
                 }

                  var EffectiveMonths1 = db.InterestPolicies
                                         .Include(e => e.StatutoryEffectiveMonthsPFT)
                                         .Include(e => e.StatutoryEffectiveMonthsPFT.Select(x => x.EffectiveMonth))
                                        .Where(e => e.Id == IntPolicyId).SingleOrDefault();

                  var EffectiveMonths = EffectiveMonths1.StatutoryEffectiveMonthsPFT.Select(r => r.EffectiveMonth).ToList();
                  IntPostMonth = EffectiveMonths.Select(x => x.LookupVal).ToList();
                  if (IntPostMonth.Contains(MonthYearFull) != true)
                  {
                      Msg.Add(" Interest Posting Month is not valid ");
                      return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                  }
                  //Check Interest posting Month end

                  Msg = P2BUltimate.Process.GlobalProcess.InterestPostingEmpWise(CompCodeid.Id, qurey.Id, Postingdate, null, DBTrack, Emp);
                  return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

            }
        }

        //public ActionResult InterestPost(string typeofbtn, string month)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        List<string> Msg = new List<string>();

        //        DateTime Postingdate = Convert.ToDateTime(month);
        //        string mPayMonth = Postingdate.ToString("mm/YYYY");
        //        var Id = Convert.ToInt32(SessionManager.CompanyId);
        //        var CompCodeid = db.Company.Where(e => e.Id == Id).SingleOrDefault();
        //        DBTrack DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
        //        var qurey = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "PFCALENDAR".ToUpper() && e.Default == true).AsEnumerable()
        //          .Select(e => new
        //          {
        //              Id = e.Id,
        //              Lvcalendardesc = "FromDate :" + e.FromDate.Value.ToShortDateString() + " ToDate :" + e.ToDate.Value.ToShortDateString(),
        //              FromDate = e.FromDate.Value.ToShortDateString(),
        //              ToDate = e.ToDate.Value.ToShortDateString()

        //          }).SingleOrDefault();
        //        //Check Interest posting Month start
        //        string MonthYearFull = Postingdate.ToString("MMMM");
        //        List<string> IntPostMonth = null;
        //        var CompantPFTrust_Id = db.CompanyPFTrust.Where(e => e.Company_Id == CompCodeid.Id).Select(e => e.Id).SingleOrDefault();
        //        var IntPolicyId = db.PFTACCalendar.Include(e => e.CompanyPFTrust).Include(e => e.InterestPolicies).Where(e => e.CompanyPFTrust.Id == CompantPFTrust_Id && e.PFTCalendar.Id == qurey.Id).Select(e => e.InterestPolicies.Id).SingleOrDefault();

        //        var EffectiveMonths1 = db.InterestPolicies
        //                               .Include(e => e.StatutoryEffectiveMonthsPFT)
        //                               .Include(e => e.StatutoryEffectiveMonthsPFT.Select(x => x.EffectiveMonth))
        //                              .Where(e => e.Id == IntPolicyId).SingleOrDefault();
        //        var EffectiveMonths = EffectiveMonths1.StatutoryEffectiveMonthsPFT.Select(r => r.EffectiveMonth).ToList();
        //        IntPostMonth = EffectiveMonths.Select(x => x.LookupVal).ToList();
        //        if (IntPostMonth.Contains(MonthYearFull) != true)
        //        {
        //            Msg.Add(" Interest Posting Month is not valid ");
        //            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //        }
        //        //Check Interest posting Month end

        //        Msg = P2BUltimate.Process.GlobalProcess.InterestPostingAllEmp(CompCodeid.Id, qurey.Id, Postingdate,
        //    null, DBTrack);

        //        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

        //    }
        //}24022024

    }
}