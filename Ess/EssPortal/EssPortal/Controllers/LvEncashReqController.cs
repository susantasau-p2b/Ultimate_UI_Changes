using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Core.Objects;
using System.Transactions;
using System.Threading.Tasks;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Reflection;
using P2b.Global;
using Leave;
using EssPortal.App_Start;
using EssPortal.Models;
using EssPortal.Security;
using System.Diagnostics;
using System.IO;

namespace P2BUltimate.Controllers.Leave.MainController
{
    [AuthoriseManger]
    public class LvEncashReqController : Controller
    {
        //  private DataBaseContext db = new DataBaseContext();
        public ActionResult Index()
        {
            return View("~/Views/LvEncashReq/Index.cshtml");
        }

        public ActionResult Partial()
        {
            return View("~/Views/Shared/_LvEncashReqView.cshtml");
        }

        public List<int> one_ids(string form)
        {
            string itsec_id = form;
            List<int> ids = itsec_id.Split(',').Select(e => int.Parse(e)).ToList();
            return (ids);
        }
        #region LOOKUPDETAILSPOPULATION

        public ActionResult GetLVNewReqDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.LvNewReq.ToList();
                IEnumerable<LvNewReq> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.LvNewReq.ToList().Where(d => d.FullDetails.Contains(data));
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

        public ActionResult GetLVWFDetails(string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var fall = db.LvWFDetails.ToList();
                IEnumerable<LvWFDetails> all;
                if (!string.IsNullOrEmpty(data))
                {
                    all = db.LvWFDetails.ToList().Where(d => d.FullDetails.Contains(data));
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
        #endregion


        #region DDList
        // public ActionResult PopulateDropDownStructureList(string data, string data2)
        // {


        //     int id = Convert.ToInt32(data2);
        //     var lk =
        //new SelectList((from s in db.EmployeeLeave.Include(e => e.LvNewReq).Include(e => e.LvNewReq.Select(t => t.LeaveHead))
        //                .Include(e => e.LvNewReq.Select(t => t.LeaveCalendar))
        //                     .Where(e => e.Employee.Id == id)
        //                    .ToList()
        //                from p in s.LvNewReq
        //                select new
        //                {

        //                    Id = p.Id,
        //                    NewReqDetails = p.FullDetails
        //                }).Distinct(), //
        //      "Id",
        //      "NewReqDetails",
        //        null);




        //     return Json(lk, JsonRequestBehavior.AllowGet);




        // }
        #endregion
        public ActionResult PopulateDropDownLvReq(string data, string data2)
        {
            //data-head value
            //emp-data2
            using (DataBaseContext db = new DataBaseContext())
            {
                int id = Convert.ToInt32(data2);
                int headid = Convert.ToInt32(data);
                //var wfst = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "1").FirstOrDefault();//db.LookupValue.Where(e => e.LookupVal == "1").SingleOrDefault();

                EmployeeLeave oEmployeeLeave = db.EmployeeLeave.Include(e => e.LvNewReq)
                 .Include(e => e.LvNewReq.Select(a => a.LeaveCalendar))
                 .Include(e => e.LvNewReq.Select(a => a.WFStatus))
             .Where(e => e.Employee.Id == id).OrderBy(e => e.Id).SingleOrDefault();
                if (oEmployeeLeave != null)
                {
                    var LvCalendarFilter = oEmployeeLeave.LvNewReq.OrderBy(e => e.Id).ToList();

                    var LvOrignal_id = LvCalendarFilter.Where(e => e.LvOrignal != null).OrderBy(e => e.Id).Select(e => e.LvOrignal.Id).ToList();

                    var lk =
                   new SelectList((from s in db.EmployeeLeave
                   .Include(e => e.LvNewReq)
                   .Include(e => e.LvNewReq.Select(t => t.LeaveHead))
                                   .Include(e => e.LvNewReq.Select(t => t.LeaveCalendar))
                                   .Include(e => e.LvNewReq.Select(a => a.WFStatus))
                                        .Where(e => e.Employee.Id == id)
                                       .ToList()
                                   from p in s.LvNewReq.Where(e => e.LeaveHead.Id == headid && !LvOrignal_id.Contains(e.Id) && e.IsCancel == false && e.TrClosed==true && e.TrReject == false && e.WFStatus != null && e.WFStatus.LookupVal != "2" && e.WFStatus.LookupVal != "3" && e.WFStatus.LookupVal != "0")
                                   select new
                                   {
                                       Id = p.Id,
                                       NewReqDetails = p.FullDetails
                                   }).Distinct(), //
                         "Id",
                   "NewReqDetails",
                           null);
                    return Json(lk, JsonRequestBehavior.AllowGet);
                }
                return null;
            }
        }
        public ActionResult GetLeaveReqMultiplier(string data, string data2) //Pass leavehead id here
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                int LvHead = int.Parse(data);

                var leaveReq = db.LvEncashPolicy.Where(e => e.LvHead.Id == LvHead && e.IsLvMultiple == true

                ).FirstOrDefault();
                if (leaveReq != null)
                {

                    return Json(new { sucess = true, data = leaveReq }, JsonRequestBehavior.AllowGet);

                }
                else
                {
                    return Json(new { sucess = false }, JsonRequestBehavior.AllowGet);
                }


            }
        }
        public class returnLeaveencashClass
        {
            public string Id { get; set; }
            public string LvMultiplier { get; set; }
        }
        public ActionResult GetLeaveReqMultipliervalue(string data, string data2) //Pass leavehead id here
        {

            using (DataBaseContext db = new DataBaseContext())
            {
                List<returnLeaveencashClass> oreturnleaveencashClass = new List<returnLeaveencashClass>();
                int LvHead = int.Parse(data);

                var leaveReq = db.LvEncashPolicy.Where(e => e.LvHead.Id == LvHead && e.IsLvMultiple == true

                ).FirstOrDefault();
                var selected = "";
                if (!string.IsNullOrEmpty(data2))
                {
                    selected = data2;
                }

                if (leaveReq != null)
                {
                    double maxday = leaveReq.MaxEncashment;
                    double multiplier = leaveReq.LvMultiplier;
                    int quotient = (int)(maxday / multiplier);
                    List<double> multival = new List<double>();
                    double mv = 0;
                    for (int i = 0; i < quotient; i++)
                    {
                        mv = mv + multiplier;
                        oreturnleaveencashClass.Add(new returnLeaveencashClass
                        {
                            Id = i.ToString(),
                            LvMultiplier = mv.ToString()
                        });
                        //mv = mv + multiplier;
                        //multival.Add(mv);

                    }

                    var returndata = new SelectList(oreturnleaveencashClass, "Id", "LvMultiplier", selected);
                    return Json(returndata, JsonRequestBehavior.AllowGet);
                    //  return Json(new Object[] { multival, JsonRequestBehavior.AllowGet });
                }
                else
                {
                    return Json(new { sucess = false }, JsonRequestBehavior.AllowGet);
                }


            }
        }

        public ActionResult PopulateDropDownListIncharge(string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                int Emp = int.Parse(data2);
                var excep = db.Employee.Include(e => e.EmpName).Where(e => e.Id == Emp).ToList();
                var query = db.Employee.Include(e => e.EmpName).OrderBy(e => e.EmpCode).ToList();
                var query2 = query.Except(excep);
                //   var qurey = db.Calendar.Include(e => e.Name).ToList();
                var selected = "";
                if (!string.IsNullOrEmpty(data2))
                {
                    selected = data2;
                }
                var returndata = new SelectList(query2, "Id", "FullDetails", selected);
                return Json(returndata, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult PopulateDropDownLvHeadList(string data, string data2)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var emp = Convert.ToInt32(data2);
                var HeadList = db.LvHead.Where(e => e.EncashRegular == true).ToList();
                //var newSelectList=new List<>
                if (HeadList.Count > 0)
                {
                    var ss = new SelectList(HeadList, "ID", "FullDetails", "");
                    return Json(ss, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
            }
        }
        #region CREATE

        public ActionResult Create(LvEncashReq L, FormCollection form, String forwarddata) //Create submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    var WFStatuslist = form["WFStatuslist"] == "0" ? "" : form["WFStatuslist"];
                    var LvNewReqlist = form["LvNewReqlist"] == "0" ? "" : form["LvNewReqlist"];
                    string Emp = form["employee-table"] == "0" ? "" : form["employee-table"];
                    string lvhead = form["LeaveHead_drop"] == "0" ? "" : form["LeaveHead_drop"];
                    var Id = Convert.ToInt32(SessionManager.CompanyId);
                    string CompCode = db.Company.Where(e => e.Id == Id).SingleOrDefault().Code.ToUpper();

                    if (lvhead != null)
                    {
                        if (lvhead != "")
                        {
                            var val = db.LvHead.Find(int.Parse(lvhead));
                            L.LvHead = val;
                        }
                    }


                    List<int> ids = null;
                    if (Emp != null && Emp != "0" && Emp != "false")
                    {
                        ids = one_ids(Emp);
                    }
                    else
                    {
                        Msg.Add("  Kindly select employee  ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //return Json(new Object[] { "", "", "Kindly select employee" }, JsonRequestBehavior.AllowGet);
                    }

                    //   Start Checking for policy
                    var Cal = db.Calendar.Where(e => e.Default == true && e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault();
                    double oLvClosingData = 0;
                    double UtilizedLv = 0;
                    double oLvOccurances = 0;
                    // Lvconert to other Leave start
                    double Encashreqday = 0;
                    double Encashreqconvertday = 0;
                    Boolean Partiallvconvert = false;
                    string convertlvhead = "";
                    // Lvconert to other Leave end
                    Int32 GeoStruct = 0;
                    Int32 PayStruct = 0;
                    Int32 FuncStruct = 0;
                    int chk = Convert.ToInt16(lvhead);
                    DateTime? Lvyearfrom = null;
                    DateTime? LvyearTo = null;
                    var encashpolicy = db.LvEncashPolicy.Where(e => e.LvHead.Id == chk).FirstOrDefault();

                    LvCreditPolicy encreditpolicy = null;

                    if (ModelState.IsValid)
                    {
                        if (ids != null)
                        {
                            foreach (var j in ids)
                            {
                                EmployeeLeave _Prv_EmpLvData = db.EmployeeLeave.Where(a => a.Employee.Id == j)
                                               .Include(a => a.LvNewReq)
                                               .Include(a => a.LvNewReq.Select(e => e.LeaveHead))
                                               .Include(a => a.LvNewReq.Select(e => e.LeaveCalendar))
                                               .Include(a => a.LvNewReq.Select(e => e.WFStatus))
                                               .Include(a => a.LvNewReq.Select(e => e.GeoStruct))
                                               .Include(a => a.LvNewReq.Select(e => e.PayStruct))
                                               .Include(a => a.LvNewReq.Select(e => e.FuncStruct))
                                                .Where(e => e.LvNewReq.Any(a => a.LeaveHead != null && a.LeaveHead.Id == chk))
                                                   .AsParallel()
                                                   .SingleOrDefault();

                                EmployeeLvStruct OLvSalStruct = db.EmployeeLvStruct
                                    .Include(e => e.EmployeeLvStructDetails)
                                    .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHead))
                                    .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula))
                                    .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvEncashPolicy))
                                     .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvCreditPolicy))
                                    .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvEncashPolicy.LvConvertPolicy))
                                    .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvEncashPolicy.LvConvertPolicy.Select(x => x.LvHead)))
                                    .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHeadFormula.LvEncashPolicy.LvConvertPolicy.Select(x => x.LvConvert)))
                                    .Include(e => e.EmployeeLvStructDetails.Select(q => q.LvHead.LvHeadOprationType))
                                        .Where(e => e.EndDate == null && e.EmployeeLeave_Id == _Prv_EmpLvData.Id).SingleOrDefault();

                                encreditpolicy = OLvSalStruct.EmployeeLvStructDetails.Where(e => e.LvHead.Id == chk && e.LvHeadFormula.LvCreditPolicy != null).Select(r => r.LvHeadFormula.LvCreditPolicy).FirstOrDefault();

                                encashpolicy = OLvSalStruct.EmployeeLvStructDetails.Where(e => e.LvHead.Id == chk && e.LvHeadFormula.LvEncashPolicy != null).Select(r => r.LvHeadFormula.LvEncashPolicy).FirstOrDefault();
                                if (encashpolicy == null)
                                {
                                    Msg.Add("Policy not defined for this employee..!");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }

                                // Lvconert to other Leave start
                                encashpolicy = OLvSalStruct.EmployeeLvStructDetails.Where(e => e.LvHead.Id == chk && e.LvHeadFormula != null && e.LvHeadFormula.LvEncashPolicy != null).Select(r => r.LvHeadFormula.LvEncashPolicy).FirstOrDefault();
                                if (encashpolicy.PartialLVConvert == true)
                                {
                                    Partiallvconvert = true;
                                    string encdays = Convert.ToString(L.EncashDays);
                                    var encconvertchk = encashpolicy.LvConvertPolicy.Where(e => e.LvConvert.LookupVal.ToString() == encdays).Select(e => new { e.LvConvert.LookupVal, e.LvConvert.LookupValData, e.LvHead }).FirstOrDefault();
                                    if (encconvertchk == null)
                                    {
                                        string eday = "";
                                        var encashapply = encashpolicy.LvConvertPolicy.Select(e => new { e.LvConvert.LookupVal, e.LvConvert.LookupValData }).ToList();
                                        foreach (var item in encashapply)
                                        {
                                            if (eday == "")
                                            {
                                                eday = item.LookupVal;
                                            }
                                            else
                                            {
                                                eday = eday + " or " + item.LookupVal;
                                            }

                                        }
                                        Msg.Add("Please apply encash days " + eday);
                                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    }
                                    else
                                    {
                                        convertlvhead = encconvertchk.LvHead.LvCode;
                                        Encashreqday = Convert.ToDouble(encconvertchk.LookupVal.ToString()) - Convert.ToDouble(encconvertchk.LookupValData.ToString());
                                        Encashreqconvertday = Convert.ToDouble(encconvertchk.LookupValData.ToString());
                                    }
                                }
                                else
                                {
                                    Partiallvconvert = false;
                                    Encashreqday = L.EncashDays;
                                }
                                // Lvconert to other Leave end

                                List<LvNewReq> Filter_oEmpLvData = new List<LvNewReq>();
                                if (_Prv_EmpLvData != null)
                                {
                                    Filter_oEmpLvData = _Prv_EmpLvData.LvNewReq
                                        .Where(a => a.LeaveHead != null && a.LeaveHead.Id == chk).ToList();
                                }

                                if (CompCode == "MSCB")
                                {
                                    if (db.LvHead.Where(e => e.Id == chk).FirstOrDefault().LvCode == "SLHP")
                                    {
                                        DateTime RetirementDt = (DateTime)db.Employee.Include(x => x.ServiceBookDates).Where(a => a.Id == j).SingleOrDefault().ServiceBookDates.RetirementDate;
                                        DateTime start = DateTime.Today;
                                        DateTime end = RetirementDt;
                                        int compMonth = (end.Month + end.Year * 12) - (start.Month + start.Year * 12);
                                        double daysInEndMonth = (end.AddMonths(1) - end).Days;
                                        double months = compMonth + Math.Abs((start.Day - end.Day) / daysInEndMonth);
                                        double mAge = Math.Abs(months / 12);

                                        if (mAge > 3)
                                        {
                                            Msg.Add("You can not encash SLHP.because Your Service More than 3 Years");
                                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        }
                                    }

                                }


                                EmployeeLeave CheckEncashSpanchk = db.EmployeeLeave.Include(t => t.LeaveEncashReq).Include(t => t.LeaveEncashReq.Select(e => e.LeaveCalendar))
                                                               .Include(t => t.LeaveEncashReq.Select(e => e.LvHead)).Where(t => t.Employee.Id == j).SingleOrDefault();

                                var Check = CheckEncashSpanchk.LeaveEncashReq.Where(e => e.IsCancel == false && e.TrReject == false && e.LvHead_Id == L.LvHead.Id).AsEnumerable().ToList();


                                if (Check.Where(e => e.FromPeriod >= L.FromPeriod && e.FromPeriod <= L.ToPeriod).Count() != 0 ||
                                    Check.Where(e => e.ToPeriod >= L.FromPeriod && e.ToPeriod <= L.ToPeriod).Count() != 0)
                                {
                                    Msg.Add(" You Can not Leave Encash for This Period, Already Exist...! \n\n    Kindly select another Period. ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                }

                                if (Check.Where(e => e.FromPeriod <= L.FromPeriod && e.ToPeriod >= L.ToPeriod).Count() != 0)
                                {
                                    Msg.Add(" You Can not Leave Encash for This Period, Already Exist...! \n\n    Kindly select another Period. ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                }


                                if (Filter_oEmpLvData.Count == 0)
                                {
                                    //get Data from opening
                                    var _Emp_EmpOpeningData = db.EmployeeLeave.Where(e => e.Employee.Id == j && e.LvOpenBal.Any(a => a.LvHead.Id == chk && a.LvCalendar.Id == Cal.Id))
                                        .Include(e => e.LvOpenBal.Select(a => a.LvHead))
                                        .Include(e => e.LvOpenBal.Select(a => a.LvCalendar))
                                        .Include(e => e.Employee.GeoStruct)
                                        .Include(e => e.Employee.PayStruct)
                                        .Include(e => e.Employee.FuncStruct)
                                        .SingleOrDefault();

                                    var _EmpOpeningData = _Emp_EmpOpeningData.LvOpenBal.Where(e => e.LvHead.Id == chk && e.LvCalendar.Id == Cal.Id).Select(e => e.LvOpening).SingleOrDefault();
                                    var _EmpOpeninglvoccranceData = _Emp_EmpOpeningData.LvOpenBal.Where(e => e.LvHead.Id == chk && e.LvCalendar.Id == Cal.Id).Select(e => e.LvOccurances).SingleOrDefault();

                                    var UtilizedLeavefromLvopebal = _Emp_EmpOpeningData.LvOpenBal.Where(e => e.LvHead.Id == chk && e.LvCalendar.Id == Cal.Id).Select(e => e.LVCount).SingleOrDefault();

                                    oLvOccurances = _EmpOpeninglvoccranceData;
                                    oLvClosingData = _EmpOpeningData;
                                    UtilizedLv = UtilizedLeavefromLvopebal;
                                    GeoStruct = _Emp_EmpOpeningData.Employee.GeoStruct.Id;
                                    PayStruct = _Emp_EmpOpeningData.Employee.PayStruct.Id;
                                    FuncStruct = _Emp_EmpOpeningData.Employee.FuncStruct.Id;
                                }
                                else
                                {
                                    var LastLvData = Filter_oEmpLvData.OrderByDescending(a => a.Id).FirstOrDefault();

                                    oLvClosingData = LastLvData.CloseBal;
                                    oLvOccurances = LastLvData.LvOccurances;
                                    UtilizedLv = LastLvData.LVCount;
                                    GeoStruct = LastLvData.GeoStruct.Id;
                                    PayStruct = LastLvData.PayStruct.Id;
                                    FuncStruct = LastLvData.FuncStruct.Id;
                                }
                                if (encashpolicy.EncashSpanYear != 0)
                                {
                                    var encid = db.SalaryHead.Include(e => e.Frequency).Include(e => e.SalHeadOperationType).Where(e => e.SalHeadOperationType.LookupVal.ToUpper() == "LVENCASH")
                                                            .FirstOrDefault();

                                    var LVENP = db.EmployeePayroll.Include(q => q.YearlyPaymentT.Select(a => a.SalaryHead)).Where(e => e.Employee.Id == j).SingleOrDefault();

                                    var dat = LVENP.YearlyPaymentT.Where(a => a.SalaryHead.Id == encid.Id).ToList();
                                    foreach (var item in dat)
                                    {
                                        if (item.ReleaseDate == null)
                                        {
                                            Msg.Add("Please Release Old enashment then apply for new. ");
                                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        }

                                    }

                                    EmployeeLeave CheckEncashSpan = db.EmployeeLeave.Include(t => t.LeaveEncashReq).Include(t => t.LeaveEncashReq.Select(e => e.LeaveCalendar))
                                                                .Include(t => t.LeaveEncashReq.Select(e => e.LvHead)).Where(t => t.Employee.Id == j).SingleOrDefault();

                                    if (CompCode == "KDCC")// Kolhapur DCC
                                    {
                                        var dat1 = CheckEncashSpan.LeaveEncashReq.Where(t => t.LvHead.Id == chk && t.TrReject != true && t.LeaveCalendar.Id == Cal.Id).ToList();
                                        if (dat1.Count() == encashpolicy.EncashSpanYear)
                                        {

                                            Msg.Add("Encashment Span Year yet to complete. " + encashpolicy.EncashSpanYear);
                                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                        }
                                    }
                                    else
                                    {
                                        //var dat1 = CheckEncashSpan.LeaveEncashReq.Where(t => t.LvHead.Id == chk && t.TrReject != true && t.LeaveCalendar.Id==Cal.Id).ToList();
                                        DateTime? lvcrdate = _Prv_EmpLvData.LvNewReq.Where(e => e.LeaveHead.Id == chk && e.LvCreditDate != null).Max(e => e.LvCreditDate.Value);
                                        if (lvcrdate != null)
                                        {
                                            Lvyearfrom = lvcrdate;
                                            LvyearTo = Lvyearfrom.Value.AddDays(-1);
                                            LvyearTo = LvyearTo.Value.AddYears(1);

                                            var dat1 = CheckEncashSpan.LeaveEncashReq.Where(t => t.LvHead.Id == chk && t.TrReject != true && t.LeaveCalendar.Id == Cal.Id && t.FromPeriod >= Lvyearfrom && t.ToPeriod <= LvyearTo).ToList();

                                            if (dat1.Count() == encashpolicy.EncashSpanYear)
                                            {
                                                //var test = dat1.ReleaseDate.Value.Year.ToString();
                                                //int oldrel = int.Parse(test);
                                                //DateTime datevalue = DateTime.Now;
                                                //var curryear = datevalue.Year.ToString();
                                                //int curr = int.Parse(curryear);
                                                //int ans = curr - oldrel;
                                                //if (ans != encashpolicy.EncashSpanYear)
                                                //{
                                                Msg.Add("Encashment Span Year yet to complete. " + encashpolicy.EncashSpanYear);
                                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                                //}
                                            }
                                        }

                                    }
                                }
                                else
                                {
                                    var encid = db.SalaryHead.Include(e => e.Frequency).Include(e => e.SalHeadOperationType).Where(e => e.SalHeadOperationType.LookupVal.ToUpper() == "LVENCASH")
                                                            .FirstOrDefault();

                                    var LVENP = db.EmployeePayroll.Include(q => q.YearlyPaymentT.Select(a => a.SalaryHead)).Where(e => e.Employee.Id == j).SingleOrDefault();

                                    var dat = LVENP.YearlyPaymentT.Where(a => a.SalaryHead.Id == encid.Id).ToList();
                                    foreach (var item in dat)
                                    {
                                        if (item.ReleaseDate == null)
                                        {
                                            Msg.Add("Please Release Old enashment then apply for new. ");
                                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        }

                                    }

                                }

                            }
                        }
                    }



                    if (L.ToPeriod.Value < L.FromPeriod.Value)
                    {
                        Msg.Add("To Period Should Be More than From Period ");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    if (CompCode != "KDCC")
                    {
                        if (Lvyearfrom != null && LvyearTo != null)
                        {
                            if (L.FromPeriod < Lvyearfrom)
                            {
                                Msg.Add("Encashment Period Should be between Leave Calender year ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                            if (L.ToPeriod > LvyearTo)
                            {
                                Msg.Add("Encashment Period Should be between Leave Calender year ");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            }
                        }
                    }
                    if (L.EncashDays < encashpolicy.MinEncashment)
                    {
                        //   Msg.Add(" Encash days should be more than  " + encashpolicy.MinEncashment);
                        return Json(new { status = false, responseText = " Encash days should be more than  " + encashpolicy.MinEncashment }, JsonRequestBehavior.AllowGet);
                        //   return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    if (L.EncashDays > encashpolicy.MaxEncashment)
                    {
                        return Json(new { status = false, responseText = " Encash days cannot more than  " + encashpolicy.MaxEncashment }, JsonRequestBehavior.AllowGet);
                        //Msg.Add(" Encash days cannot more than  " + encashpolicy.MaxEncashment);
                        //return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    if (encashpolicy.IsLvMultiple == true)
                    {
                        if (L.EncashDays % encashpolicy.LvMultiplier != 0)
                        {
                            Msg.Add("You can apply leave encashment multiplier of " + encashpolicy.LvMultiplier);
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }

                    if (encashpolicy.IsOnBalLv == true)
                    {
                        if (L.EncashDays != Math.Round((oLvClosingData * encashpolicy.LvBalPercent / 100) + 0.001, 0))
                        {
                            Msg.Add("you can apply Encash days balance leave of " + encashpolicy.LvBalPercent + " percent.");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }

                    if (UtilizedLv < encashpolicy.MinUtilized)
                    {
                        return Json(new { status = false, responseText = "For Encashment minimum utilization less than  " + encashpolicy.MinUtilized }, JsonRequestBehavior.AllowGet);
                        //Msg.Add("For Encashment minimum utilization less than  " + encashpolicy.MinUtilized);
                        //return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }
                    // NKGSB and KDCC bank leave encash min balance check after debit EncashDays start
                    string requiredPaths = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\EssPortal\\App_Data\\Menu_Json";
                    bool existss = System.IO.Directory.Exists(requiredPaths);
                    string localPaths;
                    if (!existss)
                    {
                        localPaths = new Uri(requiredPaths).LocalPath;
                        System.IO.Directory.CreateDirectory(localPaths);
                    }
                    string paths = requiredPaths + @"\LVENCASHMINBALAFTERDEBIT" + ".ini";
                    localPaths = new Uri(paths).LocalPath;
                    if (!System.IO.File.Exists(localPaths))
                    {

                        using (var fs = new FileStream(localPaths, FileMode.OpenOrCreate))
                        {
                            StreamWriter str = new StreamWriter(fs);
                            str.BaseStream.Seek(0, SeekOrigin.Begin);

                            str.Flush();
                            str.Close();
                            fs.Close();
                        }


                    }


                    string requiredPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase))) + "\\EssPortal\\App_Data\\Menu_Json";
                    bool exists = System.IO.Directory.Exists(requiredPath);
                    string localPath;
                    if (!exists)
                    {
                        localPath = new Uri(requiredPath).LocalPath;
                        System.IO.Directory.CreateDirectory(localPath);
                    }
                    string path = requiredPath + @"\LVENCASHMINBALAFTERDEBIT" + ".ini";
                    localPath = new Uri(path).LocalPath;
                    string compcodepara = "";
                    using (var streamReader = new StreamReader(localPath))
                    {
                        string line;

                        while ((line = streamReader.ReadLine()) != null)
                        {
                            var comp = line;
                            compcodepara = comp;
                        }
                    }


                    // NKGSB and KDCC bank leave encash min balance check after debit EncashDays start

                    //if (CompCode == "KDCC")
                    if (CompCode == compcodepara)
                    {
                        if ((oLvClosingData - L.EncashDays) < encashpolicy.MinBalance)
                        {
                            Msg.Add("For Encashment minimum balance require  " + encashpolicy.MinBalance);
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        if (oLvClosingData < encashpolicy.MinBalance)
                        {
                            Msg.Add("For Encashment minimum balance require  " + encashpolicy.MinBalance);
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    if (oLvClosingData < encashpolicy.MinBalance)
                    {
                        return Json(new { status = false, responseText = "For Encashment minimum balance require  " + encashpolicy.MinBalance }, JsonRequestBehavior.AllowGet);
                        //Msg.Add("For Encashment minimum balance require  " + encashpolicy.MinBalance);
                        //return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    if (oLvClosingData < L.EncashDays)
                    {
                        return Json(new { status = false, responseText = "You can not apply for encashment because your Leave Balance is  " + oLvClosingData }, JsonRequestBehavior.AllowGet);
                        //Msg.Add("You can not apply for encashment because your Leave Balance is  " + oLvClosingData);
                        //return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                    }

                    // Start Checking for policy end

                    if (LvNewReqlist != null && LvNewReqlist != "-Select-" && LvNewReqlist != "")
                    {
                        var value = db.LvNewReq.Find(int.Parse(LvNewReqlist));
                        L.LvNewReq = value;

                    }

                    //Bhagesh added code
                    //LvWFDetails oLvWFDetails = new LvWFDetails
                    //{
                    //    WFStatus = 0,
                    //    Comments = L.Narration,
                    //    DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false }
                    //};
                    //List<LvWFDetails> oLvWFDetails_List = new List<LvWFDetails>();
                    //oLvWFDetails_List.Add(oLvWFDetails);
                    //Bhagesh Code end

                    var Comp_Id = 0;
                    Comp_Id = Convert.ToInt32(Session["CompId"]);
                    var Z = db.CompanyPayroll.Where(e => e.Company.Id == Comp_Id).Include(e => e.EmployeePayroll.Select(f => f.Employee)).SingleOrDefault();
                    Employee OEmployee = null;
                    EmployeeLeave OEmployeePayroll = null;

                    L.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.EmpId, IsModified = false };
                    L.LeaveCalendar = db.Calendar.Where(e => e.Name.LookupVal.ToUpper() == "LEAVECALENDAR" && e.Default == true).SingleOrDefault();
                    LvEncashReq OBJLVWFD = new LvEncashReq()
                             {
                                 EncashDays = Encashreqday,// L.EncashDays,// Lvconert to other Leave start
                                 FromPeriod = L.FromPeriod,
                                 ToPeriod = L.ToPeriod,
                                 LvNewReq = L.LvNewReq,
                                 Narration = L.Narration,
                                 LvHead = L.LvHead,
                                 DBTrack = L.DBTrack,
                                 LeaveCalendar = L.LeaveCalendar,
                                 //  LvWFDetails = oLvWFDetails_List
                                 WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "0").FirstOrDefault(),//db.LookupValue.Where(e => e.LookupVal == "0").SingleOrDefault(),

                             };
                    if (ModelState.IsValid)
                    {
                        if (ids != null)
                        {
                            foreach (var i in ids)
                            {
                                OEmployee = db.Employee.Include(e => e.GeoStruct).Include(e => e.FuncStruct).Include(e => e.PayStruct)
                                            .Where(r => r.Id == i).SingleOrDefault();

                                OEmployeePayroll
                                = db.EmployeeLeave
                              .Where(e => e.Employee.Id == i).Include(e => e.LeaveEncashReq).SingleOrDefault();
                                OBJLVWFD.GeoStruct = db.GeoStruct.Find(OEmployee.GeoStruct.Id);
                                OBJLVWFD.FuncStruct = db.FuncStruct.Find(OEmployee.FuncStruct.Id);
                                OBJLVWFD.PayStruct = db.PayStruct.Find(OEmployee.PayStruct.Id);

                                using (TransactionScope ts = new TransactionScope())
                                {
                                    try
                                    {

                                        db.LvEncashReq.Add(OBJLVWFD);
                                        db.SaveChanges();
                                        List<LvEncashReq> OFAT = new List<LvEncashReq>();
                                        // OFAT.Add(db.LvEncashReq.Find(OBJLVWFD.Id));
                                        OFAT.Add(OBJLVWFD);
                                        if (OEmployeePayroll == null)
                                        {
                                            EmployeeLeave OTEP = new EmployeeLeave()
                                            {
                                                Employee = db.Employee.Find(OEmployee.Id),
                                                LeaveEncashReq = OFAT,
                                                DBTrack = L.DBTrack

                                            };


                                            db.EmployeeLeave.Add(OTEP);
                                            db.SaveChanges();
                                        }
                                        else
                                        {
                                            //var aa = db.EmployeeLeave.Find(OEmployeePayroll.Id);
                                            //aa.LeaveEncashReq = OFAT;
                                            ////OEmployeePayroll.DBTrack = dbt;
                                            //db.EmployeeLeave.Attach(aa);
                                            //db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                            //db.SaveChanges();
                                            //db.Entry(aa).State = System.Data.Entity.EntityState.Detached;

                                            if (OEmployeePayroll.LeaveEncashReq != null)
                                            {
                                                OFAT.AddRange(OEmployeePayroll.LeaveEncashReq);
                                            }
                                            OEmployeePayroll.LeaveEncashReq = OFAT;
                                            db.EmployeeLeave.Attach(OEmployeePayroll);
                                            db.Entry(OEmployeePayroll).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();
                                            db.Entry(OEmployeePayroll).State = System.Data.Entity.EntityState.Detached;

                                        }



                                        if (OEmployeePayroll != null)
                                        {
                                            var EmpID = Convert.ToInt32(Emp);
                                            var OEmployeeLv = db.EmployeeLeave
                                                .Include(e => e.LvNewReq)
                                                .Include(e => e.LvNewReq.Select(a => a.LeaveHead))
                                                .Include(e => e.LvOpenBal.Select(a => a.LvHead))
                                                .Include(e => e.LvNewReq.Select(a => a.LeaveCalendar))
                                                .Where(e => e.Employee.Id == EmpID)
                                                .FirstOrDefault();

                                           
                                            LvNewReq PrevReq = null;
                                            
                                            // if (L.LvNewReq != null)
                                            if (OEmployeeLv != null)
                                            {
                                                PrevReq = OEmployeeLv.LvNewReq
                                                .Where(e => e.LeaveHead.Id == chk)
                                                .OrderByDescending(e => e.Id).FirstOrDefault();
                                            }
                                            else
                                            {
                                                //PrevReq = OEmployeeLv.LvNewReq
                                                //              .Where(e => e.LeaveHead.Id == LEP.LvEncashReq.LvHead.Id && e.LeaveCalendar.Id == LEP.LvEncashReq.LeaveCalendar.Id)
                                                //    .OrderByDescending(e => e.Id).FirstOrDefault();
                                            }
                                            List<LvNewReq> Lvnewreqadd = new List<LvNewReq>();// Lvconert to other Leave start
                                            LvNewReq oLvNewReq = null;
                                            LvNewReq oLvNewReqConvert = null;
                                            if (PrevReq != null)
                                            {
                                                int id = PrevReq.Id;
                                                var LvNewReq = db.LvNewReq.Where(e => e.Id == id)
                                                    .Include(e => e.LeaveCalendar).Include(e => e.LeaveHead).Include(e => e.GeoStruct).Include(e => e.PayStruct).Include(e => e.FuncStruct)
                                                    .FirstOrDefault();

                                                //  L.LvNewReq = value;
                                                oLvNewReq = new LvNewReq()
                                                {
                                                    ReqDate = DateTime.Now,
                                                    InputMethod=1,//apply Through ess
                                                    CreditDays = 0,
                                                    FromDate = L.FromPeriod,
                                                    FromStat = LvNewReq.FromStat,
                                                    LeaveHead = LvNewReq.LeaveHead,
                                                    Reason = LvNewReq.Reason,
                                                    ResumeDate = LvNewReq.ResumeDate,
                                                    ToDate = L.ToPeriod,
                                                    ToStat = LvNewReq.ToStat,
                                                    LeaveCalendar = LvNewReq.LeaveCalendar,
                                                    DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false },
                                                    OpenBal = LvNewReq.CloseBal,
                                                    //  DebitDays = LEP.LvEncashReq.EncashDays,
                                                    DebitDays = L.EncashDays,
                                                    // CloseBal = LvNewReq.CloseBal - LEP.LvEncashReq.EncashDays,
                                                    CloseBal = LvNewReq.CloseBal - L.EncashDays,
                                                    LVCount = LvNewReq.LVCount + L.EncashDays,
                                                    LvOccurances = LvNewReq.LvOccurances,
                                                    TrClosed = true,
                                                    LvOrignal = null,
                                                    GeoStruct = LvNewReq.GeoStruct,
                                                    PayStruct = LvNewReq.PayStruct,
                                                    FuncStruct = LvNewReq.FuncStruct,
                                                    Narration = "Leave Encash Payment",
                                                    LvCountPrefixSuffix = LvNewReq.LvCountPrefixSuffix,
                                                    WFStatus = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "2").FirstOrDefault(),//db.LookupValue.Where(e => e.LookupVal == "2").SingleOrDefault(),
                                                };
                                                Lvnewreqadd.Add(oLvNewReq); // Lvconert to other Leave start
                                            }
                                            else
                                            {
                                                // var LvEncashReq = db.LvEncashReq.Include(e => e.LvHead).Include(e => e.LeaveCalendar).Where(e => e.Id == LEP.LvEncashReq.Id).SingleOrDefault();
                                                var OpenBalData = db.EmployeeLeave.Include(e => e.LvOpenBal)
                                                    .Include(e => e.LvOpenBal.Select(a => a.LvCalendar))
                                                    .Include(e => e.LvOpenBal.Select(a => a.LvHead))
                                                    .Include(e => e.Employee.GeoStruct)
                                                    .Include(e => e.Employee.PayStruct)
                                                    .Include(e => e.Employee.FuncStruct)
                                                    .Where(e => e.Employee.Id == EmpID && e.LvOpenBal.Count() > 0 && e.LvOpenBal.Any(a => a.LvHead.Id == chk && a.LvCalendar.Id == L.LeaveCalendar.Id))
                                                    .SingleOrDefault();
                                                var OpenBal = OpenBalData.LvOpenBal.Where(e => e.LvHead.Id == chk && e.LvCalendar.Id == L.LeaveCalendar.Id).SingleOrDefault();
                                                oLvNewReq = new LvNewReq()
                                                {
                                                    ReqDate = DateTime.Now,
                                                    CreditDays = 0,
                                                    InputMethod=1,//apply through ess
                                                    FromDate = L.FromPeriod,
                                                    FromStat = db.LookupValue.Where(e => e.LookupVal.ToUpper() == "FULLSESSION").SingleOrDefault(),
                                                    LeaveHead = L.LvHead,
                                                    Reason = L.Narration,
                                                    ResumeDate = L.ToPeriod.Value.AddDays(1),
                                                    ToDate = L.ToPeriod,
                                                    ToStat = db.LookupValue.Where(e => e.LookupVal.ToUpper() == "FULLSESSION").SingleOrDefault(),
                                                    LeaveCalendar = L.LeaveCalendar,
                                                    DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false },
                                                    OpenBal = OpenBal.LvClosing,
                                                    DebitDays = L.EncashDays,
                                                    CloseBal = OpenBal.LvClosing - L.EncashDays,
                                                    LVCount = L.EncashDays,
                                                    LvOccurances = 0,
                                                    TrClosed = true,
                                                    LvOrignal = null,
                                                    WFStatus =  db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal == "2").FirstOrDefault(),//db.LookupValue.Where(e => e.LookupVal == "2").SingleOrDefault(),

                                                    GeoStruct = OpenBalData.Employee.GeoStruct,
                                                    PayStruct = OpenBalData.Employee.PayStruct,
                                                    FuncStruct = OpenBalData.Employee.FuncStruct,
                                                    //IsCancel = true
                                                    Narration = "Leave Encash Payment"
                                                };
                                                Lvnewreqadd.Add(oLvNewReq); // Lvconert to other Leave start
                                            }
                                            // Lvconert to other Leave start
                                            if (Partiallvconvert == true)
                                            {
                                                var val = db.LvHead.Where(e => e.LvCode == convertlvhead).FirstOrDefault();
                                                int NxtCrdMonth = encreditpolicy.ProCreditFrequency;

                                                oLvNewReqConvert = new LvNewReq()
                                                {
                                                    LvCreditDate = Cal.FromDate,
                                                    LvCreditNextDate = Cal.FromDate.Value.AddMonths(NxtCrdMonth),
                                                    InputMethod = 0,
                                                    ReqDate = DateTime.Now,
                                                    CreditDays = Encashreqconvertday,
                                                    LeaveHead = db.LvHead.Where(e => e.LvCode == convertlvhead).FirstOrDefault(),
                                                    Reason = "Auto Converted Leave Encash For" + " : " + DateTime.Now.Date + " : " + L.LvHead.LvCode + " : " + "Credit Days" + " : " + Encashreqconvertday,
                                                    LeaveCalendar = db.Calendar.Include(e => e.Name).Where(e => e.Default == true &&
                                                    e.Name.LookupVal.ToUpper() == "LEAVECALENDAR").SingleOrDefault(),
                                                    DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false },
                                                    CloseBal = Encashreqconvertday,
                                                    TrClosed = true,
                                                    LvOrignal = null,
                                                    GeoStruct = db.GeoStruct.Find(OEmployee.GeoStruct.Id),
                                                    PayStruct = db.PayStruct.Find(OEmployee.PayStruct.Id),
                                                    FuncStruct = db.FuncStruct.Find(OEmployee.FuncStruct.Id),
                                                    Narration = "Converted Leave",
                                                    WFStatus = db.Lookup.Where(e => e.Code == "1000").FirstOrDefault().LookupValues.Where(e => e.LookupVal.ToUpper() == "3").FirstOrDefault(), // db.LookupValue.Where(e => e.LookupVal == "2").SingleOrDefault(),
                                                };
                                                Lvnewreqadd.Add(oLvNewReqConvert);
                                            }
                                            // Lvconert to other Leave end
                                            db.LvNewReq.AddRange(Lvnewreqadd); // Lvconert to other Leave start
                                            //db.LvNewReq.Add(oLvNewReq);
                                            db.SaveChanges();
                                            var aa = db.EmployeeLeave.Find(OEmployeeLv.Id);
                                            Lvnewreqadd.AddRange(aa.LvNewReq);
                                            aa.LvNewReq = Lvnewreqadd;
                                            db.EmployeeLeave.Attach(aa);

                                           // OEmployeeLv.LvNewReq.Add(oLvNewReq);
                                            db.Entry(aa).State = System.Data.Entity.EntityState.Modified;
                                            //db.Entry(OEmployeeLv).State = System.Data.Entity.EntityState.Modified;
                                            db.SaveChanges();
                                            db.Entry(aa).State = System.Data.Entity.EntityState.Detached;
                                          //  db.Entry(OEmployeeLv).State = System.Data.Entity.EntityState.Detached;

                                        }
                                        ts.Complete();
                                        return Json(new { status = true, responseText = "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);
                                        //Msg.Add("  Data Saved successfully  ");
                                        //return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                        //return this.Json(new Object[] { "", "", "Data Saved Successfully.", JsonRequestBehavior.AllowGet });
                                        //return Json(new { sucess = true, responseText = "Data Saved Successfully." }, JsonRequestBehavior.AllowGet);

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


                                }
                            }
                        }
                        Msg.Add(" Unable to create.Try again, and if the problem persists, see your system administrator");
                        return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                        //return this.Json(new Object[] { "", "", "Unable to create...", JsonRequestBehavior.AllowGet });
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
        //bhagesh Added Function 16072019
        public ActionResult GetLeaveReq(string data, string data2) //Pass leavehead id here
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                int LvHead = int.Parse(data);
                //int Emp = int.Parse(EmpId);
                //var LvCalendar = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "LEAVECALENDAR" && e.Default == true).SingleOrDefault();
                //var OEmployeeLeave = db.EmployeeLeave.Include(e => e.LvNewReq)
                //    .Include(e => e.LvNewReq.Select(a => a.LeaveHead))
                //    .Include(e => e.LvOpenBal.Select(a => a.LvHead))
                //    .Include(e => e.LvNewReq.Select(a => a.LeaveCalendar))
                //    .Where(e => e.Employee.Id == Emp).SingleOrDefault();

                // var PrevReq = OEmployeeLeave.LvNewReq.Where(e => e.LeaveHead.Id == LvHead && e.LeaveCalendar.Id == LvCalendar.Id
                var leaveReq = db.LvEncashPolicy.Where(e => e.LvHead.Id == LvHead && e.IsLvRequestAppl == true
                    //&& e.IsCancel == false
                ).FirstOrDefault();
                if (leaveReq != null)
                {

                    return Json(new { sucess = true, data = leaveReq }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { sucess = false }, JsonRequestBehavior.AllowGet);
                }


            }
        }

        public ActionResult getCalendar()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var qurey = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "LEAVECALENDAR" && e.Default == true).AsEnumerable()
                    .Select(e => new { Lvcalendardesc = "From Date :" + e.FromDate.Value.ToShortDateString() + " To Date :" + e.ToDate.Value.ToShortDateString(),
                        Fromperiod=e.FromDate.Value.ToShortDateString(),
                        Toperiod=e.ToDate.Value.ToShortDateString() }).SingleOrDefault();
                return Json(qurey, JsonRequestBehavior.AllowGet);
            }
        }
        //[HttpPost]
        //public ActionResult Create(LvEncashReq L, FormCollection form)
        //{
        //    //var WFStatuslist = form["WFStatuslist"] == "0" ? "" : form["WFStatuslist"];
        //    var LvNewReqlist = form["LvNewReqlist"] == "0" ? "" : form["LvNewReqlist"];
        //    var EmployeeLv = new EmployeeLeave();
        //    var Comp_Id = 0;
        //    Comp_Id = Convert.ToInt32(Session["CompId"]);
        //    var Z = db.CompanyPayroll.Where(e => e.Company.Id == Comp_Id).Include(e => e.EmployeePayroll.Select(f => f.Employee)).SingleOrDefault();



        //    if (LvNewReqlist != null && LvNewReqlist != "-Select-")
        //    {
        //        var value = db.LvNewReq.Find(int.Parse(LvNewReqlist));
        //        L.LvNewReq = value;

        //    }

        //    if (ModelState.IsValid)
        //    {
        //        using (TransactionScope ts = new TransactionScope())
        //        {


        //            L.DBTrack = new DBTrack { Action = "C", CreatedBy = SessionManager.UserName, IsModified = false };

        //            LvEncashReq OBJLVWFD = new LvEncashReq()
        //            {
        //                EncashDays  = L.EncashDays,
        //                FromPeriod = L.FromPeriod,
        //                ToPeriod = L.ToPeriod,
        //                LvNewReq = L.LvNewReq,
        //                Narration = L.Narration,
        //                DBTrack = L.DBTrack
        //            };
        //            try
        //            {
        //                db.LvEncashReq.Add(OBJLVWFD);
        //                var rtn_Obj = DBTrackFile.DBTrackSave("Leave/Leave", null, db.ChangeTracker, L.DBTrack);
        //                DT_LvEncashReq DT_OBJ = (DT_LvEncashReq)rtn_Obj;
        //                db.Create(DT_OBJ);
        //                db.SaveChanges();
        //                foreach (var ID in Z.EmployeePayroll.Select(x => x.Employee.Id))
        //                {
        //                    EmployeeLv = db.EmployeeLeave.Include(e => e.LeaveEncashReq).Where(e => e.Employee.Id == ID).SingleOrDefault();
        //                    List<LvEncashReq> Objlvenchasreq = new List<LvEncashReq>();
        //                    Objlvenchasreq.Add(db.LvEncashReq.Find(OBJLVWFD.Id));
        //                    if (EmployeeLv != null) 
        //                    {
        //                        EmployeeLv.LeaveEncashReq = Objlvenchasreq;
        //                        db.Entry(EmployeeLv).State = System.Data.Entity.EntityState.Modified;
        //                        db.SaveChanges();
        //                        db.Entry(EmployeeLv).State = System.Data.Entity.EntityState.Detached;

        //                    }
        //                    ts.Complete();    
        //                }

        //               Msg.Add("  Data Saved successfully  ");return Json(new Utility.JsonReturnClass {  success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
        //            }

        //            catch (DbUpdateConcurrencyException)
        //            {
        //                return RedirectToAction("Create", new { concurrencyError = true, id = L.Id });
        //            }
        //            catch (DataException /* dex */)
        //            {
        //                return this.Json(new { msg = "Unable to create. Try again, and if the problem persists contact your system administrator..", JsonRequestBehavior.AllowGet });
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

        #endregion


        #region EDIT & EDITSAVE

        public class LvWfdetailsEdit
        {
            public Array LvWFDETAILS_Id { get; set; }

            public Array LvWFDETAILS_FullDetails { get; set; }
        }

        [HttpPost]
        public ActionResult Edit(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                var Q = db.LvEncashReq
                    .Include(e => e.LvNewReq)

                    .Where(e => e.Id == data).Select
                    (e => new
                    {
                        EncashDays = e.EncashDays,
                        FromPeriod = e.FromPeriod,
                        ToPeriod = e.ToPeriod,

                        LvNewReq_Id = e.LvNewReq.Id == null ? 0 : e.LvNewReq.Id,
                        LvNewReq_FullDetails = e.LvNewReq.FullDetails,
                        Narration = e.Narration,
                        Action = e.DBTrack.Action
                    }).ToList();


                var W = db.DT_LvEncashReq
                     .Where(e => e.Orig_Id == data && e.DBTrack.IsModified == true && e.DBTrack.Action == "M").Select
                     (e => new
                     {
                         DT_Id = e.Id,
                         EncashDays = e.EncashDays == null ? 0 : e.EncashDays,
                         WFStatus_Val = e.WFStatus_Id == 0 ? "" : db.LookupValue
                                    .Where(x => x.Id == e.WFStatus_Id)
                                    .Select(x => x.LookupVal).FirstOrDefault(),

                         LvNewReq_Val = e.LvNewReq_Id == 0 ? "" : db.LvNewReq.Where(x => x.Id == e.LvNewReq_Id).Select(x => x.FullDetails).FirstOrDefault(),
                     }).OrderByDescending(e => e.DT_Id).FirstOrDefault();

                var Corp = db.LvEncashReq.Find(data);
                TempData["RowVersion"] = Corp.RowVersion;
                var Auth = Corp.DBTrack.IsModified;
                return Json(new Object[] { Q, "", W, Auth, JsonRequestBehavior.AllowGet });
            }
        }

        [HttpPost]
        public async Task<ActionResult> EditSave(LvEncashReq L, int data, FormCollection form) // Edit submit
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    string LvNewReqlist = form["LvNewReqlist"] == "0" ? "" : form["LvNewReqlist"];

                    //  bool Auth = form["autho_action"] == "" ? false : true;
                    bool Auth = form["autho_allow"] == "true" ? true : false;


                    if (LvNewReqlist != null && LvNewReqlist != "")
                    {
                        var value = db.LvNewReq.Find(int.Parse(LvNewReqlist));
                        L.LvNewReq = value;
                    }

                    List<LvWFDetails> OBJLVWFDETAILS = new List<LvWFDetails>();

                    if (Auth == false)
                    {


                        if (ModelState.IsValid)
                        {
                            try
                            {

                                //DbContextTransaction transaction = db.Database.BeginTransaction();

                                using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    LvEncashReq blog = null; // to retrieve old data
                                    DbPropertyValues originalBlogValues = null;

                                    using (var context = new DataBaseContext())
                                    {
                                        blog = context.LvEncashReq.Where(e => e.Id == data).Include(e => e.LvNewReq)
                                                                .Include(e => e.LvWFDetails)
                                                                .Include(e => e.WFStatus).SingleOrDefault();
                                        originalBlogValues = context.Entry(blog).OriginalValues;
                                    }

                                    L.DBTrack = new DBTrack
                                    {
                                        CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                        CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                        Action = "M",
                                        ModifiedBy = SessionManager.UserName,
                                        ModifiedOn = DateTime.Now
                                    };

                                    int a = EditS(LvNewReqlist, data, L, L.DBTrack);



                                    using (var context = new DataBaseContext())
                                    {
                                        var obj = DBTrackFile.DBTrackSave("Leave/Leave", originalBlogValues, db.ChangeTracker, L.DBTrack);
                                        DT_LvEncashReq DT_Corp = (DT_LvEncashReq)obj;
                                        DT_Corp.LvNewReq_Id = blog.LvNewReq == null ? 0 : blog.LvNewReq.Id;
                                        DT_Corp.WFStatus_Id = blog.WFStatus == null ? 0 : blog.WFStatus.Id;
                                        db.Create(DT_Corp);
                                        db.SaveChanges();
                                    }
                                    await db.SaveChangesAsync();
                                    ts.Complete();

                                    Msg.Add("  Record Updated");
                                    return Json(new Utility.JsonReturnClass { Id = L.Id, Val = L.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return Json(new Object[] { L.Id, L.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });

                                }
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                var entry = ex.Entries.Single();
                                var clientValues = (Corporate)entry.Entity;
                                var databaseEntry = entry.GetDatabaseValues();
                                if (databaseEntry == null)
                                {
                                    Msg.Add(" Unable to save changes. The record was deleted by another user. ");
                                    return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                    //return Json(new Object[] { "", "", "Unable to save changes. The record was deleted by another user.", JsonRequestBehavior.AllowGet });
                                }
                                else
                                {
                                    var databaseValues = (LvEncashReq)databaseEntry.ToObject();
                                    L.RowVersion = databaseValues.RowVersion;

                                }
                            }
                            Msg.Add("Record modified by another user.So refresh it and try to save again.");
                            return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { "", "", "Record modified by another user.So refresh it and try to save again.", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {

                            LvEncashReq blog = null; // to retrieve old data
                            DbPropertyValues originalBlogValues = null;
                            LvEncashReq Old_Corp = null;

                            using (var context = new DataBaseContext())
                            {
                                blog = context.LvEncashReq.Where(e => e.Id == data).SingleOrDefault();
                                originalBlogValues = context.Entry(blog).OriginalValues;
                            }
                            L.DBTrack = new DBTrack
                            {
                                CreatedBy = blog.DBTrack.CreatedBy == null ? null : blog.DBTrack.CreatedBy,
                                CreatedOn = blog.DBTrack.CreatedOn == null ? null : blog.DBTrack.CreatedOn,
                                Action = "M",
                                IsModified = blog.DBTrack.IsModified == true ? true : false,
                                ModifiedBy = SessionManager.UserName,
                                ModifiedOn = DateTime.Now
                            };

                            if (TempData["RowVersion"] == null)
                            {
                                TempData["RowVersion"] = blog.RowVersion;
                            }

                            LvEncashReq corp = new LvEncashReq()
                            {
                                EncashDays = L.EncashDays,
                                FromPeriod = L.FromPeriod,
                                Narration = L.Narration,
                                Id = data,
                                DBTrack = L.DBTrack,
                                RowVersion = (Byte[])TempData["RowVersion"]
                            };


                            //db.Entry(c).OriginalValues["RowVersion"] = TempData["RowVersion"];

                            using (var context = new DataBaseContext())
                            {
                                var obj = DBTrackFile.ModifiedDataHistory("Leave/Leave", "M", blog, corp, "LvEncashReq", L.DBTrack);
                                // var obj = DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);

                                Old_Corp = context.LvEncashReq.Where(e => e.Id == data).Include(e => e.LvNewReq)
                                    .Include(e => e.LvWFDetails).Include(e => e.WFStatus).SingleOrDefault();
                                DT_LvEncashReq DT_Corp = (DT_LvEncashReq)obj;
                                DT_Corp.LvNewReq_Id = blog.LvNewReq == null ? 0 : blog.LvNewReq.Id;
                                DT_Corp.WFStatus_Id = blog.WFStatus == null ? 0 : blog.WFStatus.Id;
                                db.Create(DT_Corp);
                                //db.SaveChanges();
                            }
                            blog.DBTrack = L.DBTrack;
                            db.LvEncashReq.Attach(blog);
                            db.Entry(blog).State = System.Data.Entity.EntityState.Modified;
                            db.Entry(blog).OriginalValues["RowVersion"] = TempData["RowVersion"];
                            db.SaveChanges();
                            ts.Complete();
                            Msg.Add("  Record Updated");
                            return Json(new Utility.JsonReturnClass { Id = blog.Id, Val = L.FullDetails, success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);
                            //return Json(new Object[] { blog.Id, L.FullDetails, "Record Updated", JsonRequestBehavior.AllowGet });
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

        public int EditS(string LvNewReqlist, int data, LvEncashReq c, DBTrack dbT)
        {
            using (DataBaseContext db = new DataBaseContext())
            {

                if (LvNewReqlist != null)
                {
                    if (LvNewReqlist != "")
                    {
                        var val = db.LvNewReq.Find(int.Parse(LvNewReqlist));
                        c.LvNewReq = val;

                        var add = db.LvEncashReq.Include(e => e.LvNewReq).Where(e => e.Id == data).SingleOrDefault();
                        IList<LvEncashReq> LvNewReqdetails = null;
                        if (add.LvNewReq != null)
                        {
                            LvNewReqdetails = db.LvEncashReq.Where(x => x.LvNewReq.Id == add.LvNewReq.Id && x.Id == data).ToList();
                        }
                        else
                        {
                            LvNewReqdetails = db.LvEncashReq.Where(x => x.Id == data).ToList();
                        }
                        if (LvNewReqdetails != null)
                        {
                            foreach (var s in LvNewReqdetails)
                            {
                                s.LvNewReq = c.LvNewReq;
                                db.LvEncashReq.Attach(s);
                                db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                                // await db.SaveChangesAsync(false);
                                db.SaveChanges();
                                TempData["RowVersion"] = s.RowVersion;
                                db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                            }
                        }
                    }
                }
                else
                {
                    var LvNewReqdetails = db.LvEncashReq.Include(e => e.LvNewReq).Where(x => x.Id == data).ToList();
                    foreach (var s in LvNewReqdetails)
                    {
                        s.LvNewReq = null;
                        db.LvEncashReq.Attach(s);
                        db.Entry(s).State = System.Data.Entity.EntityState.Modified;
                        //await db.SaveChangesAsync();
                        db.SaveChanges();
                        TempData["RowVersion"] = s.RowVersion;
                        db.Entry(s).State = System.Data.Entity.EntityState.Detached;
                    }
                }


                var CurCorp = db.LvEncashReq.Find(data);
                TempData["CurrRowVersion"] = CurCorp.RowVersion;
                db.Entry(CurCorp).State = System.Data.Entity.EntityState.Detached;
                if (DBTrackFile.ByteArrayCompare((byte[])TempData["CurrRowVersion"], (byte[])TempData["RowVersion"]) == true)
                {
                    c.DBTrack = dbT;
                    LvEncashReq lvencashreq = new LvEncashReq()
                    {
                        EncashDays = c.EncashDays,
                        FromPeriod = c.FromPeriod,
                        ToPeriod = c.ToPeriod,
                        Narration = c.Narration,
                        Id = data,
                        DBTrack = c.DBTrack
                    };
                    db.LvEncashReq.Attach(lvencashreq);
                    db.Entry(lvencashreq).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(lvencashreq).OriginalValues["RowVersion"] = TempData["RowVersion"];
                    //// DBTrackFile.DBTrackSave("Core/P2b.Global", originalBlogValues, db.ChangeTracker, c.DBTrack);
                    return 1;
                }
                return 0;
            }
        }
        #endregion


        #region P2BGRIDDETAILS
        public class P2BGridData
        {
            public int Id { get; set; }
            public double EncashDays { get; set; }
            public DateTime? FromPeriod { get; set; }
            public DateTime? ToPeriod { get; set; }
            public string Narration { get; set; }
        }



        #endregion


        #region AUTHORIZATION
        #endregion


        #region DELETE

        [HttpPost]
        public async Task<ActionResult> Delete(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                List<string> Msg = new List<string>();
                try
                {
                    LvEncashReq OBJLvEncashReq = db.LvEncashReq.Include(e => e.LvNewReq)
                                                       .Include(e => e.LvWFDetails)
                                                       .Include(e => e.WFStatus).Where(e => e.Id == data).SingleOrDefault();

                    //Corporate corporates = db.Corporate.Where(e => e.Id == data).SingleOrDefault();
                    if (OBJLvEncashReq.DBTrack.IsModified == true)
                    {
                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, corporates.DBTrack, corporates, null, "Corporate");
                            DBTrack dbT = new DBTrack
                            {
                                Action = "D",
                                CreatedBy = OBJLvEncashReq.DBTrack.CreatedBy != null ? OBJLvEncashReq.DBTrack.CreatedBy : null,
                                CreatedOn = OBJLvEncashReq.DBTrack.CreatedOn != null ? OBJLvEncashReq.DBTrack.CreatedOn : null,
                                IsModified = OBJLvEncashReq.DBTrack.IsModified == true ? true : false
                            };
                            OBJLvEncashReq.DBTrack = dbT;
                            db.Entry(OBJLvEncashReq).State = System.Data.Entity.EntityState.Modified;
                            var rtn_Obj = DBTrackFile.DBTrackSave("Leave/Leave", null, db.ChangeTracker, OBJLvEncashReq.DBTrack);
                            DT_LvEncashReq DT_Corp = (DT_LvEncashReq)rtn_Obj;
                            DT_Corp.LvNewReq_Id = OBJLvEncashReq.LvNewReq == null ? 0 : OBJLvEncashReq.LvNewReq.Id;
                            DT_Corp.WFStatus_Id = OBJLvEncashReq.WFStatus == null ? 0 : OBJLvEncashReq.WFStatus.Id;
                            db.Create(DT_Corp);
                            // db.SaveChanges();
                            //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack);
                            await db.SaveChangesAsync();
                            //using (var context = new DataBaseContext())
                            //{
                            //   // DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", corporates.DBTrack );
                            //}
                            ts.Complete();
                            Msg.Add("  Data removed successfully.  ");
                            return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                            //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });
                        }
                    }
                    else
                    {
                        var selectedRegions = OBJLvEncashReq.LvWFDetails;

                        using (TransactionScope ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            try
                            {
                                DBTrack dbT = new DBTrack
                                {
                                    Action = "D",
                                    ModifiedBy = SessionManager.UserName,
                                    ModifiedOn = DateTime.Now,
                                    CreatedBy = OBJLvEncashReq.DBTrack.CreatedBy != null ? OBJLvEncashReq.DBTrack.CreatedBy : null,
                                    CreatedOn = OBJLvEncashReq.DBTrack.CreatedOn != null ? OBJLvEncashReq.DBTrack.CreatedOn : null,
                                    IsModified = OBJLvEncashReq.DBTrack.IsModified == true ? false : false//,
                                    //AuthorizedBy = SessionManager.EmpId,
                                    //AuthorizedOn = DateTime.Now
                                };

                                // DBTrack dbT = new DBTrack { Action = "D", ModifiedBy = SessionManager.EmpId, ModifiedOn = DateTime.Now };

                                db.Entry(OBJLvEncashReq).State = System.Data.Entity.EntityState.Deleted;
                                var rtn_Obj = DBTrackFile.DBTrackSave("Leave/Leave", null, db.ChangeTracker, OBJLvEncashReq.DBTrack);
                                DT_LvEncashReq DT_Corp = (DT_LvEncashReq)rtn_Obj;
                                DT_Corp.LvNewReq_Id = OBJLvEncashReq.LvNewReq == null ? 0 : OBJLvEncashReq.LvNewReq.Id;
                                DT_Corp.WFStatus_Id = OBJLvEncashReq.WFStatus == null ? 0 : OBJLvEncashReq.WFStatus.Id;
                                db.Create(DT_Corp);

                                await db.SaveChangesAsync();


                                //using (var context = new DataBaseContext())
                                //{
                                //    corporates.Address = add;
                                //    corporates.ContactDetails = conDet;
                                //    corporates.BusinessType = val;
                                //    DBTrackFile.DBTrackSave("Core/P2b.Global", null, db.ChangeTracker, dbT);
                                //    //DBTrackFile.DBTrackSave("Core/P2b.Global", "D", corporates, null, "Corporate", dbT);
                                //}
                                ts.Complete();
                                Msg.Add("  Data removed successfully.  ");
                                return Json(new Utility.JsonReturnClass { success = true, responseText = Msg }, JsonRequestBehavior.AllowGet);

                                //return Json(new Object[] { "", "Data removed.", JsonRequestBehavior.AllowGet });

                            }
                            catch (RetryLimitExceededException /* dex */)
                            {
                                //Log the error (uncomment dex variable name and add a line here to write a log.)
                                //ModelState.AddModelError("", "Unable to delete. Try again, and if the problem persists, see your system administrator.");
                                //return RedirectToAction("Delete");
                                Msg.Add(" Unable to delete. Try again, and if the problem persists, see your system administrator");
                                return Json(new Utility.JsonReturnClass { success = false, responseText = Msg }, JsonRequestBehavior.AllowGet);
                                // return this.Json(new { msg = "Unable to delete. Try again, and if the problem persists, see your system administrator." });
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

        #endregion

        public class LvEncashReqChildDataClass
        {
            public int Id { get; set; }
            public string LvNewReq { get; set; }
            public string FromPeriod { get; set; }
            public string ToPeriod { get; set; }
            public double EncashDays { get; set; }
            public string Narration { get; set; }

        }
        public class GetLvNewReqClass2
        {
            public string LvNewReq { get; set; }
            public string FromPeriod { get; set; }
            public string ToPeriod { get; set; }
            public string EncashDays { get; set; }
            public string Status { get; set; }

            public ChildGetLvNewReqClass2 RowData { get; set; }
        }

        public class ChildGetLvNewReqClass2
        {
            public string LvNewReq { get; set; }
            public string EmpLVid { get; set; }
            public string IsClose { get; set; }
            public string LvHead_Id { get; set; }

        }

        public ActionResult GetLvEnCashReq()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                if (string.IsNullOrEmpty(Convert.ToString(SessionManager.EmpLvId)))
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                var Id = Convert.ToInt32(SessionManager.EmpLvId);
                var ComplvId = Convert.ToInt32(SessionManager.CompLvId);

                var db_data = db.EmployeeLeave
                        .Include(e => e.LeaveEncashReq.Select(t => t.LvNewReq))
                        .Include(e => e.LeaveEncashReq.Select(t => t.WFStatus))
                    //  .Include(e => e.LeaveEncashReq.Select(t => t.LvNewReq.InputMethod))
                        .Include(e => e.LeaveEncashReq.Select(t => t.LvNewReq.LeaveHead))
                        .Include(e => e.LeaveEncashReq)
                        .Where(e => e.Employee.Id == Id).SingleOrDefault();


                List<GetLvNewReqClass2> ListreturnLvnewClass = new List<GetLvNewReqClass2>();
                //    List<EmployeeLeave> LvList = new List<EmployeeLeave>();
                if (db_data != null)
                {


                    ListreturnLvnewClass.Add(new GetLvNewReqClass2
                    {
                        LvNewReq = "LvNewReq",
                        FromPeriod = "From Period",
                        ToPeriod = "To Period",
                        EncashDays = "Encash Days",
                        Status = "Status"
                    });
                    foreach (var item in db_data.LeaveEncashReq)
                    {

                        //var query = db.EmployeeLeave
                        //  .Select(e => new
                        //    {
                        //        LvNewReq = e.LvNewReq.Where(a => a.Id == item),
                        //        EmpLVid = e.Id,
                        //        IsClose = e.Employee.ReportingStructRights
                        //        .Where(a => a.AccessRights != null && a.AccessRights.ActionName.LookupVal.ToUpper() == session)
                        //        .Select(a => a.AccessRights.IsClose).FirstOrDefault(),
                        //        EmpId = e.Employee.Id,
                        //        EmpName = e.Employee.EmpName.FullNameFML,
                        //        EmpCode = e.Employee.EmpCode
                        //    }).SingleOrDefault();
                        //var LvReq = query.LvNewReq.Where(a => a.Id == item).FirstOrDefault();

                        // var FromDate = item.FromDate != null ? item.FromDate.Value.ToShortDateString() : null;
                        // var ToDate = item.ToDate != null ? item.ToDate.Value.ToShortDateString() : null;
                        var Status = "--";
                        if (item.WFStatus != null)
                        {
                            Status = Utility.GetStatusName().Where(e => e.Key == item.WFStatus.LookupVal.ToString())
                            .Select(e => e.Value).SingleOrDefault();
                        }
                        else
                        {
                            Status = "Approved By HRM (M)";
                        }


                        ListreturnLvnewClass.Add(new GetLvNewReqClass2
                        {
                            RowData = new ChildGetLvNewReqClass2
                            {
                                LvNewReq = item.Id.ToString(),
                                EmpLVid = db_data.Id.ToString(),
                                IsClose = "",
                                LvHead_Id = item.LvNewReq != null ? item.LvNewReq.LeaveHead.Id.ToString() : "",
                            },

                            LvNewReq = item.LvNewReq != null ? item.LvNewReq.FullDetails : "",
                            FromPeriod = item.FromPeriod != null ? item.FromPeriod.Value.ToString("dd/MM/yyyy") : null,
                            ToPeriod = item.ToPeriod != null ? item.ToPeriod.Value.ToString("dd/MM/yyyy") : null,
                            EncashDays = item.EncashDays.ToString(),
                            Status = Status

                        });
                    }
                    if (ListreturnLvnewClass != null && ListreturnLvnewClass.Count > 0)
                    {
                        return Json(new { status = true, data = ListreturnLvnewClass, responseText = "Okk" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { status = false, data = ListreturnLvnewClass, responseText = "Error" }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
            }
            return null;
        }
        public class GetLvNewReqClass3
        {
            public string Emp { get; set; }
            public string Leavehead { get; set; }
            public string FromPeriod { get; set; }
            public string ToPeriod { get; set; }
            public string EncashDays { get; set; }

            public ChildGetLvNewReqClass3 RowData { get; set; }
        }
        public class ChildGetLvNewReqClass3
        {
            public string LvNewReq { get; set; }
            public string EmpLVid { get; set; }
            public string IsClose { get; set; }
            public string LvHead_Id { get; set; }

        }

        public ActionResult GetLvNewEncashReq()
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                LookupValue FuncModule = new LookupValue();
                if (!string.IsNullOrEmpty(Convert.ToString(Session["user-module"])))
                {
                    var id = Convert.ToString(Session["user-module"]);
                    var Lk = db.Lookup.Include(e => e.LookupValues).Where(e => e.Code == "601").SingleOrDefault();
                    FuncModule = Lk.LookupValues.Where(e => e.LookupVal.ToUpper() == id.ToUpper()).SingleOrDefault();
                }
                if (FuncModule == null)
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                var EmpIds = UserManager.GeEmployeeList(FuncModule.Id, 0);
                if (EmpIds == null && EmpIds.Count == 0)
                {
                    return Json(new { status = false }, JsonRequestBehavior.AllowGet);
                }
                List<GetLvNewReqClass3> ListreturnLvnewClass = new List<GetLvNewReqClass3>();
                List<EmployeeLeave> LvList = new List<EmployeeLeave>();
                foreach (var item in EmpIds)
                {
                    var temp = db.EmployeeLeave
                  .Include(e => e.Employee)
                   .Include(e => e.Employee.EmpName)
                   .Include(e => e.LeaveEncashReq)
                   .Include(e => e.LeaveEncashReq.Select(a => a.LvHead))
                   .Include(e => e.LvOpenBal.Select(a => a.LvHead))
                   .Include(e => e.LeaveEncashReq.Select(a => a.LeaveCalendar))
                        //.Include(e => e.LvNewReq.Select(a => a.WFStatus))
                   .Include(e => e.LeaveEncashReq.Select(a => a.LvWFDetails))
                   .Where(e => e.Employee.Id == item).FirstOrDefault();
                    if (temp != null)
                    {
                        LvList.Add(temp);
                    }
                }
                var LvIds = UserManager.FilterLvencash(LvList.SelectMany(e => e.LeaveEncashReq).ToList(),
                    Convert.ToString(Session["auho"]), Convert.ToInt32(SessionManager.CompanyId));

                var session = Session["auho"].ToString().ToUpper();
                ListreturnLvnewClass.Add(new GetLvNewReqClass3
                {
                    Emp = "Employee",
                    Leavehead = "Leave Head",
                    FromPeriod = "From Period",
                    ToPeriod = "To Period",
                    EncashDays = "Encash Days",

                });
                foreach (var item in LvIds)
                {

                    var query = db.EmployeeLeave
                       .Where(e => e.LeaveEncashReq.Any(a => a.Id == item))
                        .Select(e => new
                        {
                            Lvencashreq = e.LeaveEncashReq.Where(a => a.Id == item),
                            EmpLVid = e.Id,
                            IsClose = e.Employee.ReportingStructRights
                            .Where(a => a.AccessRights != null && a.AccessRights.ActionName.LookupVal.ToUpper() == session)
                            .Select(a => a.AccessRights.IsClose).FirstOrDefault(),
                            EmpId = e.Employee.Id,
                            EmpName = e.Employee.EmpName.FullNameFML,
                            EmpCode = e.Employee.EmpCode
                        }).SingleOrDefault();
                    var LvEncashReq = query.Lvencashreq.Where(a => a.Id == item).FirstOrDefault();

                    ListreturnLvnewClass.Add(new GetLvNewReqClass3
                    {
                        RowData = new ChildGetLvNewReqClass3
                        {
                            LvNewReq = LvEncashReq.Id.ToString(),
                            EmpLVid = query.EmpLVid.ToString(),
                            IsClose = query.IsClose.ToString(),
                            LvHead_Id = query.Lvencashreq.Select(e => e.LvHead.Id.ToString()).FirstOrDefault(),
                        },
                        Emp = query.EmpCode + " " + query.EmpName,
                        Leavehead = LvEncashReq.LvHead.LvName,
                        FromPeriod = LvEncashReq.FromPeriod.Value.ToShortDateString(),
                        ToPeriod = LvEncashReq.ToPeriod.Value.ToShortDateString(),
                        EncashDays = LvEncashReq.EncashDays.ToString(),

                    });
                }

                if (ListreturnLvnewClass != null && ListreturnLvnewClass.Count > 0)
                {
                    return Json(new { status = true, data = ListreturnLvnewClass, responseText = "Okk" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = false, data = ListreturnLvnewClass, responseText = "Error" }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        public class returndatagridclass
        {
            public string Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
            public string JoiningDate { get; set; }
            public string Job { get; set; }
            public string Grade { get; set; }
            public string Location { get; set; }
        }

        public JsonResult EditGridDetails(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var Q = db.LvEncashReq
                    .Include(e => e.LeaveCalendar)
                    .Include(e => e.LvNewReq)
                    .Include(e => e.LvNewReq.LeaveHead)
                     .Where(e => e.Id == data).AsEnumerable().Select
                     (e => new
                     {
                         LvNewReq_Id = e.LvNewReq != null ? e.LvNewReq.Id : 0,
                         LvNewReq = e.LvNewReq != null ? e.LvNewReq.FullDetails : null,
                         FromPeriod = e.FromPeriod.Value.ToShortDateString(),
                         ToPeriod = e.ToPeriod.Value.ToShortDateString(),
                         EncashDays = e.EncashDays,
                         Narration = e.Narration,
                         Action = e.DBTrack.Action
                     }).ToList();
                var LvEncashReq = db.LvEncashReq.Find(data);
                Session["RowVersion"] = LvEncashReq.RowVersion;
                var Auth = LvEncashReq.DBTrack.IsModified;
                //return Json(new Object[] { Q, add_data, "", Auth, JsonRequestBehavior.AllowGet });
                return Json(new { data = Q }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GridEditSave(LvEncashReq lvencash, FormCollection form, string data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                var EncashDays = form["Grid-EncashDays"] == " 0" ? "" : form["Grid-EncashDays"];
                var Narr = form["Grid-Narration"] == "0" ? "" : form["Grid-Narration"];
                lvencash.EncashDays = Convert.ToDouble(EncashDays);
                lvencash.Narration = Narr.ToString();
                if (data != null)
                {
                    var id = Convert.ToInt32(data);
                    var db_data = db.LvEncashReq.Where(e => e.Id == id).SingleOrDefault();
                    db_data.EncashDays = lvencash.EncashDays;
                    db_data.Narration = lvencash.Narration;
                    try
                    {
                        db.LvEncashReq.Attach(db_data);
                        db.Entry(db_data).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        db.Entry(db_data).State = System.Data.Entity.EntityState.Detached;
                        return Json(new { status = true, data = db_data, responseText = "Record Updated..!" }, JsonRequestBehavior.AllowGet);
                    }
                    catch (Exception e)
                    {

                        throw e;
                    }
                }
                else
                {
                    return Json(new { status = false, responseText = "Data Is Null" }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        //Get_LvEncashReq
        public ActionResult Get_LvEncashReq(int data)
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                try
                {
                    var db_data = db.EmployeeLeave
                        .Include(e => e.LeaveEncashReq.Select(t => t.LvNewReq))
                        .Include(e => e.LeaveEncashReq.Select(t => t.LvNewReq.LeaveHead))
                        .Include(e => e.LeaveEncashReq)
                        .Where(e => e.Id == data).SingleOrDefault();
                    if (db_data != null)
                    {
                        List<LvEncashReqChildDataClass> returndata = new List<LvEncashReqChildDataClass>();
                        foreach (var item in db_data.LeaveEncashReq)
                        {

                            returndata.Add(new LvEncashReqChildDataClass
                            {
                                Id = item.Id,
                                LvNewReq = item.LvNewReq != null ? item.LvNewReq.FullDetails : null,
                                FromPeriod = item.FromPeriod != null ? item.FromPeriod.Value.ToString("dd/MM/yyyy") : null,
                                ToPeriod = item.ToPeriod != null ? item.ToPeriod.Value.ToString("dd/MM/yyyy") : null,
                                EncashDays = item.EncashDays,
                                Narration = item.Narration,

                            });

                        }
                        return Json(returndata, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return null;
                    }

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
        }
        public ActionResult GetLeaveBalance(string LvHeadId, string EmpId) //Pass leavehead id here
        {
            using (DataBaseContext db = new DataBaseContext())
            {
                int LvHead = int.Parse(LvHeadId);
                int Emp = int.Parse(EmpId);
                var LvCalendar = db.Calendar.Include(e => e.Name).Where(e => e.Name.LookupVal.ToUpper() == "LEAVECALENDAR" && e.Default == true).SingleOrDefault();
                var OEmployeeLeave = db.EmployeeLeave.Include(e => e.LvNewReq)
                    .Include(e => e.LvNewReq.Select(a => a.LeaveHead))
                    .Include(e => e.LvOpenBal.Select(a => a.LvHead))
                    .Include(e => e.LvOpenBal.Select(a => a.LvCalendar))
                    .Include(e => e.LvNewReq.Select(a => a.LeaveCalendar))
                    .Include(e => e.LvNewReq.Select(a => a.WFStatus))
                    .Where(e => e.Employee.Id == Emp).SingleOrDefault();
                var openbal = OEmployeeLeave.LvOpenBal.Where(e => e.LvHead.Id == LvHead && e.LvCalendar.Id == LvCalendar.Id).LastOrDefault();

                // Leave calendar id default year checking discard and when enter new leave open balance at that time lvnewreq table data auto save
                //so openbal table checking not required
                var openballvnewreq = OEmployeeLeave.LvNewReq.Where(e => e.LeaveHead.Id == LvHead).OrderByDescending(e => e.Id).FirstOrDefault();
                var PrevReq1 = OEmployeeLeave.LvNewReq.Where(e => e.LeaveHead.Id == LvHead)
               .OrderByDescending(e => e.Id).Select(e => new { LvOpening = openballvnewreq.OpenBal + openballvnewreq.CreditDays - openballvnewreq.LvLapsed, LvClosing = openballvnewreq.CloseBal, LvOccurances = e.LVCount }).FirstOrDefault();
                if (PrevReq1 != null)
                {
                    return Json(PrevReq1, JsonRequestBehavior.AllowGet);

                }

                //if (openbal == null)
                //{
                //    var openballvnewreq = OEmployeeLeave.LvNewReq.Where(e => e.LeaveHead.Id == LvHead && e.LeaveCalendar.Id == LvCalendar.Id ).OrderByDescending(e => e.Id).FirstOrDefault();
                //    var PrevReq1 = OEmployeeLeave.LvNewReq.Where(e => e.LeaveHead.Id == LvHead && e.LeaveCalendar.Id == LvCalendar.Id 

                //   )
                //   .OrderByDescending(e => e.Id).Select(e => new { LvOpening = openballvnewreq.OpenBal + openballvnewreq.CreditDays-openballvnewreq.LvLapsed, LvClosing = openballvnewreq.CloseBal, LvOccurances = e.LVCount }).FirstOrDefault();
                //    return Json(PrevReq1, JsonRequestBehavior.AllowGet);
                //}
                //else
                //{
                //    var PrevReq = OEmployeeLeave.LvNewReq.Where(e => e.LeaveHead.Id == LvHead && e.LeaveCalendar.Id == LvCalendar.Id
                //        //&& e.IsCancel == false
                //   )
                //   .OrderByDescending(e => e.Id).Select(e => new { LvOpening = openbal.LvOpening, LvClosing = e.CloseBal, LvOccurances = e.LVCount }).FirstOrDefault();
                //    return Json(PrevReq, JsonRequestBehavior.AllowGet);
                //    if (PrevReq != null)
                //    {
                //        var PrevOpenBal = OEmployeeLeave.LvOpenBal.Where(e => e.LvHead.Id == LvHead && e.LvCalendar.Id == LvCalendar.Id).SingleOrDefault();

                //        return Json(PrevOpenBal, JsonRequestBehavior.AllowGet);
                //    }
                //}


            }
            return null;
        }
    }
}