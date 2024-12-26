using CMS_SPS;
using DocumentFormat.OpenXml.ExtendedProperties;
using IR;
using P2b.Global;
using P2B.API;
using P2B.PFTRUST;
using P2BUltimate.App_Start;
using P2BUltimate.Models;
using P2BUltimate.Process;
using P2BUltimate.Security;
using Payroll;
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
using System.Web.Script.Serialization;
using System.Windows.Interop;

namespace P2BUltimate.Controllers.PFTrust
{
    public class EmployeePFTrustController : Controller
    {
        // GET: PFTEmployeeLedger
        public ActionResult Index()
        {
            return View("~/Views/PFTrust/EmployeePFTrust/index.cshtml");
        }

        //public ActionResult ChkProcess(string typeofbtn, string month, string EmpCode)
        //{
        //    using (DataBaseContext db = new DataBaseContext())
        //    {
        //        bool selected = false;
        //        var query1 = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.SalaryT).Where(e => e.Employee.EmpCode == EmpCode).Select(e => e.SalaryT).SingleOrDefault();
        //        var query = query1.Where(e => e.PayMonth == month).ToList();

        //        if (query.Count > 0)
        //        {
        //            selected = true;
        //        }
        //        var data = new
        //        {
        //            status = selected,
        //        };
        //        return Json(data, JsonRequestBehavior.AllowGet);
        //    }
        //}

        #region DDL
        public ActionResult PopulateTransactionDropDownList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var query = db.PayProcessGroup.ToList();
                var selected = (Object)null;
                if (data2 != "" && data != "0" && data2 != "0")
                {
                    selected = Convert.ToInt32(data2);
                }

                SelectList s = new SelectList(query, "Id", "FullDetails", selected);
                return Json(s, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
        public JsonResult GetEmpLTCBlock(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                string pfdateconvert = "";
                string pensiondateconvert = "";
                //var a = db.PayScaleAgreement.Find(int.Parse(data));
                int Emp_Id = int.Parse(data);
                var emppfno = db.Employee.Include(e => e.EmpOffInfo).Include(e => e.EmpOffInfo.NationalityID).Where(e => e.Id == Emp_Id).SingleOrDefault();
                var a = emppfno.EmpOffInfo.NationalityID.PFNo == null ? "0" : emppfno.EmpOffInfo.NationalityID.PFNo;
               // var a = db.EmpOff.Include(e => e.NationalityID).Where(e => e.Id == Emp_Id).Select(e => e.NationalityID.PFNo).FirstOrDefault();
               // int c = Convert.ToInt32(a);
               // var b = db.EmpOff.Include(e => e.NationalityID).Where(e => e.Id == Emp_Id).Select(p => p.NationalityID.PensionNo).FirstOrDefault();
                var b = emppfno.EmpOffInfo.NationalityID.PensionNo == null ? "0" : emppfno.EmpOffInfo.NationalityID.PensionNo;
                if (b == "null")
                {
                    b = "";
                }

                var d = db.Employee.Include(e => e.ServiceBookDates).Where(e => e.Id == Emp_Id)
                    .Select(p => p.ServiceBookDates.PFJoingDate).FirstOrDefault();
                var pfdate = Convert.ToString(d);
                if (pfdate == "")
                {
                    pfdate = Convert.ToString(pfdate);

                }
                else
                {
                    pfdate = Convert.ToString(d);
                    pfdateconvert = Convert.ToDateTime(pfdate).ToString("dd/MM/yyyy");
                }


                var f = db.Employee.Include(e => e.ServiceBookDates).Where(e => e.Id == Emp_Id).Select(p => p.ServiceBookDates.PensionJoingDate).FirstOrDefault();
                var pensiondate = Convert.ToString(f);
                if (pensiondate == "")
                {
                    pensiondate = "";
                }
                else
                {
                    pensiondate = Convert.ToString(f);
                    pensiondateconvert = Convert.ToDateTime(pensiondate).ToString("dd/MM/yyyy");

                }

                //int d = Convert.ToInt32(b);
                var result = new { a, b, pfdateconvert, pensiondateconvert };
                return Json(result, JsonRequestBehavior.AllowGet);
                //return Json(  a, b, JsonRequestBehavior.AllowGet );


            }




        }
        public JsonResult GetInterestcarry(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Id = Convert.ToInt32(SessionManager.CompanyId);
                var CompCodeid = db.Company.Where(e => e.Id == Id).SingleOrDefault();
                var qurey = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "PFCALENDAR".ToUpper() && e.Default == true).AsEnumerable()
                  .Select(e => new
                  {
                      Id = e.Id,
                      Lvcalendardesc = "FromDate :" + e.FromDate.Value.ToShortDateString() + " ToDate :" + e.ToDate.Value.ToShortDateString(),
                      FromDate = e.FromDate.Value.ToShortDateString(),
                      ToDate = e.ToDate.Value.ToShortDateString()

                  }).SingleOrDefault();

                var CompantPFTrust_Id = db.CompanyPFTrust.Where(e => e.Company_Id == CompCodeid.Id).Select(e => e.Id).SingleOrDefault();
                var IntPolicyId = db.PFTACCalendar.Include(e => e.PFTTDSMaster).Include(e => e.CompanyPFTrust).Include(e => e.InterestPolicies).Include(e => e.InterestPolicies.InterestRate).Where(e => e.CompanyPFTrust.Id == CompantPFTrust_Id && e.PFTCalendar.Id == qurey.Id).SingleOrDefault();
                bool intcarr = IntPolicyId.InterestPolicies.IsIntCarryForward;
                
                bool selected = false;
                if (intcarr == true)
                {
                    selected = true;
                }
                return Json(selected, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult Create(EmployeePFTrust pFTrust, FormCollection form)
        {

            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> list = new List<string>();
                List<string> Msg = new List<string>();
                string Employee_Id = form["Employee-Table"] == "0" ? "0" : form["Employee-Table"];
                var Employee = Convert.ToInt32(Employee_Id);
                string OwnOpenBal = form["OwnOpenBal"] == "0" ? "0" : form["OwnOpenBal"];
                var PFMonthly = Convert.ToDouble(OwnOpenBal);
                string OwnerOpenBal = form["OwnerOpenBal"] == "0" ? "0" : form["OwnerOpenBal"];
                var OwnerMonthly = Convert.ToDouble(OwnerOpenBal);
                string VPFOpenBal = form["VPFOpenBal"] == "0" ? "0" : form["VPFOpenBal"];
                var AmountMonthly = Convert.ToDouble(VPFOpenBal);
                string OwnIntOpenBal = form["OwnIntOpenBal"] == "0" ? "0" : form["OwnIntOpenBal"];
                var PFInt = Convert.ToDouble(OwnIntOpenBal);
                string OwnerIntOpenBal = form["OwnerIntOpenBal"] == "0" ? "0" : form["OwnerIntOpenBal"];
                var OwnerInt = Convert.ToDouble(OwnerIntOpenBal);
                string VPFIntOpenBal = form["VPFIntOpenBal"] == "0" ? "0" : form["VPFIntOpenBal"];
                var vpfint = Convert.ToDouble(VPFIntOpenBal);
                //13/10/2023 added
                string PfopenBal = form["PfopenBal"] == "0" ? "0" : form["PfopenBal"];
                string PfIntopenBal = form["PfIntopenBal"] == "0" ? "0" : form["PfIntopenBal"];               
                //13/10/2023 added
                string TotalIntOpenBal = form["TotalIntOpenBal"] == "0" ? "0" : form["TotalIntOpenBal"];
                var TotalInt = Convert.ToDouble(TotalIntOpenBal);
                var PassbookActivity = form["PassbookActivity"] == "0" ? "0" : form["PassbookActivity"];
                string TDSAppl = form["TDSAppl"] == "0" ? "0" : form["TDSAppl"];

                int PassbookActId = Convert.ToInt32(PassbookActivity);

                var chkPassbook = db.LookupValue.Where(e => e.Id == PassbookActId).FirstOrDefault();
                if (chkPassbook != null)
                {
                    if (chkPassbook.LookupVal.ToUpper() != "PF BALANCE")
                    {
                        Msg.Add("Please Select PassbookActivity PF BALANCE");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                }

                string PostingDate = form["PostingDate"] == "0" ? "0" : form["PostingDate"];
                var Posting = Convert.ToDateTime(PostingDate);

                string CalcDate = form["CalcDate"] == "0" ? "0" : form["CalcDate"];
                var Calc = Convert.ToDateTime(CalcDate);
                string TrustPFNo = form["TrustPFNo"] == "0" ? "0" : form["TrustPFNo"];


                if (Employee_Id == null && Employee_Id == "")
                {
                    Msg.Add("Please Select Employee");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                }
                EmployeePFTrust OEmployeePFTrust = null;
                Employee OEmployee = null;
                int EmpId = Employee;
                OEmployeePFTrust
                              = db.EmployeePFTrust
                              .Include(e => e.PFTEmployeeLedger)
                              .Where(e => e.Employee.Id == EmpId).SingleOrDefault();

                OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct)
                                            .Where(r => r.Id == EmpId).SingleOrDefault();
                
                if (OEmployeePFTrust==null)
                {
                      Msg.Add("EmployeePFTrust id not Genrated");
                     return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);  
                }
                //only one record insert(PF Balance)
                if (OEmployeePFTrust.PFTEmployeeLedger.Count()>0)
                {
                   Msg.Add("Record Already Exists");
                   return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);  
                }
                //var ID = db.EmployeePFTrust.Select(e => e.Employee_Id).ToList();
                //foreach (var id in ID)
                //{
                //    if (id == EmpId)
                //    {
                       
                //        Msg.Add("Employee Record Already Exists");
                //        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                //    }

                //}

                //var CompanyPayrollId = db.CompanyPayroll.FirstOrDefault();

                int comp_Id = 0;
                comp_Id = Convert.ToInt32(Session["CompId"]);
                var CompanyPayroll = new CompanyPayroll();
                CompanyPayroll = db.CompanyPayroll.Where(e => e.Id == comp_Id).SingleOrDefault();

                if (ModelState.IsValid)
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        pFTrust.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };
                        try
                        {
                            List<PFTEmployeeLedger> PFTEmployeeLedger = new List<PFTEmployeeLedger>();

                            PFTEmployeeLedger objPFTEmployeeLedger = new PFTEmployeeLedger()
                            {
                                OwnOpenBal = PFMonthly,
                                OwnerOpenBal = OwnerMonthly,
                                OwnIntOpenBal = PFInt,
                                OwnerIntOpenBal = OwnerInt,
                                VPFOpenBal = AmountMonthly,
                                VPFIntOpenBal = vpfint,
                                TotalIntOpenBal = TotalInt,
                                OwnCloseBal = PFMonthly,
                                OwnerCloseBal = OwnerMonthly,
                                OwnIntCloseBal = PFInt,
                                OwnerIntCloseBal = OwnerInt,
                                VPFCloseBal = AmountMonthly,
                                VPFIntCloseBal = vpfint,
                                TotalIntCloseBal = TotalInt,
                                PFOpenBal=Convert.ToDouble(PfopenBal),
                                PFCloseBal = Convert.ToDouble(PfopenBal),
                                PFIntOpenBal=Convert.ToDouble(PfIntopenBal),
                                PFIntCloseBal = Convert.ToDouble(PfIntopenBal),
                                PassbookActivity = db.LookupValue.Find(PassbookActId),
                                PostingDate = Posting,
                                CalcDate = Calc,
                                MonthYear = Convert.ToDateTime(Posting).ToString("MM/yyyy"),
                                IsTDSAppl=Convert.ToBoolean(TDSAppl),
                                DBTrack = pFTrust.DBTrack,
                                Narration="Opening Balance"

                            };
                            db.PFTEmployeeLedger.Add(objPFTEmployeeLedger);
                          //  PFTEmployeeLedger.Add(objPFTEmployeeLedger);
                            db.SaveChanges();
                           
                            PFTEmployeeLedger.Add(db.PFTEmployeeLedger.Find(objPFTEmployeeLedger.Id));

                            if (OEmployeePFTrust == null)
                            {
                                EmployeePFTrust OTEP = new EmployeePFTrust()
                                {
                                    Employee = db.Employee.Find(OEmployee.Id),
                                    PFTEmployeeLedger = PFTEmployeeLedger,
                                    DBTrack = pFTrust.DBTrack

                                };


                                db.EmployeePFTrust.Add(OTEP);
                                db.SaveChanges();
                            }
                            else
                            {
                                var aa = db.EmployeePFTrust.Find(OEmployeePFTrust.Id);
                                aa.PFTEmployeeLedger = PFTEmployeeLedger;
                                db.EmployeePFTrust.Attach(aa);
                                db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                db.Entry(aa).State = System.Data.Entity.EntityState.Detached;

                            }
                            //var company = db.CompanyPFTrust.Select(e => e.Id).FirstOrDefault();

                            //EmployeePFTrust EmployeePFTrust = new EmployeePFTrust()
                            //{
                            //    DBTrack = pFTrust.DBTrack,
                            //    CompanyPFTrust_Id = company,
                            //    Employee_Id = Employee,
                            //    TrustPFNo = TrustPFNo,
                            //    PFTEmployeeLedger = PFTEmployeeLedger,



                            //};
                            //db.EmployeePFTrust.Add(EmployeePFTrust);
                            //db.SaveChanges();


                            ts.Complete();
                            Msg.Add("  Data Saved successfully  ");
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




        }
        public ActionResult GetLookupValueDATA(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                SelectList svaldata = (SelectList)null;
                var selected = "";
                List<string> AppStatus = new List<string> { "0", "1" };
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
        public class P2BCrGridData
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }

        }
        public ActionResult LoadEmp(P2BGrid_Parameters gp)
        {
            try
            {
                DateTime? dt = null;
                string monthyr = "";
                DataBaseContext db = new DataBaseContext();
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                var jsonData = (Object)null;
                IEnumerable<P2BCrGridData> EmpList = null;
                List<P2BCrGridData> model = new List<P2BCrGridData>();
                P2BCrGridData view = null;
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


                var compid = Convert.ToInt32(Session["CompId"].ToString());
                var empdata = db.CompanyPayroll
                    .Include(e => e.EmployeePayroll)
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee))
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee.EmpName))
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee.EmpName))
                    .Include(e => e.EmployeePayroll.Select(a => a.Employee.ServiceBookDates))
                    .Where(e => e.Company.Id == compid).SingleOrDefault();

                var emp = empdata.EmployeePayroll.Select(e => e.Employee).ToList();

                foreach (var z in emp)
                {

                    view = new P2BCrGridData()
                    {
                        Id = z.Id,
                        Code = z.EmpCode,
                        Name = z.EmpName.FullNameFML
                    };
                    if (z.ServiceBookDates.ServiceLastDate == null)
                    {
                        model.Add(view);
                    }
                    else if (Convert.ToDateTime("01/" + z.ServiceBookDates.ServiceLastDate.Value.ToString("MM/yyyy")) >= Convert.ToDateTime("01/" + PayMonth))
                    {
                        model.Add(view);
                    }
                    //else if (z.ServiceBookDates.ServiceLastDate.Value.ToString("MM/yyyy") != PayMonth)
                    //{
                    //    model.Add(view);
                    //}


                }
                EmpList = model;

                IEnumerable<P2BCrGridData> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = EmpList;
                    if (gp.searchOper.Equals("eq"))
                    {
                        if (gp.searchField == "Id")
                            jsonData = IE.Select(a => new { a.Id, a.Code, a.Name }).Where(e => (e.Id.ToString().Contains(gp.searchString))
                                || (e.Code.ToString().Contains(gp.searchString))
                                || (e.Name.ToString().Contains(gp.searchString))
                                ).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Code, a.Name }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = EmpList;
                    Func<P2BCrGridData, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id.ToString() :
                                         gp.sidx == "EmpCode" ? c.Code.ToString() :
                                         gp.sidx == "EmpName" ? c.Name.ToString() : "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), a.Name }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), a.Name }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, Convert.ToString(a.Code), a.Name }).ToList();
                    }
                    totalRecords = EmpList.Count();
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




        public class P2BGridData
        {
            public int Id { get; set; }
            public Employee Employee { get; set; }
            public DateTime PostingDate { get; set; }
            public double OwnOpenBal { get; set; }
            public double OwnerOpenBal { get; set; }
            public double VPFOpenBal { get; set; }
            public double OwnIntOpenBal { get; set; }
            public double OwnerIntOpenBal { get; set; }
            public double VPFIntOpenBal { get; set; }
            public double TotalIntOpenBal { get; set; }


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


                int company_Id = 0;
                company_Id = Convert.ToInt32(Session["CompId"]);

                var Empdata = db.EmployeePFTrust.Include(e => e.Employee).Include(e => e.Employee.EmpName).Include(e => e.PFTEmployeeLedger).Include(e => e.PFTEmployeeLedger.Select(x=>x.PassbookActivity)).ToList();


                //var BindCompList = db.PFTEmployeeLedger.ToList();

                foreach (var z in Empdata)
                {
                    var a = z.PFTEmployeeLedger.Where(e => e.PassbookActivity.LookupVal.ToUpper() == "PF BALANCE").OrderBy(e=>e.Id).FirstOrDefault();

                    //foreach (var a in pftemployeledgerdata)
                    //{

                        //var aa = db.PFTEmployeeLedger.Where(e => e.Id == a.Id).SingleOrDefault();
                    if (a != null)
                    {

                        view = new P2BGridData()
                          {
                              Id = a.Id,
                              Employee = z.Employee,
                              PostingDate = a.PostingDate,
                              OwnOpenBal = a.OwnOpenBal,
                              OwnerOpenBal = a.OwnerOpenBal,
                              VPFOpenBal = a.VPFOpenBal,
                              OwnIntOpenBal = a.OwnIntOpenBal,
                              OwnerIntOpenBal = a.OwnerIntOpenBal,
                              VPFIntOpenBal = a.VPFIntOpenBal,
                              TotalIntOpenBal = a.TotalIntOpenBal,
                              //TotalInt = a.TotalInt,

                          };

                        model.Add(view);
                    }
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
                              || (e.Employee.EmpCode.ToString().Contains(gp.searchString))
                              || (e.Employee.EmpName.FullNameFML.ToString().Contains(gp.searchString))
                              || (e.PostingDate.ToString().Contains(gp.searchString))
                              || (e.OwnOpenBal.ToString().Contains(gp.searchString))
                              || (e.OwnerOpenBal.ToString().Contains(gp.searchString))
                              || (e.OwnIntOpenBal.ToString().Contains(gp.searchString))
                              || (e.OwnerIntOpenBal.ToString().Contains(gp.searchString))
                              || (e.VPFOpenBal.ToString().Contains(gp.searchString))
                              || (e.VPFIntOpenBal.ToString().Contains(gp.searchString))
                              || (e.TotalIntOpenBal.ToString().Contains(gp.searchString))
                              || (e.Id.ToString().Contains(gp.searchString))
                                ).Select(a => new Object[] { a.Id, a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.PostingDate.ToShortDateString(), a.OwnOpenBal, a.OwnerOpenBal, a.VPFOpenBal, a.OwnIntOpenBal, a.OwnerIntOpenBal, a.VPFIntOpenBal, a.TotalIntOpenBal, a.Id }).ToList();
                        //jsonData = IE.Select(a => new Object[] { a.Id, Convert.ToString(a.Code), Convert.ToString(a.Name), Convert.ToString(a.BusinessType) != null ? Convert.ToString(a.BusinessType.Id) : "" }).Where(a => a.Contains(Convert.ToInt32(gp.searchString))).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.PostingDate.ToShortDateString(), a.OwnOpenBal, a.OwnerOpenBal, a.VPFOpenBal, a.OwnIntOpenBal, a.OwnerIntOpenBal, a.VPFIntOpenBal, a.TotalIntOpenBal, a.Id }).ToList();
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
                                         gp.sidx == "EmpCode" ? c.Employee.EmpCode.ToString() :
                                         gp.sidx == "EmpName" ? c.Employee.EmpName.FullNameFML.ToString() :
                                         gp.sidx == "OwnPFMonthly" ? c.OwnOpenBal.ToString() :
                                         gp.sidx == "OwnPFMonthly" ? c.OwnOpenBal.ToString() :
                                         gp.sidx == "OwnerPFMonthly" ? c.OwnerOpenBal.ToString() :
                                         gp.sidx == "VPFAmountMonthly" ? c.VPFOpenBal.ToString() :
                                         gp.sidx == "OwnPFInt" ? c.OwnIntOpenBal.ToString() :
                                         gp.sidx == "OwnerPFInt" ? c.OwnerIntOpenBal.ToString() :
                                         gp.sidx == "VPFInt" ? c.VPFIntOpenBal.ToString() :
                                         gp.sidx == "TotalInt" ? c.TotalIntOpenBal.ToString() : "");

                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.PostingDate.ToShortDateString(), a.OwnOpenBal, a.OwnerOpenBal, a.VPFOpenBal, a.OwnIntOpenBal, a.OwnerIntOpenBal, a.VPFIntOpenBal, a.TotalIntOpenBal, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.Id, a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.PostingDate.ToShortDateString(), a.OwnOpenBal, a.OwnerOpenBal, a.VPFOpenBal, a.OwnIntOpenBal, a.OwnerIntOpenBal, a.VPFIntOpenBal, a.TotalIntOpenBal, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.Id, a.Employee.EmpCode, a.Employee.EmpName.FullNameFML, a.PostingDate.ToShortDateString(), a.OwnOpenBal, a.OwnerOpenBal, a.VPFOpenBal, a.OwnIntOpenBal, a.OwnerIntOpenBal, a.VPFIntOpenBal, a.TotalIntOpenBal, a.Id }).ToList();
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

        public async Task<ActionResult> Delete(int data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    EmployeePFTrust EmployeePFTrust = db.EmployeePFTrust
                                                      .Include(e => e.PFTEmployeeLedger)
                                                     .Where(e => e.Id == data).SingleOrDefault();


                    if (EmployeePFTrust.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = EmployeePFTrust.DBTrack.CreatedBy != null ? EmployeePFTrust.DBTrack.CreatedBy : null,
                                CreatedOn = EmployeePFTrust.DBTrack.CreatedOn != null ? EmployeePFTrust.DBTrack.CreatedOn : null,
                                IsModified = EmployeePFTrust.DBTrack.IsModified == true ? true : false
                            };

                            await db.SaveChangesAsync();

                            ts.Complete();

                            Msg.Add("  Data removed.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {


                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {



                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now,
                                CreatedBy = EmployeePFTrust.DBTrack.CreatedBy != null ? EmployeePFTrust.DBTrack.CreatedBy : null,
                                CreatedOn = EmployeePFTrust.DBTrack.CreatedOn != null ? EmployeePFTrust.DBTrack.CreatedOn : null,
                                IsModified = EmployeePFTrust.DBTrack.IsModified == true ? false : false//,

                            };



                            db.Entry(EmployeePFTrust).State = System.Data.Entity.EntityState.Deleted;

                            await db.SaveChangesAsync();



                            ts.Complete();

                            Msg.Add("  Data removed.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
                catch (RetryLimitExceededException /* dex */)
                {

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
                    Msg.Add(ex.Message);
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                }
            }
        }



        public class PFTEmployeeLedgerEdit
        {
            public int PFNo { get; set; }
            public int PensionNo { get; set; }



        }

        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                //var a = db.PFTEmployeeLedger.Select(e => e.PostingDate).ToString().FirstOrDefault();
                var Q = db.PFTEmployeeLedger.Include(e=>e.PassbookActivity).Where(e => e.Id == data)
                    .Select
                    (e => new
                    {
                        OwnOpenBal = e.OwnOpenBal,
                        OwnerOpenBal = e.OwnerOpenBal,
                        VPFOpenBal = e.VPFOpenBal,
                        OwnIntOpenBal = e.OwnIntOpenBal,
                        OwnerIntOpenBal = e.OwnerIntOpenBal,
                        VPFIntOpenBal = e.VPFIntOpenBal,
                        TotalIntOpenBal = e.TotalIntOpenBal,
                        PfopenBal= e.PFOpenBal,
                        PfIntopenBal= e.PFIntOpenBal,
                        PostingDate = e.PostingDate,
                        CalcDate = e.CalcDate,
                        PassbookActivity = e.PassbookActivity != null ? e.PassbookActivity.Id : 0,
                        TDSAppl=e.IsTDSAppl,
                        Action = e.DBTrack.Action
                    }).ToList();

                //List<PFTEmployeeLedgerEdit> oreturnEditClass = new List<PFTEmployeeLedgerEdit>();
                //var k = db.EmployeePFTrust
                //   .Include(e => e.EmployeePFTrust.Select(a => a.Id))
                //  .Where(e => e.Id == data && e.PFTEmployeeLedger.Count > 0).ToList();

                var b = db.EmployeePFTrust.Where(e => e.Id == data).Select(d => new
                {
                    Employee_Id = d.Employee_Id,
                    TrustPFNo = d.TrustPFNo,
                }).ToList();

                var empid = db.EmployeePFTrust.Where(e => e.Id == data).Select(e => e.Employee_Id).FirstOrDefault();
                var employee = db.Employee.Include(e => e.EmpOffInfo).Include(e => e.EmpOffInfo.NationalityID).Include(e => e.ServiceBookDates).Where(e => e.Id == empid).Select(e => new
                {
                    PFNo = e.EmpOffInfo.NationalityID.PFNo == null ? "0" : e.EmpOffInfo.NationalityID.PFNo,
                    PensionNo = e.EmpOffInfo.NationalityID.PensionNo == null ? "0" : e.EmpOffInfo.NationalityID.PensionNo,
                    PFJoingDate = e.ServiceBookDates.PFJoingDate.ToString(),
                    PensionJoingDate = e.ServiceBookDates.PensionJoingDate.ToString()

                }).FirstOrDefault();





                //foreach (var e in k)
                //{
                //    oreturnEditClass.Add(new PFTEmployeeLedgerEdit
                //    {
                //        EmployeePFTrust_Id = e.PFTEmployeeLedger.ToArray(),
                //        EmployeePFTrustFullDetails = e.PFTEmployeeLedger.Where(a => a.Id == data).ToArray()
                //    });
                //}
                return Json(new Object[] { Q, b, employee, "", "", JsonRequestBehavior.AllowGet });
            }
        }

        [HttpPost]
        public async Task<ActionResult> EditSave(EmployeePFTrust c, int data, FormCollection form) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    string Employee_Id = form["Employee-Table"] == "0" ? "0" : form["Employee-Table"];
                    var Employee = Convert.ToInt32(Employee_Id);
                    string OwnOpenBal = form["OwnOpenBal"] == "0" ? "0" : form["OwnOpenBal"];
                    var PFMonthly = Convert.ToDouble(OwnOpenBal);
                    string OwnerOpenBal = form["OwnerOpenBal"] == "0" ? "0" : form["OwnerOpenBal"];
                    var OwnerMonthly = Convert.ToDouble(OwnerOpenBal);
                    string VPFOpenBal = form["VPFOpenBal"] == "0" ? "0" : form["VPFOpenBal"];
                    var AmountMonthly = Convert.ToDouble(VPFOpenBal);
                    string OwnIntOpenBal = form["OwnIntOpenBal"] == "0" ? "0" : form["OwnIntOpenBal"];
                    var PFInt = Convert.ToDouble(OwnIntOpenBal);
                    string OwnerIntOpenBal = form["OwnerIntOpenBal"] == "0" ? "0" : form["OwnerIntOpenBal"];
                    var OwnerInt = Convert.ToDouble(OwnerIntOpenBal);
                    string VPFIntOpenBal = form["VPFIntOpenBal"] == "0" ? "0" : form["VPFIntOpenBal"];
                    var vpfint = Convert.ToDouble(VPFIntOpenBal);
                    string TotalIntOpenBal = form["TotalIntOpenBal"] == "0" ? "0" : form["TotalIntOpenBal"];
                    var TotalInt = Convert.ToDouble(TotalIntOpenBal);
                    //13/10/2023
                    string PfopenBal = form["PfopenBal"] == "0" ? "0" : form["PfopenBal"];
                    string PfIntopenBal = form["PfIntopenBal"] == "0" ? "0" : form["PfIntopenBal"]; 
                    //13/10/2023
                    var PassbookActivity = form["PassbookActivity"] == "0" ? "0" : form["PassbookActivity"];

                    int PassbookActId = Convert.ToInt32(PassbookActivity);
                    string TDSAppl = form["TDSAppl"] == "0" ? "0" : form["TDSAppl"];

                    var chkPassbook = db.LookupValue.Where(e => e.Id == PassbookActId).FirstOrDefault();
                    if (chkPassbook != null)
                    {
                        if (chkPassbook.LookupVal.ToUpper() != "PF BALANCE")
                        {
                            Msg.Add("Please Select PassbookActivity PF BALANCE");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }


                    string PostingDate = form["PostingDate"] == "0" ? "" : form["PostingDate"];
                    var Posting = Convert.ToDateTime(PostingDate);
                    string CalcDate = form["CalcDate"] == "0" ? "" : form["CalcDate"];
                    var Calc = Convert.ToDateTime(CalcDate);
                    string TrustPFNo = form["TrustPFNo"] == "0" ? "" : form["TrustPFNo"];



                    var pftid = db.PFTEmployeeLedger.Where(e => e.Id == data).SingleOrDefault();

                    var db_data = db.EmployeePFTrust.Where(e => e.Id == pftid.EmployeePFTrust_Id).SingleOrDefault();
                    var db_datacheck = db.EmployeePFTrust.Include(e=>e.PFTEmployeeLedger).Where(e => e.Id == pftid.EmployeePFTrust_Id).SingleOrDefault();
                    if (db_datacheck.PFTEmployeeLedger.Count()>1)
                    {
                      Msg.Add("you can not edit because PF Uploadded");
                      return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);   
                    }

                    //List<EmployeePFTrust> EmployeePFTrust = new List<EmployeePFTrust>();
                    //var pftemployeledgerdata = db_data.EmployeePFTrust.ToList();
                    //if (EmployeePFTrust != null)
                    //{
                    //    //var ids = Utility.StringIdsToListIds(EmployeeDocuments);
                    //    foreach (var ca in pftemployeledgerdata)
                    //    {

                    //        var enquiryreportdoc_val = db.EmployeePFTrust.Find(ca);

                    //        EmployeePFTrust.Add(enquiryreportdoc_val);

                    //        db_data.EmployeePFTrust = EmployeePFTrust;
                    //    }
                    //}
                    //else
                    //{
                    //    db_data.PFTEmployeeLedger = null;
                    //}

                    if (ModelState.IsValid)
                    {
                        try
                        {

                            using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                            {
                                //ChargeSheet blog = null; // to retrieve old data
                                //DbPropertyValues originalBlogValues = null;

                                //using (var context = new DataBaseContext())
                                //{
                                //    blog = context.ChargeSheet.Where(e => e.Id == data).SingleOrDefault();


                                //    originalBlogValues = context.Entry(blog).OriginalValues;
                                //}

                                c.DBTrack = new DBTrack
                                {
                                    //CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                    //CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                    Action = "M",
                                    ModifiedBy = SessionManager.UserName,
                                    ModifiedOn = DateTime.Now
                                };

                                var m1 = db.PFTEmployeeLedger.Where(e => e.Id == data).ToList();
                              
                                foreach (var s in m1)
                                {
                                    // s.AppraisalPeriodCalendar = null;
                                    db.PFTEmployeeLedger.Attach(s);
                                    db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                    //await db.SaveChangesAsync();
                                    db.SaveChanges();
                                    TempData["RowVersion"] = s.RowVersion;
                                    db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                                }
                                var CurCorp = db.PFTEmployeeLedger.Find(data);
                                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                                {
                                    var company = db.CompanyPFTrust.Select(e => e.Id).FirstOrDefault();
                                    // c.DBTrack = dbT;
                                    PFTEmployeeLedger corp = new PFTEmployeeLedger()
                                    {
                                        EmployeePFTrust_Id=db_data.Id,
                                        OwnOpenBal = PFMonthly,
                                        OwnerOpenBal = OwnerMonthly,
                                        OwnIntOpenBal = PFInt,
                                        OwnerIntOpenBal = OwnerInt,
                                        VPFOpenBal = AmountMonthly,
                                        VPFIntOpenBal = vpfint,
                                        OwnCloseBal = PFMonthly,
                                        OwnerCloseBal = OwnerMonthly,
                                        OwnIntCloseBal = PFInt,
                                        OwnerIntCloseBal = OwnerInt,
                                        VPFCloseBal = AmountMonthly,
                                        VPFIntCloseBal = vpfint,
                                        TotalIntCloseBal = TotalInt,
                                        TotalIntOpenBal = TotalInt,
                                        PFOpenBal=Convert.ToDouble(PfopenBal),
                                        PFCloseBal = Convert.ToDouble(PfopenBal),
                                        PFIntOpenBal=Convert.ToDouble(PfIntopenBal),
                                        PFIntCloseBal = Convert.ToDouble(PfIntopenBal),
                                        PassbookActivity = db.LookupValue.Find(PassbookActId),
                                        PostingDate = Posting,
                                        CalcDate = Calc,
                                        MonthYear = Convert.ToDateTime(Posting).ToString("MM/yyyy"),
                                        DBTrack = c.DBTrack,
                                        IsTDSAppl=Convert.ToBoolean(TDSAppl),
                                        Narration="Opening Balance",
                                        Id = data

                                    };
                                    db.PFTEmployeeLedger.Attach(corp);
                                    db.Entry(corp).State = System.Data.Entity.EntityState.Modified;
                                    db.Entry(corp).OriginalValues["RowVersion"] = TempData["RowVersion"];
                                   

                                    await db.SaveChangesAsync();
                                    ts.Complete();
                                    Msg.Add("  Record Updated  ");
                                    //  return Json(new Utility.JsonReturnClass { Id = corp.Id, Val = corp.LocationObj.LocDesc, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }


                            }
                        }
                        catch (DbUpdateConcurrencyException ex)
                        {
                            var entry = ex.Entries.Single();
                            var clientValues = (EmployeePFTrust)entry.Entity;
                            var databaseEntry = entry.GetDatabaseValues();
                            if (databaseEntry == null)
                            {
                                Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                            }
                            else
                            {
                                var databaseValues = (EmployeePFTrust)databaseEntry.ToObject();
                                c.RowVersion = databaseValues.RowVersion;

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
                        // return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                    }

                    else
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            //ChargeSheet blog = null; // to retrieve old data
                            //DbPropertyValues originalBlogValues = null;


                            //using (var context = new DataBaseContext())
                            //{
                            //    blog = context.ChargeSheet.Where(e => e.Id == data).SingleOrDefault();
                            //    originalBlogValues = context.Entry(blog).OriginalValues;
                            //}
                            c.DBTrack = new DBTrack
                            {
                                //CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                //CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                Action = "M",
                                IsModified = db_data.DBTrack.IsModified == true ? true : false,
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };
                            PFTEmployeeLedger corp = new PFTEmployeeLedger()
                            {


                                Id = data,
                                OwnOpenBal = PFMonthly,
                                OwnerOpenBal = OwnerMonthly,
                                VPFOpenBal = AmountMonthly,
                                OwnIntOpenBal = PFInt,
                                OwnerIntOpenBal = OwnerInt,
                                VPFIntOpenBal = vpfint,
                                TotalIntOpenBal = TotalInt,
                                PassbookActivity = db.LookupValue.Find(PassbookActId),
                                PostingDate = Posting,
                                CalcDate = Calc,
                                DBTrack = c.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };

                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("  Record Updated  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { blog.Id, c.LocationObj.LocDesc, "Record Updated", JsonRequestBehavior.AllowGet });
                        }

                    }
                    return View();
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




        public class EditData
        {
            public int Id { get; set; }
            public string EmployeeCode { get; set; }
            public string EmployeeName { get; set; }
            public string PayMonth { get; set; }
            public string Surcharge { get; set; }
            public string TaxOnIncome { get; set; }
            public string TaxPaid { get; set; }
            public string Mode { get; set; }
            public bool Editable { get; set; }
        }
        public class DeserializeClass
        {
            public String Id { get; set; }
            public double TaxPaid { get; set; }
            public string EmpCode { get; set; }

        }

        public ActionResult P2BInlineGrid(P2BGrid_Parameters gp)
        {
            try
            {
                DataBaseContext db = new DataBaseContext();
                int pageIndex = gp.rows * Convert.ToInt32(gp.page) - gp.rows;
                int pageSize = gp.rows;
                int totalPages = 0;
                int totalRecords = 0;
                var jsonData = (Object)null;
                IEnumerable<EditData> ITaxTranst = null;

                List<EditData> model = new List<EditData>();
                var view = new EditData();
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
                var OEmployeePayroll = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.Employee.EmpName)
                    .Include(o => o.ITaxTransT).ToList();



                foreach (var z in OEmployeePayroll)
                {
                    foreach (var S in z.ITaxTransT)
                    {
                        if (S.PayMonth == PayMonth)
                        {


                            bool EditAppl = true;
                            view = new EditData()
                            {
                                Id = S.Id,
                                EmployeeCode = z.Employee.EmpCode != null ? Convert.ToString(z.Employee.EmpCode) : null,
                                EmployeeName = z.Employee.EmpName.FullNameFML != null ? Convert.ToString(z.Employee.EmpName.FullNameFML) : null,
                                PayMonth = S.PayMonth,
                                //  Surcharge = S.Surcharge.ToString(),
                                //  TaxOnIncome = S.TaxOnIncome.ToString(),
                                TaxPaid = S.TaxPaid.ToString(),
                                //  Mode = S.Mode,
                                Editable = EditAppl
                            };
                            model.Add(view);
                        }


                    }
                }

                ITaxTranst = model;

                IEnumerable<EditData> IE;
                if (!string.IsNullOrEmpty(gp.searchField) && !string.IsNullOrEmpty(gp.searchString))
                {
                    IE = ITaxTranst;
                    if (gp.searchOper.Equals("eq"))
                    {

                        jsonData = IE.Where(e => (e.EmployeeCode != null ? e.EmployeeCode.ToString().Contains(gp.searchString) : false)
                                        || (e.EmployeeName != null ? e.EmployeeName.ToUpper().ToString().Contains(gp.searchString.ToUpper()) : false)
                                        || (e.PayMonth != null ? e.PayMonth.ToString().Contains(gp.searchString.ToUpper()) : false)
                                        || (e.TaxPaid != null ? e.TaxPaid.ToUpper().ToString().Contains(gp.searchString.ToUpper()) : false)
                                        || (e.Editable.ToString().ToUpper().Contains(gp.searchString.ToUpper()))
                                        || (e.Id.ToString().Contains(gp.searchString.ToUpper()))
                                  ).Select(a => new Object[] { a.EmployeeCode != null ? Convert.ToString(a.EmployeeCode) : "", a.EmployeeName != null ? a.EmployeeName : "", a.PayMonth != null ? Convert.ToString(a.PayMonth) : "", a.TaxPaid != null ? Convert.ToString(a.TaxPaid) : "", a.Editable, a.Id }).ToList();


                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.EmployeeCode != null ? Convert.ToString(a.EmployeeCode) : "", a.EmployeeName != null ? a.EmployeeName : "", a.PayMonth != null ? Convert.ToString(a.PayMonth) : "", a.TaxPaid != null ? Convert.ToString(a.TaxPaid) : "", a.Editable, a.Id }).ToList();
                    }
                    totalRecords = IE.Count();
                }
                else
                {
                    IE = ITaxTranst;
                    Func<EditData, dynamic> orderfuc;
                    if (gp.sidx == "Id")
                    {
                        orderfuc = (c => gp.sidx == "Id" ? c.Id : 0);
                    }
                    else
                    {
                        orderfuc = (c => gp.sidx == "EmpCode" ? c.EmployeeCode.ToString() :
                                         gp.sidx == "EmpName" ? c.EmployeeName.ToString() :
                                         gp.sidx == "PayMonth" ? c.PayMonth.ToString() :
                                         gp.sidx == "TaxPaid" ? c.TaxPaid.ToString() :
                                         gp.sidx == "Editable" ? c.Editable.ToString() :
                                        "");
                    }
                    if (gp.sord == "asc")
                    {
                        IE = IE.OrderBy(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.EmployeeCode != null ? Convert.ToString(a.EmployeeCode) : "", a.EmployeeName != null ? a.EmployeeName : "", a.PayMonth != null ? Convert.ToString(a.PayMonth) : "", a.TaxPaid != null ? Convert.ToString(a.TaxPaid) : "", a.Editable, a.Id }).ToList();
                    }
                    else if (gp.sord == "desc")
                    {
                        IE = IE.OrderByDescending(orderfuc);
                        jsonData = IE.Select(a => new Object[] { a.EmployeeCode != null ? Convert.ToString(a.EmployeeCode) : "", a.EmployeeName != null ? a.EmployeeName : "", a.PayMonth != null ? Convert.ToString(a.PayMonth) : "", a.TaxPaid != null ? Convert.ToString(a.TaxPaid) : "", a.Editable, a.Id }).ToList();
                    }
                    if (pageIndex > 1)
                    {
                        int h = pageIndex * pageSize;
                        jsonData = IE.Skip(pageIndex).Take(pageSize).Select(a => new Object[] { a.EmployeeCode != null ? Convert.ToString(a.EmployeeCode) : "", a.EmployeeName != null ? a.EmployeeName : "", a.PayMonth != null ? Convert.ToString(a.PayMonth) : "", a.TaxPaid != null ? Convert.ToString(a.TaxPaid) : "", a.Editable, a.Id }).ToList();
                    }
                    totalRecords = ITaxTranst.Count();
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



        #region Create
        //public List<int> one_ids(string form)
        //{
        //    string itsec_id = form;
        //    List<int> ids = itsec_id.Split(',').Select(e => int.Parse(e)).ToList();
        //    return (ids);
        //}




        #endregion

        #region EDIT & EDITSAVE






        #endregion

        #region Delete

        #endregion

        #region TreeGridDetails
        public class returndatagridclass
        {
            public string Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
        }

        public ActionResult Emp_Grid(ParamModel param, string y)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {

                    var all = db.EmployeePayroll.Include(e => e.Employee).Include(e => e.YearlyPaymentT).ToList();
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
                    var dcompaines = fall
                            .Skip(param.iDisplayStart).Take(param.iDisplayLength).ToList();
                    if (dcompaines.Count == 0)
                    {
                        List<returndatagridclass> result = new List<returndatagridclass>();
                        foreach (var item in fall)
                        {
                            result.Add(new returndatagridclass
                            {
                                Id = item.Id.ToString(),
                                Code = item.Employee.CardCode,

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
                        var result = from c in dcompaines

                                     select new[] { null, Convert.ToString(c.Id), c.Employee.EmpCode };
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

        public class YearlyPaymentChildDataClass
        {
            public String Id { get; set; }
            public String TDSAmount { get; set; }
            public String AmonutPaid { get; set; }
            public String OtherDeduction { get; set; }
            public String Narration { get; set; }
        }

        public ActionResult GridEditSave(YearlyPaymentT ypay, FormCollection from, string data)
        {
            List<string> Msg = new List<string>();
            using (DataBaseContext db = new DataBaseContext())
            {
                if (data != null)
                {
                    var id = Convert.ToInt32(data);
                    var db_data = db.YearlyPaymentT.Where(e => e.Id == id).SingleOrDefault();
                    db_data.AmountPaid = ypay.AmountPaid;
                    try
                    {
                        db.YearlyPaymentT.Attach(db_data);
                        db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;
                        Msg.Add("  Record Updated");
                        return Json(new Utility.JsonReturnClass { data = db_data.ToString(), success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        // return Json(new { data = db_data, responseText = "Record Updated..!" }, JsonRequestBehavior.AllowGet);
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
                else
                {
                    Msg.Add("  Data Is Null  ");
                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                    //return Json(new { responseText = "Data Is Null" }, JsonRequestBehavior.AllowGet);
                }
            }
        }




    }
}

        #endregion